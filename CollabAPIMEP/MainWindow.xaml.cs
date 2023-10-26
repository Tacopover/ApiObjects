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
        }
        private void DisableFamLoaded_Click(object sender, RoutedEventArgs e)
        {
            familyLoadHandler.DisableFamilyLoader();
        }
    }
}
