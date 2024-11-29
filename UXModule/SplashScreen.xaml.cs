using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using UXModule.Views;

namespace UXModule;

public partial class SplashScreen : Window
{
    private readonly DispatcherTimer timer;

    public SplashScreen(DispatcherTimer timer)
    {
        this.timer = timer;
    }

    public SplashScreen()
    {
        InitializeComponent();
        this.Title = "Synapse";
        // Initialize and start the timer for 4 minutes
        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(3); // 4 minutes
        timer.Tick += Timer_Tick;
        timer.Start();

        // Start animations
        StartAnimations();
    }

    private void StartAnimations()
    {
        // Use FindName to locate the SynapseStoryboard directly
        var synapseStoryboard = (Storyboard)this.FindName("SynapseStoryboard");
        synapseStoryboard?.Begin(this, true);

        // Start each unique floating circle animation storyboard
        for (int i = 1; i <= 70; i++)
        {
            // Properly format the storyboard name and cast to Storyboard
            var circleStoryboard = this.FindName($"FloatingCirclesStoryboard{i}") as Storyboard;
            circleStoryboard?.Begin(this, true);
        }
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        StopAnimations();
        timer.Stop();
        OpenNextWindow();
        this.Close();   

    }

    private void StopAnimations()
    {
        // Stop the main SynapseStoryboard
        var synapseStoryboard = (Storyboard)this.FindName("SynapseStoryboard");
        synapseStoryboard?.Stop(this);

        // Stop each unique floating circle animation storyboard
        for (int i = 1; i <= 70; i++)
        {
            var circleStoryboard = this.FindName($"FloatingCirclesStoryboard{i}") as Storyboard;
            circleStoryboard?.Stop(this);
        }
    }
    private void OpenNextWindow()
    {
        // Open the main application window
        var mainWindow = new MainWindow();
        mainWindow.Show();
    }
}
