using System.Windows;

namespace CollabAPIMEP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FamilyLoadHandler familyLoadHandler;
        public MainWindow(FamilyLoadHandler famHandler)
        {
            InitializeComponent();
            familyLoadHandler = famHandler;
        }

        private void EnableFamLoaded_Click(object sender, RoutedEventArgs e)
        {
            familyLoadHandler.EnableFamilyLoader();
            LoaderStateText.Text = "Enabled";
        }
        private void DisableFamLoaded_Click(object sender, RoutedEventArgs e)
        {
            familyLoadHandler.DisableFamilyLoader();
            LoaderStateText.Text = "Disabled";
        }
    }
}
