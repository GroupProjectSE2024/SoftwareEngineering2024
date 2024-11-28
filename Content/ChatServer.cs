
using Networking.Communication;
using Networking;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;
using Content.ChatViewModel;

namespace Content;

/// <summary>
/// Represents a chat server that handles client connections, message processing, 
/// and broadcasting messages to the chat module.
/// </summary>

public class ChatServer : INotificationHandler
{
    /// <summary>
    /// Starts the chat server by subscribing to the "ChatModule" with a high-priority listener.
    /// </summary>
    private ICommunicator _communicator = CommunicationFactory.GetCommunicator(false);

    //public readonly Dictionary<int, string> ClientUsernames = new(); // Maps clientId to username

    public event EventHandler ClientUsernamesUpdated; //new

    public string ClientId { get; set; }

    private static ChatServer? s_serverInstance;

    /// <summary>
    /// Stops the chat server by halting the communication service.
    /// </summary>

    public ChatServer()
    {
        _communicator.Subscribe("ChatModule", this, isHighPriority: true);

    }

    public static ChatServer GetServerInstance
    {
        get {
            if (s_serverInstance == null)
            {
                s_serverInstance = new ChatServer();
            }
            return s_serverInstance;
        }

    }

    /// <summary>
    /// Processes incoming data from clients. Handles message types such as:
    /// - "connect": Registers a new client and updates the client list.
    /// - "private": Sends a private message to the specified recipient.
    /// - Other types: Broadcasts public messages to all connected clients.
    /// </summary>
    /// <param name="serializedData">The serialized data received from a client.</param>


    public void GetClientDictionary(Dictionary<int, string> clientDict)
    {
        //int clientIdInt = int.Parse(ClientId);
        //ClientUsernames[clientIdInt] = senderUsername;
        string clientDictionarySerialized = "";

        clientDictionarySerialized = JsonSerializer.Serialize(clientDict);

        string formattedMessage = $"clientlist|{clientDictionarySerialized}";
        _communicator.Send(formattedMessage, "ChatModule", destination: null);
    }


    public void OnDataReceived(string serializedData)
    {
        string[] dataParts = serializedData.Split('|');
        if (dataParts.Length < 3)
        {
            return;
        }

        string messageType = dataParts[0];
        string senderUsername = dataParts[2] + ".url." + dataParts[4];
        string senderId = dataParts[3];
        string recipientId = dataParts.Length > 5 ? dataParts[5] : null;
        //string userProfileUrl = dataParts[4];



        //if (messageType == "connect")
        //{

        //    int clientIdInt = int.Parse(ClientId);
        //    ClientUsernames[clientIdInt] = senderUsername;
        //    string tp = "";

        //    tp = JsonSerializer.Serialize(ClientUsernames);

        //    string formattedMessage = $"clientlist|{tp}";


        //    _communicator.Send(formattedMessage, "ChatModule", destination: null);

        //}

        string messageContent = dataParts[1];
        if (messageType == "private")
        {
            messageContent = $"[PRIVATE] : {messageContent}";

            //_communicator.Send($"private|{messageContent}|{senderUsername}", "ChatModule", recipientId);
            _communicator.Send($"{senderUsername} :.: {messageContent} |private|{senderUsername}|{messageContent} ", "ChatModule", recipientId);
        }
        else
        {

            _communicator.Send($"{senderUsername} :.: {messageContent} |abc|", "ChatModule", destination: null);
        }

    }


}
