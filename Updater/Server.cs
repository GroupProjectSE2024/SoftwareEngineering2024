/******************************************************************************
* Filename    = Server.cs
*
* Author      = Amithabh A and Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Server side sending and receiving files logic
*****************************************************************************/

using Networking.Communication;
using Networking;
using System.Net.Sockets;
using System.Diagnostics;

namespace Updater;

/// <summary>
/// Class to handle server side logic
/// </summary>
public class Server : INotificationHandler
{
    private static readonly string s_serverDirectory = AppConstants.ToolsDirectory;

    private readonly BinarySemaphore _semaphore = new();

    private ICommunicator? _communicator;

    public static event Action<string>? NotificationReceived; // Event to notify the view model
    public string _clientID = "";
    private readonly Dictionary<string, TcpClient> _clientConnections = []; // Track clients
    private static Server? s_instance;
    private static readonly object s_lock = new object();

    /// <summary>
    /// Constructor
    /// </summary>
    private Server()
    {
        // Subscribing the "FileTransferHandler" for handling notifications
        _communicator = CommunicationFactory.GetCommunicator(isClientSide: false);
        _communicator.Subscribe("FileTransferHandler", this);
    }

    public static Server GetServerInstance(Action<string>? notificationReceived = null)
    {
        lock (s_lock)
        {
            if (s_instance == null)
            {
                s_instance = new Server();
            }

            if (notificationReceived != null)
            {
                NotificationReceived = notificationReceived;
            }
        }
        return s_instance;
    }

    public void Broadcasting(string serializedPacket)
    {
        _semaphore.Wait();

        UpdateUILogs("Broadcasting the new files");
        Trace.WriteLine("[Updater] Broadcasting the new files");
        try
        {

            if (_communicator == null)
            {
                UpdateUILogs("Communicator is null");
                throw new Exception("Communicator is null");
            }

            _communicator.Send(serializedPacket, "FileTransferHandler", null); // Broadcast to all clients
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error sending data to client: {ex.Message}");
        }
        // Wait for one second
        System.Threading.Thread.Sleep(1000);
        this.CompleteSync();
    }

    public void Broadcast(string serializedPacket)
    {
        Thread thread = new Thread(() => Broadcasting(serializedPacket));
        thread.Start();
    }

    /// <summary>
    /// Send SyncUp request to client
    /// </summary>
    /// <param name="clientId">ID of the client</param>
    public void SyncUp(string clientId)
    {
        try
        {
            UpdateUILogs($"Sending sync up request to client {clientId}");
            string? serializedSyncUpPacket = Utils.SerializedSyncUpPacket(clientId) ?? throw new Exception("Serialized SyncUp packet is null");

            // Write equivalent of this: 
            // UpdateUILogs("Syncing Up with the server");
            Trace.WriteLine($"[Updater] Sending SyncUp request dataPacket to client: {clientId}");
            if (_communicator != null)
            {
                _communicator.Send(serializedSyncUpPacket, "FileTransferHandler", clientId);
            }
            else
            {
                UpdateUILogs("Communicator is null");
            }
        }
        catch (Exception ex)
        {
            UpdateUILogs($"Error in SyncUp: {ex.Message}");
        }

    }

    /// <summary>
    /// Implementation of Wait before SyncUp
    /// </summary>
    public void RequestSyncUp(string clientId)
    {
        try
        {
            _semaphore.Wait();
            _clientID = clientId;
            SyncUp(clientId);
        }
        catch (Exception ex)
        {
            UpdateUILogs($"Error in RequestSyncUp: {ex.Message}");
        }
    }

    /// <summary>
    /// Complete the sync by Signalling _semaphore
    /// </summary>
    public void CompleteSync()
    {
        try
        {
            _semaphore.Signal();
        }
        catch (Exception ex)
        {
            UpdateUILogs($"Error in CompleteSync: {ex.Message}");
        }
    }

    public static void UpdateUILogs(string message)
    {
        NotificationReceived?.Invoke(message);
    }




