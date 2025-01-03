using System.Windows;

namespace CollabAPIMEP.APS
{
    public partial class MainWindow_APS : Window
    {
        private static string ClientId = "680LcYsfUK8oOJv9pr0Y0KIJNKgzowhF1AieLR0F8KVr0reC";
        private static string CallbackUrl = "http://localhost:8080/";

        APS_Service ApsService = new APS_Service(ClientId, CallbackUrl);

        public MainWindow_APS()
        {
            InitializeComponent();
            DataContext = new MainViewModel(this, ApsService);
        }
    }
}