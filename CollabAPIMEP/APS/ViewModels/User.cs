
using CommunityToolkit.Mvvm.ComponentModel;


namespace CollabAPIMEP.APS
{
    public class User : ObservableObject
    {
        public string Name { get; set; }
        public string Image { get; set; }
    }
}