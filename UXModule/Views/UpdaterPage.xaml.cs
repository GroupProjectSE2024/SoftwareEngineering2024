/****************************************************************************** 
 * Filename    = UpdaterPage.xaml.cs 
 * 
 * Author      = Updater Team 
 * 
 * Product     = UI 
 * 
 * Project     = Views 
 * 
 * Description = Initialize a page for Updater 
 *****************************************************************************/

using System.IO;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using Updater;
using ViewModel.UpdaterViewModel;

namespace UXModule.Views;


/// <summary> 
/// Interaction logic for UpdaterPage.xaml 
/// </summary> 
public partial class UpdaterPage : Page
{
    public LogServiceViewModel LogServiceViewModel { get; }
    private readonly FileChangeNotifier _analyzerNotificationService;
    private readonly ToolListViewModel _toolListViewModel;
    private static CloudViewModel? s_cloudViewModel;
    private static ServerViewModel? s_serverViewModel; // Added server view model 
    private static ClientViewModel? s_clientViewModel; // Added client view model 
    private readonly ToolAssemblyLoader _loader;

    private readonly string _sessionType;
    public UpdaterPage(string sessionType)
    {
        InitializeComponent();

        _toolListViewModel = new ToolListViewModel();
        _toolListViewModel.LoadAvailableTools();

        ListView listView = (ListView)this.FindName("ToolViewList");
        listView.DataContext = _toolListViewModel;

        _analyzerNotificationService = new FileChangeNotifier();
        _analyzerNotificationService.MessageReceived += OnMessageReceived;

        LogServiceViewModel = new LogServiceViewModel();
        _loader = new ToolAssemblyLoader();

        _sessionType = sessionType;



        if (sessionType != "server")
        {
            s_clientViewModel = new ClientViewModel(LogServiceViewModel); // Initialize the client view model 
            CloudSyncButton.Visibility = Visibility.Hidden;
        }
        else
        {
            s_serverViewModel = new ServerViewModel(LogServiceViewModel, _loader); // Initialize the server view model 
            s_cloudViewModel = new CloudViewModel(LogServiceViewModel, s_serverViewModel);
        }

        this.DataContext = LogServiceViewModel;

    }

    private void OnMessageReceived(string message)
    {
        Dispatcher.Invoke(() => {
            _toolListViewModel.LoadAvailableTools(); // Refresh the tool list on message receipt 
            LogServiceViewModel.ShowNotification(message); // Show received message as a notification 
            LogServiceViewModel.UpdateLogDetails(message); // Update log with received message 
        });
    }

    private async void SyncButtonClick(object sender, RoutedEventArgs e)
    {
        if (s_clientViewModel != null)
        {
            if (_sessionType != "server")
            {
                try
                {
                    Dispatcher.Invoke(() => LogServiceViewModel.UpdateLogDetails("Initiating sync with the server..."));
                    await s_clientViewModel.SyncUpAsync(); // Call the sync method on the ViewModel
                }
                catch (Exception)
                {
                    Dispatcher.Invoke(() => LogServiceViewModel.UpdateLogDetails("Client is not connected. Please connect first."));
                }
            }
        }
        else
        {
            // Open File Dialog
            OpenFileDialog openFileDialog = new OpenFileDialog {
                Title = "Select a File to Upload",
                Filter = "All Files (*.*)|*.*",
                Multiselect = false // Allow selecting only one file
            };

            if (s_serverViewModel != null)
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFilePath = openFileDialog.FileName; // Get the selected file path
                    string selectedFileName = Path.GetFileName(selectedFilePath); // Extract the file name from the path

                    s_serverViewModel.BroadcastToClients(selectedFilePath, selectedFileName);
                }
            }
        }
    }

    private async void SyncCloudButtonClick(object sender, RoutedEventArgs e)
    {
        // Disable the Sync button to prevent multiple syncs at the same time
        Dispatcher.Invoke(() => CloudSyncButton.IsEnabled = false);
        if (s_cloudViewModel != null)
        {
            if (_sessionType == "server")
            {
                try
                {
                    Dispatcher.Invoke(() => LogServiceViewModel.UpdateLogDetails("Server is running. Starting cloud sync."));

                    // Perform cloud sync asynchronously
                    await s_cloudViewModel.PerformCloudSync();

                    Dispatcher.Invoke(() => LogServiceViewModel.UpdateLogDetails("Cloud sync completed."));
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => LogServiceViewModel.UpdateLogDetails($"Error during cloud sync: {ex.Message}"));
                }
                finally
                {
                    Dispatcher.Invoke(() => CloudSyncButton.IsEnabled = true); // Re-enable Sync button
                }
            }
        }
    }

    private void ClosePopup(object sender, RoutedEventArgs e)
    {
        Dispatcher.Invoke(() => LogServiceViewModel.NotificationVisible = false);
    }
}
