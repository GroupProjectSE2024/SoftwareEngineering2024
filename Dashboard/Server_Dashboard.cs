﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using Networking;
using Networking.Communication;
using WhiteboardGUI;

namespace Dashboard
{
    /// <summary>
    /// Enum representing different actions in the Dashboard.
    /// </summary>
    public enum Action
    {
        ClientUserConnected,
        ClientUserLeft,
        ServerSendUserID,
        ServerUserAdded,
        ServerUserLeft,
        ServerEnd,
        StartOfMeeting
    }

    /// <summary>
    /// Class representing the details of the dashboard.
    /// </summary>
    [JsonSerializable(typeof(DashboardDetails))]
    public class DashboardDetails
    {
        [JsonInclude]
        public UserDetails? User { get; set; }

        [JsonInclude]
        public bool IsConnected { get; set; }

        [JsonInclude]
        public Action Action { get; set; }

        [JsonInclude]
        public string? Msg { get; set; }
    }

    /// <summary>
    /// Server dashboard class implementing notification handler.
    /// </summary>
    public class ServerDashboard : INotificationHandler
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
        /// Gets or sets the profile picture URL.
        /// </summary>
        private string ProfilePictureUrl { get; set; }

        /// <summary>
        /// Gets or sets the server IP.
        /// </summary>
        private string ServerIp { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the server port.
        /// </summary>
        private string ServerPort { get; set; } = string.Empty;

        public int TotalUserCount { get; private set; } = 1;  // Start at 1 since server is user 1
        public int CurrentUserCount { get; private set; } = 1;

        /// <summary>
        /// Gets the server user list.
        /// </summary>
        public ObservableCollection<UserDetails> ServerUserList { get; private set; } = new ObservableCollection<UserDetails>();

        /// <summary>
        /// Gets the total server user list.
        /// </summary>
        public ObservableCollection<UserDetails> TotalServerUserList { get; private set; } = new ObservableCollection<UserDetails>();

        public FileCloner.Models.NetworkService.Server _fileClonerInstance = FileCloner.Models.NetworkService.Server.GetServerInstance();
        public Updater.Server _updaterServerInstance = Updater.Server.GetServerInstance();

        /// <summary>
        /// Initializes a new instance of the ServerDashboard class.
        /// </summary>
        /// <param name="communicator">Communicator instance.</param>
        /// <param name="username">User name.</param>
        /// <param name="useremail">User email.</param>
        /// <param name="profilePictureUrl">Profile picture URL.</param>
        public ServerDashboard(ICommunicator communicator, string username, string useremail, string profilePictureUrl)
        {
            _communicator = communicator;
            _communicator.Subscribe("Dashboard", this, true);
            UserName = username;
            UserEmail = useremail;
            ProfilePictureUrl = profilePictureUrl;
            ServerUserList.CollectionChanged += (s, e) => OnPropertyChanged(nameof(ServerUserList));
        }

        /// <summary>
        /// Initializes the server dashboard and starts the server.
        /// </summary>
        /// <returns>Server credentials.</returns>
        public string Initialize()
        {
            var serverUser = new UserDetails
            {
                UserName = UserName,
                UserEmail = UserEmail,
                UserId = "1",
                IsHost = true,
                ProfilePictureUrl = ProfilePictureUrl
            };
            ServerUserList.Add(serverUser);
            TotalServerUserList.Add(serverUser);

            string serverCredentials = "failure";
            while (serverCredentials == "failure")
            {
                serverCredentials = _communicator.Start();
            }
            if (serverCredentials != "failure")
            {
                string[] parts = serverCredentials.Split(':');
                ServerIp = parts[0];
                ServerPort = parts[1];

                ICommunicator _client = CommunicationFactory.GetCommunicator(isClientSide: true);
                _client.Start(ServerIp, ServerPort);

                // Notify that server user is ready
                OnPropertyChanged(nameof(ServerUserList));
            }
            Trace.WriteLine("[DashboardServer] started server");
            WhiteboardGUI.Models.ServerOrClient serverOrClient = WhiteboardGUI.Models.ServerOrClient.ServerOrClientInstance;

            serverOrClient.SetUserDetails(UserName, "1", UserEmail, ProfilePictureUrl);

            return serverCredentials;
        }

        /// <summary>
        /// Broadcasts a message to all connected users.
        /// </summary>
        /// <param name="message">Message to be sent.</param>
        public void BroadcastMessage(string message)
        {
            foreach (UserDetails user in ServerUserList)
            {
                if (user.UserId != null)
                {
                    SendMessage(user.UserId, message);
                }
            }
        }

        /// <summary>
        /// Sends a message to a specific client.
        /// </summary>
        /// <param name="clientIP">Client IP address.</param>
        /// <param name="message">Message to be sent.</param>
        public void SendMessage(string clientIP, string message)
        {
            try
            {
                _communicator.Send(message, "Dashboard", clientIP);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles data received from clients.
        /// </summary>
        /// <param name="message">Received message.</param>
        public void OnDataReceived(string message)
        {
            try
            {
                DashboardDetails? details = JsonSerializer.Deserialize<DashboardDetails>(message);
                if (details == null)
                {
                    Console.WriteLine("Error: Deserialized message is null");
                    return;
                }
                Trace.WriteLine("[Dashserver]" + details.Action);
                switch (details.Action)
                {
                    case Action.ClientUserConnected:
                        HandleUserConnected(details);
                        break;
                    case Action.ClientUserLeft:
                        HandleUserLeft(details);
                        break;
                    default:
                        Console.WriteLine($"Unknown action: {details.Action}");
                        break;
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing message: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles a user connecting to the server.
        /// </summary>
        /// <param name="details">Details of the connected user.</param>
        private void HandleUserConnected(DashboardDetails details)
        {
            Trace.WriteLine("[DashboardServer] received client info");
            if (details?.User != null)
            {
                var userToUpdate = ServerUserList.FirstOrDefault(u => u.UserId == details.User.UserId);
                var userInTotalList = TotalServerUserList.FirstOrDefault(u => u.UserId == details.User.UserId);

                if (userToUpdate == null)
                {
                    // Update user details
                    string newUserId = details.User.UserId ?? string.Empty;

                    // Update the existing user's details
                    var newUserDetails = new UserDetails
                    {
                        UserName = details.User.UserName,
                        UserEmail = details.User.UserEmail,
                        IsHost = false,
                        ProfilePictureUrl = details.User.ProfilePictureUrl, // Update profile picture URL
                        UserId = newUserId
                    };

                    ServerUserList.Add(newUserDetails);

                    if (userInTotalList == null)
                    {
                        TotalServerUserList.Add(newUserDetails);
                    }

                    var listUsers = new List<UserDetails>(ServerUserList);
                    var jsonUserList = JsonSerializer.Serialize(listUsers);
                    SendMessage(newUserId, jsonUserList);

                    // Create message for new user joined
                    DashboardDetails dashboardMessage = new()
                    {
                        User = newUserDetails,
                        Action = Action.ServerUserAdded,
                        Msg = "User " + newUserDetails.UserName + " Joined"
                    };

                    // First send individual update
                    string joinMessage = JsonSerializer.Serialize(dashboardMessage);
                    BroadcastMessage(joinMessage);

                    // Trigger UI update
                    OnPropertyChanged(nameof(ServerUserList));
                }
            }
        }

        /// <summary>
        /// Handles a user leaving the server.
        /// </summary>
        /// <param name="details">Details of the user leaving.</param>
        private void HandleUserLeft(DashboardDetails details)
        {
            if (details?.User != null)
            {
                var userToRemove = ServerUserList.FirstOrDefault(u => u.UserId == details.User.UserId);
                if (userToRemove != null)
                {
                    DashboardDetails dashboardMessage = new()
                    {
                        User = details.User,
                        Action = Action.ServerUserLeft,
                        Msg = "User with " + userToRemove.UserName + " Left"
                    };

                    CurrentUserCount--;
                    string jsonMessage = JsonSerializer.Serialize(dashboardMessage);
                    ServerUserList.Remove(userToRemove);
                    OnPropertyChanged(nameof(ServerUserList));
                    BroadcastMessage(jsonMessage);

                    Trace.WriteLine($"[Dashboard server] {userToRemove.UserName} left");
                }
            }
        }

        /// <summary>
        /// Handles the start of the meeting.
        /// </summary>
        private void HandleStartOfMeeting()
        {
            Trace.WriteLine("[Dash Server] Meeting started");
        }

        /// <summary>
        /// Stops the server and ends the meeting.
        /// </summary>
        /// <returns>True if the server stops successfully.</returns>
        public bool ServerStop()
        {
            DashboardDetails dashboardMessage = new()
            {
                Action = Action.ServerEnd,
                Msg = "Meeting Ended"
            };
            string jsonMessage = JsonSerializer.Serialize(dashboardMessage);
            BroadcastMessage(jsonMessage);
            ServerUserList.Clear();
            System.Threading.Thread.Sleep(8000);
            _communicator.Stop();
            return true;
        }

        /// <summary>
        /// Handles a new client joining the server.
        /// </summary>
        /// <param name="socket">Client socket.</param>
        /// <param name="ip">Client IP address.</param>
        /// <param name="port">Client port number.</param>
        public void OnClientJoined(TcpClient socket, string? ip, string? port)
        {
            TotalUserCount++;
            CurrentUserCount++;

            string newUserId = TotalUserCount.ToString();

            // Create new user with temporary placeholder - don't set a name yet
            UserDetails details = new UserDetails
            {
                UserId = newUserId,
                UserEmail = "",
                IsHost = false
            };

            _communicator.AddClient(newUserId, socket);

            _updaterServerInstance.SetUser(newUserId, socket);

            _fileClonerInstance.SetUser(newUserId,socket);

            // Send only the userId to the new client
            DashboardDetails dashboardMessage = new DashboardDetails
            {
                User = new UserDetails { UserId = newUserId },  // Only send userId
                Action = Action.ServerSendUserID,
                IsConnected = true
            };

            string jsonMessage = JsonSerializer.Serialize(dashboardMessage);
            _communicator.Send(jsonMessage, "Dashboard", newUserId);
        }

        /// <summary>
        /// Handles a client leaving the server.
        /// </summary>
        /// <param name="clientId">Client ID.</param>
        public void OnClientLeft(string clientId)
        {
            var userLeaving = ServerUserList.FirstOrDefault(u => u.UserId == clientId);

            if (userLeaving != null)
            {
                DashboardDetails dashboardMessage = new()
                {
                    Action = Action.ServerUserLeft,
                    Msg = "User with " + userLeaving.UserName + " left"
                };
                string jsonMessage = JsonSerializer.Serialize(dashboardMessage);

                ServerUserList.Remove(userLeaving);
                _communicator.RemoveClient(clientId);

                OnPropertyChanged(nameof(ServerUserList));
                BroadcastMessage(jsonMessage);
            }
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
}
