using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using Networking;
using Networking.Communication;
using System.Windows;
using System.Diagnostics;
using Dashboard;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.Text.Json;

namespace ViewModel.DashboardViewModel;

public class MainPageViewModel : INotifyPropertyChanged
{
    private ICommunicator _communicator;
    private ServerDashboard _serverSessionManager;
    private ClientDashboard _clientSessionManager;
    private readonly object _lock = new object();

    // UserDetailsList is bound to the UI to display the participant list
    private ObservableCollection<UserDetails> _userDetailsList = new ObservableCollection<UserDetails>();
    public ObservableCollection<UserDetails> UserDetailsList
    {
        get => _userDetailsList;
        set
        {
            if (_userDetailsList != value)
            {
                lock (_lock)
                {
                    _userDetailsList = value;
                    OnPropertyChanged(nameof(UserDetailsList));
                }
            }
        }
    }

    public SeriesCollection SeriesCollection { get; private set; }

    private List<string> _timeLabels = [];
    public List<string> TimeLabels
    {
        get => _timeLabels;
        set
        {
            lock (_lock)
            {
                _timeLabels = value;
                OnPropertyChanged(nameof(TimeLabels));
            }
        }
    }

    private DispatcherTimer _timer;

    private int _currentUserCount;
    public int CurrentUserCount
    {
        get => _currentUserCount;
        set
        {
            lock (_lock)
            {
                if (_currentUserCount != value)
                {
                    _currentUserCount = value;
                    OnPropertyChanged(nameof(CurrentUserCount));
                }
            }
        }
    }

    public MainPageViewModel()
    {
        _serverPort = string.Empty;
        _serverIP = string.Empty;
        _userName = string.Empty;

        InitializeGraph();
        SetupTimer();
    }

    private string _serverPort;
    private string _serverIP;
    private string _userName;
    private string _profilePicUrl;

    public string? UserName
    {
        get => _userName;
        set
        {
            lock (_lock)
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }
    }

    public string? ProfilePicUrl
    {
        get => _profilePicUrl;
        set
        {
            lock (_lock)
            {
                _profilePicUrl = value;
                OnPropertyChanged(nameof(ProfilePicUrl));
            }
        }
    }

    public string? UserEmail { get; private set; }

    public string? ServerIP
    {
        get => _serverIP;
        set
        {
            lock (_lock)
            {
                _serverIP = value;
                OnPropertyChanged(nameof(ServerIP));
            }
        }
    }

    public string ServerPort
    {
        get => _serverPort;
        set
        {
            lock (_lock)
            {
                if (_serverPort != value)
                {
                    _serverPort = value;
                    OnPropertyChanged(nameof(ServerPort));
                }
            }
        }
    }

    public bool IsHost { get; private set; } = false;

    /// <summary>
    /// Method to create a session as host.
    /// </summary>
    /// <param name="userName">The user's name.</param>
    /// <param name="userEmail">The user's email.</param>
    /// <param name="profilePictureUrl">The URL of the user's profile picture.</param>
    /// <returns>Returns "success" if the session is created successfully, otherwise "failure".</returns>
    public string CreateSession(string userName, string userEmail, string profilePictureUrl)
    {
        lock (_lock)
        {
            IsHost = true;
            UserName = userName;
            ProfilePicUrl = profilePictureUrl ?? string.Empty;
            _communicator = CommunicationFactory.GetCommunicator(isClientSide: false);
            _serverSessionManager = new ServerDashboard(_communicator, userName, userEmail, profilePictureUrl);
            _serverSessionManager.PropertyChanged += UpdateUserListOnPropertyChanged; // Subscribe to PropertyChanged
            string serverCredentials = _serverSessionManager.Initialize();

            if (serverCredentials != "failure")
            {
                string[] parts = serverCredentials.Split(':');
                ServerIP = parts[0];
                ServerPort = parts[1];
                return "success";
            }
            return "failure";
        }
    }

    /// <summary>
    /// Method to join a session as client.
    /// </summary>
    /// <param name="userName">The user's name.</param>
    /// <param name="userEmail">The user's email.</param>
    /// <param name="serverIP">The server IP address.</param>
    /// <param name="serverPort">The server port.</param>
    /// <param name="profilePictureUrl">The URL of the user's profile picture.</param>
    /// <returns>Returns "success" if the session is joined successfully, otherwise "failure".</returns>
    public string JoinSession(string userName, string userEmail, string serverIP, string serverPort, string profilePictureUrl)
    {
        lock (_lock)
        {
            IsHost = false;
            UserName = userName;
            ProfilePicUrl = profilePictureUrl ?? string.Empty;
            _communicator = CommunicationFactory.GetCommunicator();
            _clientSessionManager = new ClientDashboard(_communicator, userName, userEmail, profilePictureUrl);
            _clientSessionManager.PropertyChanged += UpdateUserListOnPropertyChanged; // Subscribe to PropertyChanged
            string serverResponse = _clientSessionManager.Initialize(serverIP, serverPort);

            if (serverResponse == "success")
            {
                UserName = userName;
                UserEmail = userEmail;
                UpdateUserListOnPropertyChanged(this, new PropertyChangedEventArgs(nameof(ClientDashboard.ClientUserList)));
            }
            return serverResponse;
        }
    }

