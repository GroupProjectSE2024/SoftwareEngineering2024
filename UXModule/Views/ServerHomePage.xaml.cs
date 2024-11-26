using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViewModel.DashboardViewModel;

namespace UXModule.Views
{
    /// <summary>
    /// Interaction logic for ServerHomePage.xaml
    /// </summary>
    public partial class ServerHomePage : Page
    {
        private readonly MainPageViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the ServerHomePage class.
        /// </summary>
        /// <param name="viewModel">The view model for the main page.</param>
        public ServerHomePage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        /// <summary>
        /// Handles the click event of the button to stop the server session.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool response = _viewModel.ServerStopSession();
            if (response)
            {
                NavigationService.GoBack();
            }
        }
    }
}