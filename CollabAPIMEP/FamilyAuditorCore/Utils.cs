using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FamilyAuditorCore
{
    public static class Utils
    {
        public static System.Windows.Media.Imaging.BitmapImage LoadEmbeddedImage(string imagePath)
        {
            var img = new System.Windows.Media.Imaging.BitmapImage();
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(x => x.Contains(imagePath));
                System.IO.Stream stream = assembly.GetManifestResourceStream(resourceName);
                img.BeginInit();
                img.StreamSource = stream;
                img.EndInit();
            }
            catch
            {
                // Handle any exceptions or error logging here
            }
            return img;
        }
    }
}