    /// <summary>
    /// Stops the server session.
    /// </summary>
    /// <returns>Returns true if the session is stopped successfully, otherwise false.</returns>
    public bool ServerStopSession()
    {
        // Notify clients and stop the server
        DashboardDetails dashboardMessage = new() {
            Action = Dashboard.Action.ServerEnd,
            Msg = "Meeting Ended"
        };
        string jsonMessage = JsonSerializer.Serialize(dashboardMessage);

        if (_serverSessionManager != null)
        {
            _serverSessionManager.BroadcastMessage(jsonMessage);
            _serverSessionManager.ServerStop();
            _serverSessionManager.PropertyChanged -= UpdateUserListOnPropertyChanged;
            _serverSessionManager = null;  // Clear instance
        }

        if (_clientSessionManager != null)
        {
            _clientSessionManager.PropertyChanged -= UpdateUserListOnPropertyChanged;
            _clientSessionManager = null;  // Clear instance
        }

        // Stop communicator
        if (_communicator != null)
        {
            _communicator.Stop();
            _communicator = null;  // Clear instance
        }

        // Clear user details
        UserDetailsList.Clear();
        IsHost = false;
        ServerIP = string.Empty;
        ServerPort = string.Empty;
        UserName = string.Empty;
        ProfilePicUrl = string.Empty;

        // Dispose of any running timers
        if (_timer != null)
        {
            _timer.Stop();
            _timer = null;
        }

        return true;
    }

    /// <summary>
    /// Leaves the client session.
    /// </summary>
    /// <returns>Returns true if the client leaves the session successfully, otherwise false.</returns>
    public bool ClientLeaveSession()
    {
        return _clientSessionManager.ClientLeft();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string property)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }

    public void UpdateUserListOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        lock (_lock)
        {
            if (e.PropertyName == nameof(ServerDashboard.ServerUserList) || e.PropertyName == nameof(ClientDashboard.ClientUserList))
            {
                ObservableCollection<UserDetails>? users = _serverSessionManager?.ServerUserList ?? _clientSessionManager?.ClientUserList;

                if (users != null)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        UserDetailsList.Clear();
                        foreach (UserDetails user in users)
                        {
                            UserDetailsList.Add(user);
                        }
                    });
                }
            }
        }
    }

    /// <summary>
    /// Initializes the graph.
    /// </summary>
    private void InitializeGraph()
    {
        lock (_lock)
        {
            SeriesCollection = new SeriesCollection
            {
            new LineSeries
            {
                Title = "Users",
                Values = new ChartValues<ObservableValue>(), // Ensure ChartValues<ObservableValue>
                PointGeometry = null,
                LineSmoothness = 0
            }
            };
        }

    }

    /// <summary>
    /// Updates the user count graph.
    /// </summary>
    private void UpdateUserCountGraph()
    {
        lock (_lock)
        {
            Application.Current.Dispatcher.Invoke(() => {
                DateTime now = DateTime.Now;
                int currentCount = UserDetailsList.Count;

                CurrentUserCount = currentCount;
                SeriesCollection[0].Values.Add(new ObservableValue(currentCount));
                TimeLabels.Add(now.ToString("HH:mm:ss"));
                OnPropertyChanged(nameof(TimeLabels));
                OnPropertyChanged(nameof(SeriesCollection));
            });
        }
    }

    /// <summary>
    /// Sets up the timer for updating the graph.
    /// </summary>
    public void SetupTimer()
    {
        lock (_lock)
        {
            _timer = new DispatcherTimer {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += TimerOnTick;
            _timer.Start();
        }
    }

    /// <summary>
    /// Tick event handler for the timer.
    /// </summary>
    public void TimerOnTick(object sender, EventArgs e)
    {
        lock (_lock)
        {
            UpdateUserCountGraph();
        }
    }

    /// <summary>
    /// Cleans up resources.
    /// </summary>
    public void Cleanup()
    {
        _timer?.Stop();
    }
}
