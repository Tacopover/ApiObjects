using Autodesk.Authentication.Model;
using System.Diagnostics;
using System.IO;

namespace CollabAPIMEP.APS
{
    public static class ThreeLeggedTokenExtension
    {
        private static string FileName => GetThreeLeggedTokenFileName();
        private static string GetThreeLeggedTokenFileName()
        {
            var userDataFolderName = typeof(ThreeLeggedTokenExtension).Assembly.GetName().Name;
            var fileName = $"{userDataFolderName}.json";
            var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), userDataFolderName);
            Directory.CreateDirectory(userDataFolder);
            return Path.Combine(userDataFolder, fileName);
        }

        public static ThreeLeggedToken Save(this ThreeLeggedToken threeLeggedToken)
        {
            Debug.WriteLine($"Save: {threeLeggedToken?.GetHashCode()}");

            if (threeLeggedToken is null)
            {
                if (File.Exists(FileName))
                    File.Delete(FileName);

                return null;
            }

            var json = threeLeggedToken.SerializeObject();
            File.WriteAllText(FileName, json);
            return threeLeggedToken;
        }

        public static ThreeLeggedToken Load(this ThreeLeggedToken threeLeggedToken)
        {
            if (threeLeggedToken is null)
            {
                if (File.Exists(FileName))
                {
                    var json = File.ReadAllText(FileName);
                    threeLeggedToken = json.DeserializeObject<ThreeLeggedToken>();
                }
            }
            return threeLeggedToken;
        }
    }
}