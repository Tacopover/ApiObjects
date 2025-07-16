using System.Collections.Generic;

namespace FamilyAuditorCore.Models
{
    /// <summary>
    /// Represents family information extracted from a Revit family document,
    /// without any direct dependencies on Revit API objects.
    /// </summary>
    public class FamilyInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public int ParameterCount { get; set; }
        public int ElementCount { get; set; }
        public int ImportedInstanceCount { get; set; }
        public int MaterialCount { get; set; }
        public int DetailLineCount { get; set; }
        public int VertexCount { get; set; }
        public bool HasElementsWithoutSubcategory { get; set; }
        
        /// <summary>
        /// Additional properties that can be checked against rules
        /// </summary>
        public Dictionary<string, object> CustomProperties { get; set; } = new Dictionary<string, object>();
    }
}