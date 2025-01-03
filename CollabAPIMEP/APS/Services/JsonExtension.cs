using Newtonsoft.Json;

namespace CollabAPIMEP.APS
{
    public static class JsonExtension
    {
        public static string SerializeObject<T>(this T value)
        {
            if (value is null)
                return null;

            return JsonConvert.SerializeObject(value);
        }

        public static T DeserializeObject<T>(this string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch
            {
                return default;
            }
        }
    }
}