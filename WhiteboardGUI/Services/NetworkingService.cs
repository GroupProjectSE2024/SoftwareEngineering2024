/**************************************************************************************************
 * Filename    = NetworkingService.cs
 *
 * Author      = Likith Anaparty
 *
 * Product     = WhiteBoard
 * 
 * Project     = Networking module for transferring the shape
 *
 * Description = The logic that is invoked in the networking layer when whiteboard is invoke
 *************************************************************************************************/

using System.Diagnostics;
using WhiteboardGUI.Models;
using Networking.Communication;
using Networking;
namespace WhiteboardGUI.Services;

/// <summary>
/// Provides networking functionalities for the whiteboard application, enabling hosting, 
/// client connections, and data synchronization between clients.
/// </summary>
public class NetworkingService : INotificationHandler
{
    /// <summary>
    /// The unique identifier of the client.
    /// </summary>
    public double _clientID;

    /// <summary>
    /// Indicates whether the current instance is acting as the host.
    /// </summary>
    private bool _isHost;

    /// <summary>
    /// The communication interface used for sending and receiving data.
    /// </summary>
    public ICommunicator? _communicator;

    /// <summary>
    /// Service to handle received data and maintain synchronized shapes.
    /// </summary>
    public ReceivedDataService _receivedDataService;

    /// <summary>
    /// List of shapes synchronized across all clients.
    /// </summary>
    public List<IShape> _synchronizedShapes;


    /// <summary>
    /// Module identifier for communication purposes.
    /// </summary>
    private string _moduleIdentifier = "WhiteBoard";

    private int _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkingService"/> class.
    /// </summary>
    /// <param name="dataTransferService">Service responsible for data synchronization.</param>
    public NetworkingService(ReceivedDataService dataTransferService, int userId)
    {
        _receivedDataService = dataTransferService;
        _synchronizedShapes = dataTransferService._synchronizedShapes;
        _userId = userId;
    }

    /// <summary>
    /// Starts the service as a host server.
    /// </summary>
    public void StartHost()
    {
        StartHostServer();
    }

    /// <summary>
    /// Configures and starts the host server.
    /// </summary>
    private void StartHostServer()
    {
        try
        {
            _communicator = CommunicationFactory.GetCommunicator(false);
            _communicator.Subscribe(_moduleIdentifier, this, false);
            _communicator.Start();
            _isHost = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Host error: {ex.Message}");
        }
    }

    /// <summary>
    /// Starts the service as a client server.
    /// </summary>
    public void StartClient()
    {
        StartClientServer();
    }

    /// <summary>
    /// Configures and starts the client server.
    /// </summary>
    private void StartClientServer()
    {
        try
        {
            _communicator = CommunicationFactory.GetCommunicator(true);
            _communicator.Subscribe(_moduleIdentifier, this, false);
            _communicator.Start();
            _isHost = false;
            string serializedMessage = $"ID{_userId}ENDNEWCLIENT";
            BroadcastShapeData(serializedMessage);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Host error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles data received from the communicator and processes it based on the instance role (host or client).
    /// </summary>
    /// <param name="serializedData">The serialized data received.</param>
    public void OnDataReceived(string serializedData)
    {
        if (_isHost)
        {
            int id = _receivedDataService.DataReceived(serializedData);
            if (id == -1)
            {
                return;
            }
            BroadcastShapeData(serializedData);
        }
        else
        {
            int id = _receivedDataService.DataReceived(serializedData);
        }
    }

    /// <summary>
    /// Broadcasts serialized shape data to all connected clients.
    /// </summary>
    /// <param name="serializedData">The serialized shape data to broadcast.</param>
    public void BroadcastShapeData(string serializedData, string newUserId=null)
    {
        _communicator.Send(serializedData, _moduleIdentifier, newUserId);
    }

    /// <summary>
    /// Stops the host server.
    /// </summary>
    public void StopHost()
    {
        _communicator?.Stop();
    }

    /// <summary>
    /// Stops the client.
    /// </summary>
    public void StopClient()
    {
        _communicator?.Stop();
    }
}
