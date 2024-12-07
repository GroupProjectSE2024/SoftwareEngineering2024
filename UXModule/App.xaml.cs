using System.Diagnostics;
using System.Windows;

namespace UXModule;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize the TraceLogger
        string logFilePath = TraceLogger.GetLogFilePath();

        // Show the log file path in a MessageBox
        //MessageBox.Show($"Log file location: {logFilePath}", "Log File Path", MessageBoxButton.OK, MessageBoxImage.Information);

        // Log application startup
        Trace.WriteLine("Application started.");

        // Your application's main logic here
        Trace.WriteLine("Performing some operations...");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Log application exit
        Trace.WriteLine("Application exiting.");

        base.OnExit(e);
    }
}
