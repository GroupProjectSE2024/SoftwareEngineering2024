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

namespace ViewModel.DashboardViewModel
{
    /// <summary>
    /// Model representing data for the graph.
    /// </summary>
    public class GraphDataModel
    {
        public DateTime Time { get; set; }
        public int UserCount { get; set; }
    }

    public class MainPageViewModel : INotifyPropertyChanged
    {
        private ICommunicator _communicator;
        private ServerDashboard _serverSessionManager;
        private ClientDashboard _clientSessionManager;

        // UserDetailsList is bound to the UI to display the participant list
        private ObservableCollection<UserDetails> _userDetailsList = new ObservableCollection<UserDetails>();
        public ObservableCollection<UserDetails> UserDetailsList
        {
            get => _userDetailsList;
            set
            {
                if (_userDetailsList != value)
                {
                    _userDetailsList = value;
                    OnPropertyChanged(nameof(UserDetailsList));
                }
            }
        }

        private ObservableCollection<GraphDataModel> _userCountData;
        public ObservableCollection<GraphDataModel> UserCountData
        {
            get => _userCountData;
            private set
            {
                _userCountData = value;
                OnPropertyChanged(nameof(UserCountData));
            }
        }

        public SeriesCollection SeriesCollection { get; private set; }

        private List<string> _timeLabels;
        public List<string> TimeLabels
        {
            get => _timeLabels;
            set
            {
                _timeLabels = value;
                OnPropertyChanged(nameof(TimeLabels));
            }
        }

        private double _axisMax;
        private double _axisMin;
        public double AxisMax
        {
            get => _axisMax;
            set
            {
                _axisMax = value;
                OnPropertyChanged(nameof(AxisMax));
            }
        }
        public double AxisMin
        {
            get => _axisMin;
            set
            {
                _axisMin = value;
                OnPropertyChanged(nameof(AxisMin));
            }
        }

        // Add property for max Y value
        private int _maxYValue = 3;  // Default value
        public int MaxYValue
        {
            get => _maxYValue;
            set
            {
                _maxYValue = value;
                OnPropertyChanged(nameof(MaxYValue));
            }
        }

        private DispatcherTimer _timer;

        private int _currentUserCount;
        public int CurrentUserCount
        {
            get => _currentUserCount;
            set
            {
                if (_currentUserCount != value)
                {
                    _currentUserCount = value;
                    OnPropertyChanged(nameof(CurrentUserCount));
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
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public string? ProfilePicUrl
        {
            get => _profilePicUrl;
            set
            {
                _profilePicUrl = value;
                OnPropertyChanged(nameof(ProfilePicUrl));
            }
        }

        public string? UserEmail { get; private set; }

        public string? ServerIP
        {
            get => _serverIP;
            set
            {
                _serverIP = value;
                OnPropertyChanged(nameof(ServerIP));
            }
        }

        public string ServerPort
        {
            get => _serverPort;
            set
            {
                if (_serverPort != value)
                {
                    _serverPort = value;
                    OnPropertyChanged(nameof(ServerPort));
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

        /// <summary>
        /// Stops the server session.
        /// </summary>
        /// <returns>Returns true if the session is stopped successfully, otherwise false.</returns>
        public bool ServerStopSession()
        {
            return _serverSessionManager.ServerStop();
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
            if (e.PropertyName == nameof(ServerDashboard.ServerUserList) || e.PropertyName == nameof(ClientDashboard.ClientUserList))
            {
                var users = _serverSessionManager?.ServerUserList ?? _clientSessionManager?.ClientUserList;

                if (users != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UserDetailsList.Clear();
                        foreach (var user in users)
                        {
                            UserDetailsList.Add(user);
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Initializes the graph.
        /// </summary>
        private void InitializeGraph()
        {
            _userCountData = new ObservableCollection<GraphDataModel>();
            _timeLabels = new List<string>();

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

            AxisMax = DateTime.Now.Ticks + TimeSpan.FromSeconds(1).Ticks;
            AxisMin = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Updates the user count graph.
        /// </summary>
        private void UpdateUserCountGraph()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var now = DateTime.Now;
                var currentCount = UserDetailsList.Count;

                CurrentUserCount = currentCount;

                // Ensure we are adding ObservableValue to the ChartValues
                SeriesCollection[0].Values.Add(new ObservableValue(currentCount));
                TimeLabels.Add(now.ToString("hh:mm"));

                if (currentCount + 2 > MaxYValue)
                {
                    MaxYValue = currentCount + 2;
                }

                OnPropertyChanged(nameof(SeriesCollection));
                OnPropertyChanged(nameof(TimeLabels));
            });
        }

        /// <summary>
        /// Sets up the timer for updating the graph.
        /// </summary>
        public void SetupTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += TimerOnTick;
            _timer.Start();
        }

        /// <summary>
        /// Tick event handler for the timer.
        /// </summary>
        public void TimerOnTick(object sender, EventArgs e)
        {
            UpdateUserCountGraph();
        }

        /// <summary>
        /// Cleans up resources.
        /// </summary>
        public void Cleanup()
        {
            _timer?.Stop();
        }
    }
}