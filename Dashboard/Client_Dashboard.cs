using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Networking;
using Networking.Communication;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;
using Screenshare;
using WhiteboardGUI;
using Content.ChatViewModel;

namespace Dashboard;

/// <summary>
/// Client dashboard class implementing notification handler and property changed notifier.
/// </summary>
public class ClientDashboard : INotificationHandler, INotifyPropertyChanged
{
    private ICommunicator _communicator;

    /// <summary>
    /// Gets or sets the user name.
    /// </summary>
    private string UserName { get; set; }

    /// <summary>
    /// Gets or sets the user email.
    /// </summary>
    private string UserEmail { get; set; }

    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    private string UserID { get; set; }

    /// <summary>
    /// Gets or sets the user profile URL.
    /// </summary>
    private string UserProfileUrl { get; set; }

    /// <summary>
    /// Gets or sets the current user count.
    /// </summary>
    public int CurrentUserCount { get; set; }

    /// <summary>
    /// Gets or sets the client user list.
    /// </summary>
    public ObservableCollection<UserDetails> ClientUserList { get; set; } = new ObservableCollection<UserDetails>();

    private readonly Screenshare.ScreenShareClient.ScreenshareClient _screenShareClient = Screenshare.ScreenShareClient.ScreenshareClient.GetInstance();

    private readonly Updater.Client _updaterClient = Updater.Client.GetClientInstance();
    private readonly FileCloner.Models.Singleton _fileClonerInstance = FileCloner.Models.Singleton.Instance;

    /// <summary>
    /// Initializes a new instance of the Client_Dashboard class.
    /// </summary>
    /// <param name="communicator">Communicator instance.</param>
    /// <param name="username">User name.</param>
    /// <param name="useremail">User email.</param>
    /// <param name="pictureURL">User profile picture URL.</param>
    MainViewModel _contentInstance = MainViewModel.GetInstance;
    public ClientDashboard(ICommunicator communicator, string username, string useremail, string pictureURL)
    {
        _communicator = communicator;
        _communicator.Subscribe("Dashboard", this, isHighPriority: true);
        UserName = username;
        UserEmail = useremail;
        UserProfileUrl = pictureURL;
        _fileClonerInstance.UserName = UserName;
        UserID = string.Empty; // Initialize UserID
        ClientUserList.CollectionChanged += (s, e) => OnPropertyChanged(nameof(ClientUserList));
    }

    /// <summary>
    /// Initializes the dashboard and connects to the server.
    /// </summary>
    /// <param name="serverIP">Server IP address.</param>
    /// <param name="serverPort">Server port number.</param>
    /// <returns>Server response.</returns>
    public string Initialize(string serverIP, string serverPort)
    {
        string server_response = _communicator.Start(serverIP, serverPort);
        Trace.WriteLine("[DashboardClient] client connected to server");
        return server_response;
    }

