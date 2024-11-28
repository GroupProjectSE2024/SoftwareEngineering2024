
using Networking.Communication;
using Networking;
//using Dashboard;

using System.Collections.Generic;
using System.Text.Json;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Networking.Serialization;

namespace Content;


public class ChatClient : INotificationHandler
{
    private ICommunicator _communicator = CommunicationFactory.GetCommunicator(true);

    public List<string> _clientList = new();
    public event EventHandler<string> MessageReceived;
    public event EventHandler<List<string>> ClientListUpdated;

    public Dictionary<int, string> _client_dict = new();
    public ObservableCollection<string> _clientListobs = new();
    public event PropertyChangedEventHandler PropertyChanged;

    public event Action < Dictionary<int, string> > LatestAction;
    public string _clientIdCheck;

    //public MessageData _messageDataClass;
    public string Username { get; set; }
    public string ClientId { get; set; }
    public string UserProfileUrl { get; set; }

    /// <summary>
    /// Initializes a new instance of the ChatClient class, subscribing to the communicator
    /// with a high-priority subscription for the "ChatModule".
    /// </summary>

    public ChatClient()
    {
        _communicator.Subscribe("ChatModule", this, isHighPriority: true);
    }

    /// <summary>
    /// Sends a message to the chat server. Supports both public and private messaging, with private messages requiring
    /// recipient ID adjustment to match server requirements.
    /// </summary>
    /// <param name="message">The message content to send.</param>
    /// <param name="recipientId">Optional recipient ID for private messages. If null, the message is sent publicly.</param>

    public void SendMessage(string message, string recipientId = null)
    {
        string messageType = recipientId == null ? "public" : "private";

        string formattedMessage = $"{messageType}Æ{message}Æ{Username}Æ{ClientId}Æ{UserProfileUrl}Æ{recipientId}";
        _communicator.Send(formattedMessage, "ChatModule", null);
        
    }

    /// <summary>
    /// Handles incoming data from the server. Parses the serialized data and performs actions based on the message type.
    /// Updates the client list for "clientlist" messages, handles private messages, and broadcasts other messages.
    /// </summary>
    /// <param name="serializedData">The data received from the server, in serialized string format.</param>

    public void OnDataReceived(string serializedData)
    {

        string[] dataParts = serializedData.Split('Æ');
        if (dataParts[0] == "clientlist")
        {
            // Update Client dictionary and ObservableCollection for the client list
            _client_dict = JsonSerializer.Deserialize<Dictionary<int, string>>(dataParts[1]);

            _clientIdCheck = _client_dict.FirstOrDefault(x => x.Value == Username).Key.ToString();


            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
               LatestAction?.Invoke(_client_dict);
            });

            OnPropertyChanged(nameof(_clientListobs));


        }
        else if (dataParts[1] == "private")
        {
            // Handle private message
            //string messageContent = dataParts[3];
            //MessageReceived?.Invoke(this, $"Private from {dataParts[2]} : {messageContent}");
            MessageReceived?.Invoke(this, serializedData);
        }
        else
        {
            // Handle public message
            //MessageReceived?.Invoke(this, serializedData);
            MessageReceived?.Invoke(this, serializedData);
        }
    }

    /// <summary>
    /// Notifies listeners that a property value has changed, primarily used for the client list.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    /// 
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