    /// <summary>
    /// Demultiplex the data packet
    /// </summary>
    /// <param name="serializedData">Serialized data packet</param>
    /// <param name="communicator">Communicator object</param>
    /// <param name="server">Server object</param>
    /// <param name="clientId">Client ID</param>
    public static void PacketDemultiplexer(string serializedData, ICommunicator communicator, Server server, string clientId)
    {
        try
        {
            // Deserialize data
            DataPacket dataPacket = Utils.DeserializeObject<DataPacket>(serializedData);

            // Check PacketType
            switch (dataPacket.DataPacketType)
            {
                case DataPacket.PacketType.SyncUp:
                    SyncUpHandler(dataPacket, communicator, server, clientId);
                    break;
                case DataPacket.PacketType.Metadata:
                    MetadataHandler(dataPacket, communicator, server, clientId);
                    break;
                case DataPacket.PacketType.ClientFiles:
                    ClientFilesHandler(dataPacket, communicator, server, clientId);
                    break;
                default:
                    throw new Exception("Invalid PacketType");
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error in PacketDemultiplexer: {ex.Message}");
        }
    }

    /// <summary>
    /// Handler for SyncUp request from client
    /// </summary>
    /// <param name="dataPacket">Data packet</param>
    /// <param name="communicator">Communicator object</param>
    /// <param name="server">Server object</param>
    /// <param name="clientId">Client ID</param>
    private static void SyncUpHandler(DataPacket dataPacket, ICommunicator communicator, Server server, string clientId)
    {
        try
        {
            List<FileContent> fileContents = dataPacket.FileContentList;

            if (!fileContents.Any())
            {
                UpdateUILogs("No client ID received.");
                throw new Exception("[Updater] No client ID received");
            }

            // Process the first file content
            FileContent fileContent = fileContents[0];

            if (fileContent.SerializedContent == null)
            {
                UpdateUILogs("Client ID is null");
                throw new Exception("Client ID is null");
            }

            clientId = fileContent.SerializedContent;

            // Start new thread for client for communication
            Thread thread = new Thread(() => server.RequestSyncUp(clientId));
            thread.Start();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error in SyncUpHandler: {ex.Message}");
        }
    }

    /// <summary>
    /// Metadata dataPacket Handler + validate sync up
    /// </summary>
    /// <param name="dataPacket">Data packet</param>
    /// <param name="communicator">Communicator object</param>
    /// <param name="clientId">Client ID</param>
    private static void MetadataHandler(DataPacket dataPacket, ICommunicator communicator, Server server, string clientId)
    {
        try
        {
            UpdateUILogs($"Received {clientId} metadata");
            // Extract metadata of client directory
            List<FileContent> fileContents = dataPacket.FileContentList;

            if (fileContents.Count == 0)
            {
                UpdateUILogs("No file content received in the data packet.");
                throw new Exception("No file content received in the data packet.");
            }

            // Process the first file content
            FileContent fileContent = fileContents[0];
            string? serializedContent = fileContent.SerializedContent;

            Trace.WriteLine("[Updater] " + serializedContent ?? "Serialized content is null");

            // Deserialize the client metadata
            List<FileMetadata>? metadataClient;
            if (serializedContent != null)
            {
                metadataClient = Utils.DeserializeObject<List<FileMetadata>>(serializedContent);
            }
            else
            {
                metadataClient = null;
            }
            if (metadataClient == null)
            {
                UpdateUILogs("Deserialized client metadata is null");
                throw new Exception("[Updater] Deserialized client metadata is null");
            }

            Trace.WriteLine("[Updater]: Metadata from client received");

            // Generate metadata of server
            List<FileMetadata>? metadataServer = new DirectoryMetadataGenerator(s_serverDirectory).GetMetadata();
            if (metadataServer == null)
            {
                UpdateUILogs("Metadata server is null");
                throw new Exception("Metadata server is null");
            }
            Trace.WriteLine("[Updater] Metadata from server generated");

            // Compare metadata and get differences
            DirectoryMetadataComparer comparerInstance = new DirectoryMetadataComparer(metadataServer, metadataClient);
            List<MetadataDifference> differences = comparerInstance.Differences;

            // Check if the sync up is invalid
            // If it is invalid, server will send an InvalidSync response packet to
            // client along with list of filenames that needs to be changed in the client side
            if (!comparerInstance.ValidateSync())
            {
                try
                {
                    List<string>? invalidFileNames = comparerInstance.InvalidSyncUpFiles;
                    if (invalidFileNames?.Count == 0)
                    {
                        UpdateUILogs("InvalidSyncUpFiles is empty");
                        throw new Exception("InvalidSyncUpFiles is empty");
                    }

                    string? serializedInvalidFileNames = Utils.SerializeObject(invalidFileNames);

                    if (serializedInvalidFileNames == null)
                    {
                        UpdateUILogs("Serialized invalid file names is null");
                        throw new Exception("Serialized invalid file names is null");
                    }

                    FileContent fileContentToSend = new FileContent(
                            "invalidFileNames.list",
                            serializedInvalidFileNames
                            );

                    DataPacket dataPacketToSend = new DataPacket(
                            DataPacket.PacketType.InvalidSync,
                            [fileContentToSend]
                            );


                    try
                    {
                        string? serializedDataPacket = Utils.SerializeObject(dataPacketToSend);

                        if (serializedDataPacket == null)
                        {
                            UpdateUILogs("Serialized data packet is null");
                            throw new Exception("Serialized data packet is null");
                        }

                        UpdateUILogs($"Sending files to client and waiting to recieve files from client {clientId}");
                        communicator.Send(serializedDataPacket, "FileTransferHandler", clientId);

                        // End the sync up
                        server.CompleteSync();
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"[Updater] Error sending data to client: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[Updater] Error in InvalidSyncUp: {ex.Message}");
                }
            }

            // If sync up is valid, send differences file to client
            else
            {
                // Serialize and save differences to C:\temp\ folder
                string? serializedDifferences = Utils.SerializeObject(differences);
                string tempFilePath = @$"{Server.s_serverDirectory}\differences.xml";

                if (string.IsNullOrEmpty(serializedDifferences))
                {
                    Trace.WriteLine("[Updater] Serialization of differences failed or resulted in an empty string.");
                    return; // Exit if serialization fails
                }

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(tempFilePath)!);
                    File.WriteAllText(tempFilePath, serializedDifferences);
                    UpdateUILogs($"Differences file saved to {tempFilePath}");
                    Trace.WriteLine($"[Updater] Differences file saved to {tempFilePath}");
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[Updater] Error saving differences file: {ex.Message}");
                }

                // Prepare data to send to client
                List<FileContent> fileContentsToSend =
                    [
                        // Added difference file to be sent to client
                        new("differences.xml", serializedDifferences)
                    ];

                // Retrieve and add unique server files to fileContentsToSend
                foreach (string filename in comparerInstance.UniqueServerFiles)
                {
                    string filePath = Path.Combine(Server.s_serverDirectory, filename);
                    string? content = Utils.ReadBinaryFile(filePath);

                    if (content == null)
                    {
                        Trace.WriteLine($"Warning: Content of file {filename} is null, skipping.");
                        continue; // Skip to the next file instead of throwing an exception
                    }

                    Trace.WriteLine($"[Updater] Content length of {filename}: {content.Length}");

                    // Serialize file content and create FileContent object
                    string? serializedFileContent = Utils.SerializeObject(content);
                    if (string.IsNullOrEmpty(serializedFileContent))
                    {
                        Trace.WriteLine($"[Updater] Warning: Serialized content for {filename} is null or empty.");
                        continue; // Skip to next file if serialization fails
                    }

                    FileContent fileContentToSend = new FileContent(filename, serializedFileContent);
                    fileContentsToSend.Add(fileContentToSend);
                }

                // Create DataPacket after all FileContents are ready
                DataPacket dataPacketToSend = new DataPacket(DataPacket.PacketType.Differences, fileContentsToSend);
                Trace.WriteLine($"[Updater] Total files to send: {fileContentsToSend.Count}");


                try
                {
                    // Serialize DataPacket
                    string? serializedDataPacket = Utils.SerializeObject(dataPacketToSend);

                    if (serializedDataPacket == null)
                    {
                        UpdateUILogs("Serialized data packet is null");
                        throw new Exception("Serialized data packet is null");
                    }

                    UpdateUILogs($"Sending files to client and waiting to recieve files from client {clientId}");
                    communicator.Send(serializedDataPacket, "FileTransferHandler", clientId);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"[Updater] Error sending data to client: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error sending data to client: {ex.Message}");
        }
    }