    /// <summary>
    /// Sends a message to the server.
    /// </summary>
    /// <param name="clientIP">Client IP address.</param>
    /// <param name="message">Message to be sent.</param>
    public void SendMessage(string clientIP, string message)
    {
        string json_message = JsonSerializer.Serialize(message);
        try
        {
            _communicator.Send(json_message, "Dashboard", null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    /// <summary>
    /// Sends user information to the server.
    /// </summary>
    /// <param name="username">User name.</param>
    /// <param name="useremail">User email.</param>
    public void SendInfo(string username, string useremail)
    {
        DashboardDetails details = new DashboardDetails
        {
            User = new UserDetails
            {
                UserName = username,
                UserEmail = useremail,
                ProfilePictureUrl = UserProfileUrl,
                UserId = UserID
            },
            Action = Action.ClientUserConnected
        };
        string json_message = JsonSerializer.Serialize(details);
        try
        {
            _communicator.Send(json_message, "Dashboard", null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
        Trace.WriteLine("[DashboardClient] sent user info to server");

        _screenShareClient.SetUserDetails(username, UserID);

        _updaterClient.GetClientId(UserID);
        WhiteboardGUI.Models.ServerOrClient serverOrClient = WhiteboardGUI.Models.ServerOrClient.ServerOrClientInstance;

        serverOrClient.SetUserDetails(UserName, UserID, UserEmail, UserProfileUrl);
        _contentInstance.SetUserDetails_client(UserName, UserID, UserProfileUrl);

        Trace.WriteLine("[DashboardServer] sent info to whiteboard client");
    }

    /// <summary>
    /// Handles the client leaving the session.
    /// </summary>
    /// <returns>True if the client leaves gracefully.</returns>
    public bool ClientLeft()
    {
        DashboardDetails details = new DashboardDetails
        {
            User = new UserDetails { UserName = UserName, UserId = UserID },
            Action = Action.ClientUserLeft
        };
        string json_message = JsonSerializer.Serialize(details);

        try
        {
            _communicator.Send(json_message, "Dashboard", null);
            Trace.WriteLine("[Dashboardclient] left session gracefully");
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"{ex.Message}");
        }
        System.Threading.Thread.Sleep(5000);
        _communicator.Stop();
        return true;
    }

    /// <summary>
    /// Handles data received from the server.
    /// </summary>
    /// <param name="message">Received message.</param>
    public void OnDataReceived(string message)
    {
        try
        {
            var details = JsonSerializer.Deserialize<DashboardDetails>(message);
            if (details == null)
            {
                Console.WriteLine("Error: Deserialized message is null");
                return;
            }
            Trace.WriteLine("[DashClient]" + details.Action);
            switch (details.Action)
            {
                case Action.ServerSendUserID:
                    HandleRecievedUserInfo(details);
                    break;
                case Action.ServerUserAdded:
                    HandleUserConnected(details);
                    break;
                case Action.ServerUserLeft:
                    HandleUserLeft(details);
                    break;
                case Action.ServerEnd:
                    HandleEndOfMeeting();
                    break;
                default:
                    Console.WriteLine($"Unknown action: {details.Action}");
                    break;
            }
        }
        catch (JsonException)
        {
            Trace.WriteLine("[DashClient] received list from server");
            var userList = JsonSerializer.Deserialize<List<UserDetails>>(message);
            if (userList != null)
            {
                ClientUserList = new ObservableCollection<UserDetails>(userList);
                CurrentUserCount = userList.Count;
            }
            else
            {
                Console.WriteLine("Error: Deserialized user list is null");
            }
            OnPropertyChanged(nameof(ClientUserList));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing message: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles received user information.
    /// </summary>
    /// <param name="message">Received message.</param>
    private void HandleRecievedUserInfo(DashboardDetails message)
    {
        if (message.User != null && message.User.UserId != null)
        {
            UserID = message.User.UserId;
            SendInfo(UserName, UserEmail);
        }
        else
        {
            Console.WriteLine("Error: Received user info is null");
        }
    }

    /// <summary>
    /// Handles a user connecting to the server.
    /// </summary>
    /// <param name="message">Received message.</param>
    private void HandleUserConnected(DashboardDetails message)
    {
        if (message.User != null && message.User.UserId != null)
        {
            UserDetails userData = message.User;
            string newuserid = userData.UserId;

            Trace.WriteLine($"[Dash client] User Connected: {userData.UserName}");

            if (newuserid != UserID)
            {
                if (ClientUserList.Count >= int.Parse(newuserid))
                {
                    ClientUserList[int.Parse(newuserid) - 1] = userData;
                }
                else
                {
                    ClientUserList.Add(userData);
                }
            }
            CurrentUserCount++;
            OnPropertyChanged(nameof(ClientUserList));
        }
        else
        {
            Console.WriteLine("Error: Received user info is null");
        }
    }

    /// <summary>
    /// Handles a user leaving the server.
    /// </summary>
    /// <param name="message">Received message.</param>
    private void HandleUserLeft(DashboardDetails message)
    {
        if (message.User != null && message.User.UserId != null)
        {
            Trace.WriteLine("[Dashboard client] some random client left");
            CurrentUserCount--;
            string leftuserid = message.User.UserId;

            foreach (var user in ClientUserList)
            {
                if (user.UserId == leftuserid)
                {
                    ClientUserList.Remove(user);
                }
            }
            OnPropertyChanged(nameof(ClientUserList));
        }
        else
        {
            Console.WriteLine("Error: Received user info is null");
        }
    }

    /// <summary>
    /// Handles the end of the meeting.
    /// </summary>
    private void HandleEndOfMeeting()
    {
        ClientUserList.Clear();
        _communicator.Stop();
    }

    /// <summary>
    /// Event triggered when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Notifies listeners that a property value has changed.
    /// </summary>
    /// <param name="property">Property name.</param>
    protected void OnPropertyChanged(string property)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}
