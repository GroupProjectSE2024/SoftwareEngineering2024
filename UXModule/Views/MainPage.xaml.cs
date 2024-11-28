using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using ViewModel;
using Screenshare;
using WhiteboardGUI;

namespace UXModule.Views;

/// <summary>
/// Interaction logic for MainPage.xaml
/// </summary>
public partial class MainPage : Page
{
    // Static instances for different pages
    private static DashboardPage dashboardPage;
    private static WhiteboardPage whiteboardPage;
    private static UpdaterPage updaterPage;
    private static FileCloner fileClonerPage;
    private static ScreensharePage screensharePage;
    private static AnalyserPage analyserPage;
    private static ChatPage chatPage;
    private static UploadPage uploadPage;

    // Fields for session type, current page, and chat state
    private readonly string sessionType;
    private static Page _currentPage;
    private bool chatOn = true;

    // Event for property changes
    public event PropertyChangingEventHandler? PropertyChanged;

    /// <summary>
    /// Constructor for MainPage
    /// </summary>
    /// <param name="_sessionType">Session type (server or client)</param>
    /// <param name="mainPageViewModel">View model for MainPage</param>
    /// <param name="currentPage">Current page to display</param>
    public MainPage(string _sessionType, ViewModel.DashboardViewModel.MainPageViewModel mainPageViewModel, Page currentPage)
    {
        InitializeComponent(); // Initialize components
        sessionType = _sessionType; // Store session type

        _currentPage = currentPage; // Set current page
        updaterPage = new UpdaterPage(sessionType);
        Main.Content = _currentPage;  // Set the initial content

        fileClonerPage = new FileCloner(); 
    }
    /// <summary>
    /// Navigate to Dashboard
    /// </summary>
    private void DashboardClick(object sender, RoutedEventArgs e)
    {
   
           Main.Content = _currentPage; // Display the current page
    }

    /// <summary>
    /// Navigate to Whiteboard
    /// </summary>
    private void WhiteboardClick(object sender, RoutedEventArgs e)
    {
        whiteboardPage ??= new WhiteboardPage(); // Create new instance of WhiteboardPage
        Main.Content = whiteboardPage; // Set WhiteboardPage as content
    }

    /// <summary>
    /// Navigate to FileCloner
    /// </summary>
    private void FileClonerClick(object sender, RoutedEventArgs e)
    {
        Main.Content = fileClonerPage; // Set FileClonerPage as content
    }

    /// <summary>
    /// Navigate to Updater
    /// </summary>
    private void UpdaterClick(object sender, RoutedEventArgs e)
    {
        Main.Content = updaterPage; // Set UpdaterPage as content

    }

    /// <summary>
    /// Navigate to Analyser
    /// </summary>
    private void AnalyserClick(object sender, RoutedEventArgs e)
    {
        analyserPage = new AnalyserPage();  // Create new instance of AnalyserPage
        Main.Content = analyserPage; // Set AnalyserPage as content

    }

    /// <summary>
    /// Navigate to Screenshare
    /// </summary>
    private void ScreenShareClick(object sender, RoutedEventArgs e)
    {
        // Navigate based on session type
        if (sessionType == "server")
        {
            Main.Content = new ScreenShareServer(); // Server mode

        }
        else
        {
            Main.Content = new ScreenShareClient(); // Client mode
        }

    }

    /// <summary>
    /// Toggle Chat Panel
    /// </summary>
    private void ChatButtonClick(object sender, RoutedEventArgs e)
    {
        if (!chatOn)
        {
            // Enable chat panel
            chatOn = true;
            ChatScreen.Visibility = Visibility.Visible; // Show chat panel
            ChatColumn.Width = new GridLength(300); // Set chat panel width
            MainColumn.Width = new GridLength(1,GridUnitType.Star); // Adjust main content width
            ChatScreen.Content = new ChatPage(); // Load chat page
        }
        else
        {
            // Disable chat panel
            chatOn = false;
            ChatScreen.Visibility = Visibility.Collapsed; // Hide chat panel
            ChatColumn.Width = new GridLength(0); // Collapse chat panel
            MainColumn.Width = new GridLength(1, GridUnitType.Star); // Adjust main content width
            ChatScreen.Content = null; // Clear chat content
        }

    }

    /// <summary>
    /// Navigate to Upload
    /// </summary>
    private void UploadClick(object sender, RoutedEventArgs e)
    {
        uploadPage = new UploadPage(); // Create new instance of UploadPage
        Main.Content = uploadPage; // Set UploadPage as content

    }
}