    /// <summary>
    /// ClientFiles dataPacket handler
    /// </summary>
    /// <param name="dataPacket">Data packet</param>
    /// <param name="communicator">Communicator object</param>
    /// <param name="server">Server object</param>
    /// <param name="clientId">Client ID</param>
    private static void ClientFilesHandler(DataPacket dataPacket, ICommunicator communicator, Server server, string clientId)
    {
        try
        {
            UpdateUILogs("Recieved files from client");
            // File list
            List<FileContent> fileContentList = dataPacket.FileContentList;

            // Get files
            foreach (FileContent fileContent in fileContentList)
            {
                if (fileContent != null && fileContent.SerializedContent != null && fileContent.FileName != null)
                {
                    string content = Utils.DeserializeObject<string>(fileContent.SerializedContent);
                    string filePath = Path.Combine(Server.s_serverDirectory, fileContent.FileName);
                    bool status = Utils.WriteToFileFromBinary(filePath, content);

                    if (!status)
                    {
                        throw new Exception("Failed to write file");
                    }
                }
            }

            UpdateUILogs("Successfully received client's files");
            Trace.WriteLine("[Updater] Successfully received client's files");

            // Broadcast client's new files to all clients
            dataPacket.DataPacketType = DataPacket.PacketType.Broadcast;

            try
            {
                // Serialize packet
                string? serializedPacket = Utils.SerializeObject(dataPacket);

                if (serializedPacket == null)
                {
                    UpdateUILogs("Serialized packet is null");
                    throw new Exception("Serialized packet is null");
                }

                UpdateUILogs("Broadcasting the new files");
                Trace.WriteLine("[Updater] Broadcasting the new files");
                communicator.Send(serializedPacket, "FileTransferHandler", null); // Broadcast to all clients
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[Updater] Error sending broadcast data to client: {ex.Message}");
            }

            // Wait for one second
            System.Threading.Thread.Sleep(1000);
            server.CompleteSync();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error in ClientFilesHandler: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle data received from client
    /// </summary>
    public void OnDataReceived(string serializedData)
    {
        try
        {
            Trace.WriteLine("[Updater] FileTransferHandler received data");
            DataPacket deserializedData = Utils.DeserializeObject<DataPacket>(serializedData);
            if (deserializedData == null)
            {
                Trace.WriteLine("Deserialized data is null.");
            }
            else
            {
                Trace.WriteLine("[Updater] Read received data Successfully");

                if (_communicator == null)
                {
                    UpdateUILogs("Communicator is null");
                    throw new Exception("Communicator is null");
                }

                PacketDemultiplexer(serializedData, _communicator, this, _clientID);
            }

        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Deserialization failed: {ex.Message}");
        }
        finally
        {
        }
    }


    public void SetUser(string clientId, TcpClient socket)
    {
        try
        {
            Trace.WriteLine($"[Updater] FileTransferHandler detected new client connection: {socket.Client.RemoteEndPoint}, assigned ID: {clientId}");
            UpdateUILogs($"Detected new client connection: {socket.Client.RemoteEndPoint}, assigned ID: {clientId}");

            _clientConnections.Add(clientId, socket); // Add client connection to the dictionary

            // Start new thread for client for communication
            if (NotificationReceived != null)
            {
                Thread thread = new Thread(() => this.RequestSyncUp(clientId));
                thread.Start();
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error in SetUser: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle client left event
    /// </summary>
    public void OnClientLeft(string clientId)
    {
        try
        {
            if (_clientConnections.Remove(clientId))
            {
                UpdateUILogs($"Detected client {clientId} disconnected");
                Trace.WriteLine($"[Updater] FileTransferHandler detected client {clientId} disconnected");
            }
            else
            {
                Trace.WriteLine($"[Updater] Client {clientId} was not found in the connections.");
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[Updater] Error in OnClientLeft: {ex.Message}");
        }
    }
}

