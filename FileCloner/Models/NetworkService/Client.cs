/******************************************************************************
 * Filename    = Client.cs
 *
 * Author      = Sai Hemanth Reddy & Neeraj Krishna N
 * 
 * Project     = FileCloner
 *
 * Description = Client class handles network communication for the FileCloner 
 *               application. It manages requests, responses, and file transfers 
 *               with other nodes. Implements INotificationHandler for message 
 *               handling and integrates logging for communication status updates.
 *****************************************************************************/

using Networking.Communication;
using Networking;
using Networking.Serialization;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using FileCloner.FileClonerLogging;
using System.Timers;

namespace FileCloner.Models.NetworkService;

/// <summary>
/// The Client class handles network communication, including sending requests, receiving responses,
/// and managing file transfers. Implements INotificationHandler to handle incoming messages.
/// </summary>
public class Client : INotificationHandler
{
    // Static fields for client communicator and request ID
    private static readonly ICommunicator s_client = CommunicationFactory.GetCommunicator(isClientSide: true);
    private static int s_requestID = 0;

    // Action delegate for logging messages, serializer for data, and flags for tracking state
    private readonly Action<string> _logAction;
    private readonly Serializer _serializer;
    private readonly List<string> _responders = [];

    private FileClonerLogger _logger = new("Client");
    private int _numberOfFilesRequested = 0;
    private int _numberOfFilesReceived = 0;
    private readonly System.Timers.Timer _timeoutTimer; // Timer for handling timeout
    private readonly int _timeoutInterval = 30000; // 30 seconds (adjust as needed)
    private readonly object _lock = new(); // For thread-safety


    /// <summary>
    /// Constructor initializes the client, sets up communication, and subscribes to the server.
    /// </summary>
    /// <param name="logAction">Action delegate to log messages to UI or console.</param>
    /// <param name="ipAddress">IP address of the server to connect to.</param>
    /// <param name="port">Port of the server to connect to.</param>
    public Client(Action<string> logAction)
    {
        this._logAction = logAction;
        _serializer = new Serializer();
        _timeoutTimer = new System.Timers.Timer(_timeoutInterval);
        _timeoutTimer.Elapsed += OnTimeoutElapsed;
        _timeoutTimer.AutoReset = false; // Ensure it triggers once per activation

        logAction?.Invoke("[Client] Connected to server!");
        _logger.Log("[Client] Connected to server!");
        s_client.Subscribe(Constants.ModuleName, this, false); // Subscribe to receive notifications
    }

    /// <summary>
    /// Starts the timeout timer when cloning begins.
    /// </summary>
    private void StartTimeoutTimer()
    {
        lock (_lock)
        {
            if (!_timeoutTimer.Enabled)
            {
                _timeoutTimer.Start();
                _logAction?.Invoke("[Client] Timeout timer started.");
            }
        }
    }

    /// <summary>
    /// Stops the timeout timer when cloning ends.
    /// </summary>
    private void StopTimeoutTimer()
    {
        lock (_lock)
        {
            if (_timeoutTimer.Enabled)
            {
                _timeoutTimer.Stop();
                _logAction?.Invoke("[Client] Timeout timer stopped.");
            }
        }
    }

    /// <summary>
    /// Event handler for when the timeout timer elapses.
    /// Logs a timeout message.
    /// </summary>
    private void OnTimeoutElapsed(object sender, ElapsedEventArgs e)
    {
        _logAction?.Invoke($"[Client] Request Timed Out _numberOfFilesRequested = {_numberOfFilesRequested}  _numberOfFilesReceived = {_numberOfFilesReceived}");
        _logger.Log($"[Client] Request Timed Out _numberOfFilesRequested = {_numberOfFilesRequested}  _numberOfFilesReceived = {_numberOfFilesReceived}", isErrorMessage: true);
    }

    /// <summary>
    /// Sends a broadcast request to initiate the file cloning process.
    /// </summary>
    public void SendRequest()
    {
        try
        {
            Message message = new Message {
                Subject = Constants.Request,
                RequestID = s_requestID,
                From = Constants.IPAddress,
                To = Constants.Broadcast
            };

            // Send serialized message to server
            s_client.Send(_serializer.Serialize<Message>(message), Constants.ModuleName, null);

            // clean the folder that stores responses after sending request
            if (Directory.Exists(Constants.ReceivedFilesFolderPath))
            {
                foreach (string file in Directory.GetFiles(Constants.ReceivedFilesFolderPath))
                {
                    File.Delete(file);
                }
            }
            else
            {
                Directory.CreateDirectory(Constants.ReceivedFilesFolderPath);
            }

            StartTimeoutTimer(); // Start timeout tracking
            _logAction?.Invoke("[Client] Request Sent");
        }
        catch (Exception ex)
        {
            _logAction?.Invoke("[Client] Request Failed : " + ex.Message);
            _logger.Log("[Client] Request Failed : " + ex.Message, isErrorMessage: true);
        }
    }



