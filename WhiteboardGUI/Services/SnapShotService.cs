using SECloud.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WhiteboardGUI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using SECloud.Services;
using SECloud.Models;
using System.Net.Http;
using SECloud.Interfaces;
using System.Windows.Data;

namespace WhiteboardGUI.Services;

/// <summary>
/// The SnapShotService class manages the creation, uploading, downloading, and management of snapshots
/// containing shapes in the Whiteboard application.
/// </summary>
public class SnapShotService
{

    /// <summary>
    /// Networking service used to manage client-related network operations.
    /// </summary>
    private readonly NetworkingService _networkingService;

    /// <summary>
    /// Rendering service used to render shapes on the whiteboard.
    /// </summary>
    private readonly RenderingService _renderingService;

    /// <summary>
    /// Service for handling undo and redo operations.
    /// </summary>
    private readonly UndoRedoService _undoRedoService;

    /// <summary>
    /// Observable collection of shapes currently on the whiteboard.
    /// </summary>
    private ObservableCollection<IShape> _shapes;

    /// <summary>
    /// List of snapshot download items available to the user.
    /// </summary>
    private List<SnapShotDownloadItem> _snapShotDownloadItems = new();

    /// <summary>
    /// Event triggered when a snapshot is uploaded successfully.
    /// </summary>
    public event Action OnSnapShotUploaded;

    /// <summary>
    /// Cloud service for interacting with the backend snapshot storage.
    /// </summary>
    private ICloud _cloudService;

    /// <summary>
    /// Maximum number of snapshots allowed to be stored.
    /// </summary>
    int _limit = 5;

    /// <summary>
    /// The user id of the current user
    /// </summary>
    string _userEmail;

    /// <summary>
    /// Initializes a new instance of the SnapShotService class.
    /// </summary>
    /// <param name="networkingService">Networking service for managing client ID.</param>
    /// <param name="renderingService">Rendering service for managing whiteboard shapes.</param>
    /// <param name="shapes">Observable collection of shapes on the whiteboard.</param>
    /// <param name="undoRedoService">Undo/redo service for managing user operations.</param>
    /// <param name="UserID">The unique identifier of the user.</param>
    public SnapShotService(NetworkingService networkingService, RenderingService renderingService, ObservableCollection<IShape> shapes, UndoRedoService undoRedoService, string userEmail)
    {
        _networkingService = networkingService;
        _renderingService = renderingService;
        _undoRedoService = undoRedoService;
        _shapes = shapes;
        _userEmail = userEmail;
        InitializeCloudService();
    }

