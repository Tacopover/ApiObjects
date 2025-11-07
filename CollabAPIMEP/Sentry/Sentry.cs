using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sentry;
using Autodesk.Revit.DB;
using AttachmentType = Sentry.AttachmentType;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Cascad
{
    class Sentry
    {
        public static ISpan CurrentChild = null;
        public static ITransactionTracer CurrentTransaction = null;

        // ===== Tracking state =====
        private static readonly object _trackSync = new object();
        private static readonly List<TrackedEntry> _trackedObjects = new List<TrackedEntry>();
        private static int _maxTrackedObjects = 500;

        // Keep the SDK handle so we can dispose it on shutdown
        private static IDisposable _sdk;

        private sealed class TrackedEntry
        {
            public string Name { get; set; }
            public object Instance { get; set; }
        }

        public sealed class PluginInfo
        {
            public string CommandName { get; set; }
            public string Operation { get; set; }
            public string CommandType { get; set; }
            public DateTime StartUtc { get; set; }
            public DateTime? EndUtc { get; set; }
            public bool? Succeeded { get; set; }
            public string ErrorMessage { get; set; }
        }

        private static RevitProjectInfo _currentProjectInfo;
        private static PluginInfo _currentPluginInfo;

        static public void Initiate(string dsn)
        {
            // If already initialized, dispose the previous instance first
            _sdk?.Dispose();

            _sdk = SentrySdk.Init(o =>
            {
                o.Dsn = dsn;
                o.Debug = true;
                o.TracesSampleRate = 0;      // Disable tracing unless you want it
                o.AttachStacktrace = true;
                o.AutoSessionTracking = false;
            });
        }

        // Gracefully flush and shutdown Sentry
        static public void Shutdown(int flushSeconds = 2)
        {
            try
            {
                if (flushSeconds > 0)
                {
                    SentrySdk.FlushAsync(TimeSpan.FromSeconds(flushSeconds))
                        .GetAwaiter().GetResult();
                }
            }
            catch
            {
                // Swallow flush failures on shutdown
            }
            finally
            {
                _sdk?.Dispose();
                _sdk = null;
                CurrentChild = null;
                CurrentTransaction = null;
            }
        }

        // ---------- Revit Project Context ----------

        public sealed class RevitProjectInfo
        {
            public string Title { get; set; }
            public string Path { get; set; }
            public string ProjectNumber { get; set; }
            public string ProjectName { get; set; }
            public string ClientName { get; set; }
            public string Status { get; set; }
            public string Address { get; set; }
            public string BuildingName { get; set; }
            public string OrganizationName { get; set; }
            public string OrganizationDescription { get; set; }
            public string Username { get; set; }
            public string RevitVersion { get; set; }
            public string CentralModelPath { get; set; }
            public bool IsWorkshared { get; set; }
            public string ProjectInfoElementId { get; set; }
        }

        private static string GetParamAsString(Element e, BuiltInParameter bip)
            => e?.get_Parameter(bip)?.AsString();

        public static RevitProjectInfo BuildRevitProjectInfo(Document doc)
        {
            var pi = doc.ProjectInformation;
            var app = doc.Application;

            string central = null;
            try
            {
                if (doc.IsWorkshared)
                {
                    var cm = doc.GetWorksharingCentralModelPath();
                    central = cm != null ? ModelPathUtils.ConvertModelPathToUserVisiblePath(cm) : null;
                }
            }
            catch { /* ignore */ }

            return new RevitProjectInfo
            {
                Title = doc.Title,
                Path = doc.PathName,
                ProjectNumber = GetParamAsString(pi, BuiltInParameter.PROJECT_NUMBER),
                ProjectName = GetParamAsString(pi, BuiltInParameter.PROJECT_NAME),
                ClientName = GetParamAsString(pi, BuiltInParameter.CLIENT_NAME),
                Status = GetParamAsString(pi, BuiltInParameter.PROJECT_STATUS),
                Address = GetParamAsString(pi, BuiltInParameter.PROJECT_ADDRESS),
                BuildingName = GetParamAsString(pi, BuiltInParameter.PROJECT_BUILDING_NAME),
                OrganizationName = GetParamAsString(pi, BuiltInParameter.PROJECT_ORGANIZATION_NAME),
                OrganizationDescription = GetParamAsString(pi, BuiltInParameter.PROJECT_ORGANIZATION_DESCRIPTION),
                Username = app.Username,
                RevitVersion = app.VersionName,
                CentralModelPath = central,
                IsWorkshared = doc.IsWorkshared,
                ProjectInfoElementId = pi?.Id?.IntegerValue.ToString()
            };
        }

        public static void SetRevitProjectContext(Document doc, bool attachJson = false)
        {
            var info = BuildRevitProjectInfo(doc);

            SentrySdk.ConfigureScope(scope =>
            {
                if (!string.IsNullOrEmpty(info.ProjectNumber))
                    scope.SetTag("revit.project_number", info.ProjectNumber);
                if (!string.IsNullOrEmpty(info.ProjectName))
                    scope.SetTag("revit.project_name", info.ProjectName);
                if (!string.IsNullOrEmpty(info.Title))
                    scope.SetTag("revit.title", info.Title);

                scope.SetExtra("revit.project", info);
            });

            if (attachJson)
            {
                AttachJson("revit-project.json", info);
            }
        }

        private static readonly string[] DangerousNamespaces = new[]
        {
        "Autodesk.Revit.DB.Geometry",
        "Autodesk.Revit.DB.Structure", 
        "Autodesk.Revit.DB.Analysis"
        };




        public static object SafeSerialize(object obj, int depth, HashSet<object> visited)
        {
            if (obj == null)
                return null;

            if (depth <= 0)
                return obj.ToString();

            if (visited == null)
                visited = new HashSet<object>(new ReferenceEqualityComparer());

            if (visited.Contains(obj))
                return "[Cyclic Reference]";

            visited.Add(obj);

            Type type = obj.GetType();

            if (type.IsPrimitive || obj is string || obj is Guid || obj is Enum)
                return obj;

            if (obj is ElementId)
                return ((ElementId)obj).IntegerValue;

            if (obj is XYZ)
            {
                XYZ xyz = (XYZ)obj;
                return new { X = xyz.X, Y = xyz.Y, Z = xyz.Z };
            }

            if (type.Namespace != null)
            {
                foreach (string ns in DangerousNamespaces)
                {
                    if (type.Namespace.StartsWith(ns))
                        return "[" + type.Name + "]";
                }
            }

            if (obj is IEnumerable)
            {
                var list = new List<object>();
                foreach (object item in (IEnumerable)obj)
                    list.Add(SafeSerialize(item, depth - 1, visited));
                return list;
            }

            // For regular objects
            var dict = new Dictionary<string, object>();

            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in props)
            {
                try
                {
                    if (!p.CanRead)
                        continue;

                    Type propType = p.PropertyType;
                    if (propType.Namespace != null)
                    {
                        bool skip = false;
                        foreach (string ns in DangerousNamespaces)
                        {
                            if (propType.Namespace.StartsWith(ns))
                            {
                                skip = true;
                                break;
                            }
                        }
                        if (skip)
                            continue;
                    }

                    object val = p.GetValue(obj, null);
                    dict[p.Name] = SafeSerialize(val, depth - 1, visited);
                }
                catch
                {
                    // Skip if property throws
                }
            }

            return dict;
        }

        public static string ToJson(object obj, int depth)
        {
            object safeObj = SafeSerialize(obj, depth, null);

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;

            return JsonSerializer.Serialize(safeObj, options);
        }

        // Reference comparer to detect cycles
        private class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static ReferenceEqualityComparer Instance { get; } = new ReferenceEqualityComparer();
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }


        public static void AttachJson(string fileName, object data)
        {
            if (data == null) return;

            string json;
            try
            {
                json = System.Text.Json.JsonSerializer.Serialize(
                    data,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    });
            }
            catch
            {
                json = data.ToString() ?? string.Empty;
            }

            var bytes = Encoding.UTF8.GetBytes(json);
            var stream = new MemoryStream(bytes) { Position = 0 };

            SentrySdk.ConfigureScope(scope =>
            {
                var attachment = new SentryAttachment(
                    AttachmentType.Default,
                    new StreamAttachmentContent(stream),
                    fileName,
                    "application/json");

                scope.AddAttachment(attachment);
            });
        }

        // ---------- Tracking API (named objects) ----------

        // Resets tracking and sets up plugin info (called at command start)
        static public void StartTransaction(Document doc, string name, string operation)
        {
            _currentProjectInfo = BuildRevitProjectInfo(doc);
            _currentPluginInfo = new PluginInfo
            {
                CommandName = name,
                Operation = operation,
                CommandType = null,
                StartUtc = DateTime.UtcNow
            };

            lock (_trackSync)
            {
                _trackedObjects.Clear();
                _maxTrackedObjects = Math.Max(50, 500);
            }

            SetRevitProjectContext(doc, attachJson: false);
            CurrentTransaction = SentrySdk.StartTransaction(name, operation);
            CurrentChild = CurrentTransaction.StartChild(name);
        }

        static public void FinishTransaction()
        {
            CurrentChild?.Finish();
            CurrentTransaction?.Finish();

            if (_currentPluginInfo != null && _currentPluginInfo.EndUtc == null)
            {
                _currentPluginInfo.EndUtc = DateTime.UtcNow;
                _currentPluginInfo.Succeeded = true;
            }
        }

        // Track with an explicit name (append by default)
        public static void TrackObject(string name, object instance)
            => TrackObject(name, instance, overwrite: false);

        // Track with an explicit name, optionally overwriting any prior entries with the same name
        public static void TrackObject(string name, object instance, bool overwrite)
        {
            if (instance == null) return;
            if (string.IsNullOrWhiteSpace(name))
                name = instance.GetType().FullName;

            var entry = new TrackedEntry { Name = name, Instance = instance };

            lock (_trackSync)
            {
                if (overwrite)
                {
                    // Remove all existing entries with the same name to keep the latest only
                    for (int i = _trackedObjects.Count - 1; i >= 0; i--)
                    {
                        if (string.Equals(_trackedObjects[i].Name, name, StringComparison.Ordinal))
                            _trackedObjects.RemoveAt(i);
                    }
                }

                _trackedObjects.Add(entry);

                var overflow = _trackedObjects.Count - _maxTrackedObjects;
                if (overflow > 0)
                    _trackedObjects.RemoveRange(0, overflow);
            }
        }


        // Send one payload { projectinfo, plugininfo, trackedobjects:[{name, data}] } on error
        public static void CaptureExceptionWithState(Exception ex, string fileName = "state.json")
        {
            if (_currentPluginInfo != null)
            {
                _currentPluginInfo.EndUtc = DateTime.UtcNow;
                _currentPluginInfo.Succeeded = false;
                _currentPluginInfo.ErrorMessage = ex?.Message;
            }

            SentrySdk.ConfigureScope(scope =>
            {
                object payload;
                List<string> names;
                int count;
                List<object> safeTracked;

                lock (_trackSync)
                {
                    names = _trackedObjects.ConvertAll(te => te.Name);
                    count = _trackedObjects.Count;

                    // Build expand-able objects without changing the original instances
                    safeTracked = new List<object>(_trackedObjects.Count);
                    foreach (var te in _trackedObjects)
                    {
                        var safe = SafeSerialize(te.Instance, depth: 3, visited: null);
                        safeTracked.Add(new { name = te.Name, data = safe });
                    }

                    payload = new
                    {
                        projectinfo = _currentProjectInfo,
                        plugininfo = _currentPluginInfo,
                        trackedobjects = safeTracked
                    };
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    MaxDepth = 8
                };

                var json = JsonSerializer.Serialize(payload, options);
                var bytes = Encoding.UTF8.GetBytes(json);
                var stream = new MemoryStream(bytes) { Position = 0 };

                var attachment = new SentryAttachment(
                    AttachmentType.Default,
                    new StreamAttachmentContent(stream),
                    fileName,
                    "application/json");

                scope.AddAttachment(attachment);

                // Optional: also add to extras so Sentry UI shows an expandable tree
                scope.SetExtra("tracked.objects", safeTracked);

                scope.SetExtra("tracked.count", count);
                scope.SetExtra("tracked.names", names);
                scope.SetExtra("command.name", _currentPluginInfo?.CommandName);
                scope.SetExtra("command.operation", _currentPluginInfo?.Operation);

                SentrySdk.CaptureException(ex);
            });
        }



    }


}