    /// <summary>
    /// Sends a summary of the cloned files to each responder.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public void SendSummary()
    {
        foreach (string responder in _responders)
        {
            try
            {
                _numberOfFilesRequested++;
                // Generate path for the summary file specific to each responder
                string filePath = Path.Combine(Constants.SenderFilesFolderPath, $"{responder}.txt");

                // Read file content for summary
                string fileContent;
                using (StreamReader reader = new StreamReader(filePath))
                {
                    fileContent = reader.ReadToEnd();
                }

                // Create message containing the summary content
                Message message = new Message {
                    Subject = Constants.Summary,
                    RequestID = s_requestID,
                    From = Constants.IPAddress,
                    To = responder,
                    Body = fileContent
                };

                // Send the message
                s_client.Send(_serializer.Serialize<Message>(message), Constants.ModuleName, "");
                _logAction?.Invoke($"[Client] Summary Sent to {responder}");
                _logger.Log($"Summary sent to {responder}");
            }
            catch (Exception ex)
            {
                _logAction?.Invoke($"[Client] Summary not sent to {responder} : {ex.Message}");
                _logger.Log($"[Client] Failed to send summary to {responder} : {ex.Message}", isErrorMessage: true);
            }
        }
    }

    /// <summary>
    /// Sends a response to a received request message, including input file content.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public void SendResponse(Message data)
    {
        try
        {
            Message message = new Message {
                Subject = Constants.Response,
                RequestID = data.RequestID,
                From = Constants.IPAddress,
                To = data.From,
                Body = string.Join(Environment.NewLine, File.ReadAllLines(Constants.InputFilePath))
            };

            s_client.Send(_serializer.Serialize<Message>(message), Constants.ModuleName, "");
            _logAction?.Invoke($"[Client] Response Sent to {data.From}");
            _logger.Log($"[Client] Response Sent to {data.From}");
        }
        catch (Exception ex)
        {
            _logAction?.Invoke($"[Client] Failed to send response to {data.From} : {ex.Message}");
            _logger.Log($"[Client] Failed to send response to {data.From} : {ex.Message}", isErrorMessage: true);
        }
    }

    /// <summary>
    /// Sends a file to the requester for cloning, specifying both local and requester paths.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public void SendFileForCloning(string from, string path, string requesterPath)
    {
        try
        {
            Thread senderThread = new Thread(() => {
                SendFilesInChunks(from, path, requesterPath);
            }) {
                IsBackground = true
            };
            _logger.Log($"Starting to send file: {path} in chunks");
            senderThread.Start();


            // logAction?.Invoke($"[Client] Sent {path} to {from}");
        }
        catch (Exception ex)
        {
            _logAction?.Invoke($"[Client] Failed to send response to from {from} : {ex.Message}");
            _logger.Log($"[Client] Failed to send response to from {from} : {ex.Message}", isErrorMessage: true);
        }
    }

