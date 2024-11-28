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
    /// Interaction logic for ClientHomePage.xaml
    /// </summary>
    public partial class ClientHomePage : Page
    {
        private readonly MainPageViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the ClientHomePage class.
        /// </summary>
        /// <param name="viewModel">The view model for the client home page.</param>
        public ClientHomePage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        /// <summary>
        /// Handles the click event of the button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The RoutedEventArgs that contains the event data.</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool response = _viewModel.ClientLeaveSession();
            if (response)
            {
                NavigationService.GoBack();
            }
        }
    }
}