    /// <summary>
    /// Initializes the cloud service for managing snapshot uploads and downloads.
    /// </summary>
    private void InitializeCloudService()
    {
        // Dependency injection setup for logging and HTTP client
        ServiceProvider serviceProvider = new ServiceCollection()
        .AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        })
        .AddHttpClient()
        .BuildServiceProvider();

        ILogger<CloudService> logger = serviceProvider.GetRequiredService<ILogger<CloudService>>();
        var httpClient = new HttpClient(); // Simplified for testing

        // Configuration for the cloud service
        string baseUrl = "https://secloudapp-2024.azurewebsites.net/api";
        string team = "whiteboard";
        string sasToken = "sp=racwdli&st=2024-11-14T21:02:09Z&se=2024-11-30T05:02:09Z&spr=https&sv=2022-11-02&sr=c&sig=tSw6pO8%2FgqiG2MgU%2FoepmRkFuuJrTerVy%2BDn91Y0WH8%3D";

        _cloudService = new CloudService(baseUrl, team, sasToken, httpClient, logger);
    }

    /// <summary>
    /// Uploads a snapshot containing the given shapes to the cloud.
    /// </summary>
    /// <param name="snapShotFileName">Name of the snapshot file.</param>
    /// <param name="shapes">Collection of shapes to be included in the snapshot.</param>
    /// <param name="isTest">Indicates if this is a test operation.</param>
    public async Task UploadSnapShot(string snapShotFileName, ObservableCollection<IShape> shapes, bool isTest)
    {
        await Task.Run(async () =>
        {
            // Create and parse snapshot details
            var snapShot = new SnapShot();
            snapShotFileName = ParseSnapShotName(snapShotFileName, snapShot);
            Debug.WriteLine($"Uploading snapshot '{snapShotFileName}' with {shapes.Count} shapes.");

            // Upload snapshot to cloud
            SendToCloud(snapShotFileName, snapShot, shapes);

            // Show confirmation message if not in test mode
            if (!isTest){
                MessageBox.Show($"Filename '{snapShotFileName}' has been set.", "Filename Set", MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.DefaultDesktopOnly);
            }
        });

        // Trigger upload completed event
        if (!isTest){
            System.Windows.Application.Current.Dispatcher.Invoke(() => OnSnapShotUploaded?.Invoke());
        }
        Debug.WriteLine("Upload completed.");
    }

    /// <summary>
    /// Sends the serialized snapshot to the cloud storage.
    /// </summary>
    /// <param name="snapShotFileName">Snapshot file name.</param>
    /// <param name="snapShot">Snapshot object containing shapes and metadata.</param>
    /// <param name="shapes">Shapes to be serialized and uploaded.</param>
    private async void SendToCloud(string snapShotFileName, SnapShot snapShot, ObservableCollection<IShape> shapes)
    {
        CheckLimit(); // Ensure storage limit is respected
        snapShot._userID = _userEmail.ToString();
        snapShot._shapes = new ObservableCollection<IShape>(shapes);
        string snapShotSerialized = SerializationService.SerializeSnapShot(snapShot);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(snapShotSerialized));
        ServiceResponse<string> response = await _cloudService.UploadAsync(snapShotFileName + ".json", stream, "application/json");
        Debug.WriteLine("RESPONSE:" + response.ToString());
    }

    /// <summary>
    /// Checks the current number of snapshots and removes the oldest one if the limit is exceeded.
    /// </summary>
    private void CheckLimit()
    {
        while (_snapShotDownloadItems.Count >= _limit)
        {
            SnapShotDownloadItem lastSnapName = FindLastSnap();
            DeleteSnap(lastSnapName);
        }
    }

    /// <summary>
    /// Deletes a snapshot from both cloud and local storage.
    /// </summary>
    /// <param name="lastSnap">Snapshot to delete.</param>
    private void DeleteSnap(SnapShotDownloadItem lastSnap)
    {
        _snapShotDownloadItems.RemoveAll(item => item.FileName == lastSnap.FileName && item.Time == lastSnap.Time);
        string snapName = $"{lastSnap.FileName}_{((DateTimeOffset)lastSnap.Time.ToUniversalTime()).ToUnixTimeSeconds()}";

        _cloudService.DeleteAsync(snapName + ".json");
    }

    /// <summary>
    /// Finds the oldest snapshot in the list based on its timestamp.
    /// </summary>
    /// <returns>The oldest snapshot download item.</returns>
    private SnapShotDownloadItem FindLastSnap()
    {
        return _snapShotDownloadItems
            .OrderBy(item => item.Time)
            .FirstOrDefault();
    }

    /// <summary>
    /// Parses and prepares the snapshot name, adding it to the local list.
    /// </summary>
    /// <param name="snapShotFileName">Name of the snapshot file.</param>
    /// <param name="snapShot">Snapshot object to populate metadata.</param>
    /// <returns>The parsed snapshot file name.</returns>
    private string ParseSnapShotName(string snapShotFileName, SnapShot snapShot)
    {
        if (string.IsNullOrWhiteSpace(snapShotFileName))
        {
            DateTime currentDateTime = DateTime.Now;
            snapShotFileName = currentDateTime.ToString("yyyyMMdd-HHmmss");
        }
        DateTimeOffset currentDateTimeEpoch = DateTimeOffset.UtcNow;
        snapShot._dateTime = currentDateTimeEpoch.LocalDateTime;
        long epochTime = currentDateTimeEpoch.ToUnixTimeSeconds();
        string newSnapShotFileName = $"{snapShotFileName}_{epochTime}";
        snapShot._fileName = snapShotFileName;

        // Add to the list of downloadable snapshots
        _snapShotDownloadItems.Add(new SnapShotDownloadItem(snapShotFileName, snapShot._dateTime));
        return newSnapShotFileName;
    }

    /// <summary>
    /// Retrieves the list of snapshots from the cloud and populates the local list.
    /// </summary>
    /// <param name="v">Not used in the current implementation.</param>
    /// <param name="isInit">Indicates whether this is an initialization request.</param>
    /// <returns>A list of <see cref="SnapShotDownloadItem"/> objects.</returns>
    public async Task<List<SnapShotDownloadItem>> GetSnaps(string v, bool isInit)
    {
        if (isInit)
        {
            // Search for JSON files in the cloud matching the user ID
            ServiceResponse<JsonSearchResponse> response = await _cloudService.SearchJsonFilesAsync("_userID", _userEmail.ToString());
            if (response != null && response.Data != null && response.Data.Matches != null)
            {
                // Populate the local list of SnapShotDownloadItems
                PopulateSnapShotDownloadItems(response);
        
                return _snapShotDownloadItems;
            }

            // Return an empty list if the response or data is null
            return new List<SnapShotDownloadItem>();
        }

        // Return the current list of SnapShotDownloadItems
        return _snapShotDownloadItems;
    }

    /// <summary>
    /// Downloads a snapshot and renders its shapes on the whiteboard.
    /// </summary>
    /// <param name="selectedDownloadItem">The snapshot item to be downloaded.</param>
    public async void DownloadSnapShot(SnapShotDownloadItem selectedDownloadItem)
    {
        // Retrieve the snapshot from local storage
        ObservableCollection<IShape> snapShot = await GetSnapShot(selectedDownloadItem);

        // Clear the whiteboard before adding new shapes
        _renderingService.RenderShape(null, "CLEAR");

        // Add shapes to the whiteboard
        AddShapes(snapShot);

        // Clear undo and redo lists
        _undoRedoService._redoList.Clear();
        _undoRedoService._undoList.Clear();
    }

    /// <summary>
    /// Adds a collection of shapes to the whiteboard and renders them.
    /// </summary>
    /// <param name="snapShot">The collection of shapes to be added.</param>
    private void AddShapes(ObservableCollection<IShape> snapShot)
    {
        foreach (IShape shape in snapShot)
        {
            _shapes.Add(shape);
            _renderingService.RenderShape(shape, "DOWNLOAD");
            Debug.WriteLine($"Added Shape {shape.GetType()}");
        }
    }

    /// <summary>
    /// Validates whether a filename is unique in the current list of snapshots.
    /// </summary>
    /// <param name="filename">The filename to validate.</param>
    /// <returns><c>true</c> if the filename is valid; otherwise, <c>false</c>.</returns>
    public bool IsValidFilename(string filename)
    {
        return !_snapShotDownloadItems.Any(item => item.FileName == filename);
    }

    /// <summary>
    /// Retrieves the shapes associated with a specific snapshot.
    /// </summary>
    /// <param name="selectedDownloadItem">The snapshot item to retrieve.</param>
    /// <returns>An <see cref="ObservableCollection{IShape}"/> containing the shapes.</returns>
    private async Task<ObservableCollection<IShape>> GetSnapShot(SnapShotDownloadItem selectedDownloadItem)
    {
        string snapName = $"{selectedDownloadItem.FileName}_{((DateTimeOffset)selectedDownloadItem.Time.ToUniversalTime()).ToUnixTimeSeconds()}.json";
        ServiceResponse<Stream> snapdownload = await _cloudService.DownloadAsync(snapName);
        if (snapdownload.Success && snapdownload.Data!=null)
        {
            var sr = new StreamReader(snapdownload.Data);
            string snapData = sr.ReadToEnd();
            return SerializationService.DeserializeSnapShot(snapData)._shapes;
        }
        return new ObservableCollection<IShape>(_shapes);
    }

    /// <summary>
    /// Populates the local list of SnapShotDownloadItems from the cloud response.
    /// </summary>
    /// <param name="response">The cloud service response containing snapshot metadata.</param>
    private void PopulateSnapShotDownloadItems(ServiceResponse<JsonSearchResponse> response)
    {
        _snapShotDownloadItems = response.Data.Matches
            .Select(match =>
            {
                // Deserialize snapshot details
                string fileName = SerializationService.DeserializeSnapShot(match.Content.ToString())._fileName;
                DateTime time = SerializationService.DeserializeSnapShot(match.Content.ToString())._dateTime;

                // Create a new SnapShotDownloadItem
                return new SnapShotDownloadItem(fileName, time);
            })
            .ToList();
    }

}