    /// <summary>
    /// Function to send files in chunks rather than at once
    /// this function can send any type of file over the network
    /// </summary>
    /// <param name="from"></param>
    /// <param name="path"></param>
    /// <param name="requesterPath"></param>
    [ExcludeFromCodeCoverage]
    public void SendFilesInChunks(string from, string path, string requesterPath)
    {
        try
        {
            using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read);
            FileInfo fileInfo = new(path);
            long fileSizeInBytes = fileInfo.Length;
            int bufferSize = fileSizeInBytes < Constants.FileChunkSize ? (int)fileSizeInBytes : Constants.FileChunkSize;
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;
            int indexOfChunkBeingSent = Constants.ChunkStartIndex;

            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (bytesRead < buffer.Length)
                {
                    // Resize the buffer to match the exact bytes read on the final chunk
                    buffer = buffer.Take(bytesRead).ToArray();
                }

                Message message = new Message {
                    Subject = Constants.Cloning,
                    RequestID = s_requestID,
                    From = Constants.IPAddress,
                    MetaData = requesterPath,
                    To = from,
                    Body = $"{indexOfChunkBeingSent}:" + _serializer.Serialize(buffer)
                };

                s_client.Send(_serializer.Serialize<Message>(message), Constants.ModuleName, "");
                _logger.Log($"Sent file {path} chunk Number : {indexOfChunkBeingSent}");
                if (indexOfChunkBeingSent % 10 == 0)
                {
                    _logAction?.Invoke($"[Client] Sent {indexOfChunkBeingSent} chunks of {path} to {from}");
                }
                ++indexOfChunkBeingSent;
            }
        }
        catch (Exception ex)
        {
            _logAction?.Invoke(
                $"[Client] Exception occured while sending chunk : {ex.Message}"
            );
            _logger.Log(
                $"[Client] Exception occured while sending chunk : {ex.Message}",
                isErrorMessage: true
            );
        }


    }

    /// <summary>
    /// Stops the cloning process by incrementing request ID to track new requests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public void StopCloning()
    {
        s_requestID++;
        _numberOfFilesReceived = 0;
        _numberOfFilesRequested = 0;
        StopTimeoutTimer(); // Stop timeout tracking
        _responders.Clear();
        _logger.Log($"Stopping Cloning, Increased s_requestID to {s_requestID}");
    }

    /// <summary>
    /// Handles incoming data and processes it based on the message subject.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public void OnDataReceived(string serializedData)
    {
        Message data = _serializer.Deserialize<Message>(serializedData);
        string subject = data.Subject;
        string from = data.From;
        _logger.Log($"Data received from {data.From} with subject {data.Subject}");

        // Prevent processing self-sent messages
        if (from != Constants.IPAddress || s_requestID != data.RequestID)
        {
            //logAction?.Invoke($"[Client] Received {subject} from {from}");

            switch (subject)
            {
                case Constants.Request:
                    SendResponse(data);
                    break;
                case Constants.Response:
                    OnResponseReceived(data);
                    break;
                case Constants.Summary:
                    OnSummaryReceived(data);
                    break;
                case Constants.Cloning:
                    OnFileForCloningReceived(data);
                    break;
            }
        }
    }

    /// <summary>
    /// Processes a response message by saving the received file locally.
    /// </summary>
    public void OnResponseReceived(Message data)
    {
        try
        {
            _logAction?.Invoke($"[Client] Response received from {data.From}");
            _logger.Log($"[Client] Response received from {data.From}");
            _responders.Add(data.From);
            string savePath = Path.Combine(Constants.ReceivedFilesFolderPath, $"{data.From}.json");
            File.WriteAllText(savePath, data.Body);
        }
        catch (Exception e)
        {
            _logAction?.Invoke($"[Client] Failed to save response from {data.From} : {e}");
            _logger.Log($"[Client] Failed to save response from {data.From} : {e}", isErrorMessage: true);
        }
    }

    /// <summary>
    /// Processes a summary message by extracting file paths and sending files for cloning.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public void OnSummaryReceived(Message data)
    {
        try
        {
            _logAction?.Invoke($"[Client] Summary received from {data.From}");
            // Parse each line containing local and requester paths
            string[] lines = data.Body.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] paths = line.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                if (paths.Length == 2)
                {
                    string localPath = paths[0].Trim();
                    string requesterPath = paths[1].Trim();
                    _logger.Log($"Starting to send file for cloning {data.From}, {localPath}, {requesterPath}");

                    // Send file for cloning using the specified paths
                    SendFileForCloning(data.From, localPath, requesterPath);
                }
                else
                {
                    _logAction?.Invoke($"[Client] Invalid path format in summary data: {line}");
                    _logger.Log($"[Client] Invalid path format in summary data: {line}");
                }
            }
        }
        catch (Exception ex)
        {
            _logAction?.Invoke($"[Client] Failed to process summary: {ex.Message}");
            _logger.Log($"[Client] Failed to process summary: {ex.Message}", isErrorMessage: true);
        }
    }

    /// <summary>
    /// Processes a cloning file message by saving the received file to the specified path.
    /// </summary>
    public void OnFileForCloningReceived(Message data)
    {
        try
        {
            _numberOfFilesReceived++;
            // Extract the save path from message metadata
            string requesterPath = data.MetaData;
            _logger.Log($"File for cloning received : meta data : {data.MetaData}");

            // Ensure directory exists for the requester path
            if (!Directory.Exists(Path.GetDirectoryName(requesterPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(requesterPath));
                _logger.Log($"Directory created {Path.GetDirectoryName(requesterPath)}");
            }

            // Write the file content to the specified path
            //using (StreamWriter writer = new StreamWriter(requesterPath))
            //{
            //    writer.Write(data.Body);
            //}

            string messageBody = data.Body;
            string[] messageBodyList = messageBody.Split(':', 2);
            if (messageBodyList.Length != 2)
            {
                return;
            }

            int chunkNumber = int.Parse(messageBodyList[0]);
            _logger.Log($"Receving File {requesterPath} chunk Number : {chunkNumber}");
            string serializedFileContent = messageBodyList[1];
            FileMode fileMode = chunkNumber == Constants.ChunkStartIndex ? FileMode.Create : FileMode.Append;
            byte[] buffer = _serializer.Deserialize<byte[]>(serializedFileContent);

            using FileStream fileStream = new FileStream(requesterPath, fileMode, FileAccess.Write);
            if (buffer != null)
            {
                fileStream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                byte[] emptyBytes = Encoding.UTF8.GetBytes("");
                fileStream.Write(emptyBytes, 0, emptyBytes.Length);
            }

            if (chunkNumber % 10 == 0)
            {
                _logAction?.Invoke($"[Client] File received ({chunkNumber} chunks till now)" +
                    $" from {data.From} and saved to {requesterPath}");
                _logger.Log($"[Client] File received ({chunkNumber} chunks till now)" +
                    $" from {data.From} and saved to {requesterPath}");
            }

        }
        catch (Exception ex)
        {
            _logAction?.Invoke($"[Client] Failed to save received file from {data.From}: {ex.Message}");
            _logger.Log($"[Client] Failed to save received file from {data.From}: {ex.Message}", isErrorMessage: true);
        }
    }

    /// <summary>
    /// Stops the client communicator.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public void Stop()
    {
        _logger.Log("Stopping Client");
        s_client.Stop();
    }
}
