/******************************************************************************
* Filename    = CloudViewModel.cs
*
* Author      = Karumudi Harika
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = CloudModel for handling functions between server and cloud
*****************************************************************************/


using System.Text.Json;
using Updater;
using SECloud.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;
using SECloud.Models;


namespace ViewModel.UpdaterViewModel;

/// <summary>
/// CloudViewModel handles synchronization and file exchange between the server and cloud.
/// It manages cloud data download/upload, file comparison between server and cloud, 
/// and facilitates server updates with cloud data and vice versa.
/// </summary>
public class CloudViewModel
{
    private const string CloudFilesName = "CloudFiles";
    private readonly LogServiceViewModel _logServiceViewModel;
    private readonly ServerViewModel _serverViewModel;
    private readonly CloudService _cloudService;
    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    public static string NameToSaveCloudFiles => CloudFilesName;
    /// <summary>
    /// Constructor to initialize CloudViewModel with necessary services and configuration.
    /// </summary>

    public CloudViewModel(LogServiceViewModel logServiceViewModel, ServerViewModel serverViewModel)
    {
        // Setting up service provider for dependency injection
        ServiceProvider serviceProvider = new ServiceCollection()
        .AddLogging(builder => {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        })
        .AddHttpClient()
        .BuildServiceProvider();
        // Logger for CloudService
        ILogger<CloudService> logger = Provider(serviceProvider);
        var httpClient = new HttpClient(); // Simplified for testing

        // Configuration values
        string baseUrl = BaseUrl();
        string team = Team();
        string sasToken = Token();

        // Create CloudService instance
        _cloudService = new CloudService(
            baseUrl,
            team,
            sasToken,
            httpClient,
            logger);

        _logServiceViewModel = logServiceViewModel;
        _serverViewModel = serverViewModel;
    }

    //Method to retrieve the SAS token
    private static string Token()
    {
        return "sp=racwdli&st=2024-11-23T08:59:00Z&se=2024-11-30T16:59:00Z&spr=https&sv=2022-11-02&sr=c&sig=WrpuCccbFULandQ5wmO0hS2JGu%2B7TD5a%2BDucFIrQRY4%3D";
    }

    // Method to retrieve the team name for cloud configuration
    private static string Team()
    {
        return "updater";
    }

    // Method to retrieve the base URL for the cloud service API
    private static string BaseUrl()
    {
        return "https://secloudapp-2024.azurewebsites.net/api";
    }

    // Method to provide logger for the CloudService using Dependency Injection
    private static ILogger<CloudService> Provider(ServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<ILogger<CloudService>>();
    }

    /// <summary>
    /// Data structure to represent the file information that will be exchanged between the server and cloud.
    /// </summary>

    public class FileData
    {
        public List<string>? Id { get; set; }
        public List<string>? Name { get; set; }
        public List<string>? Description { get; set; }
        public List<string>? FileVersion { get; set; }
        public List<string>? LastUpdate { get; set; }
        public List<string>? LastModified { get; set; }
        public List<string>? CreatorName { get; set; }
        public List<string>? CreatorMail { get; set; }
    }
    /// <summary>
    /// This method performs the cloud synchronization, comparing data between cloud and server, 
    /// uploading new files to the cloud, and downloading newer files from the cloud.
    /// </summary>

    public async Task PerformCloudSync()
    {
        _logServiceViewModel.UpdateLogDetails("Cloud starting sync..");
        // Download current cloud data
        ServiceResponse<Stream> downloadResponse = await DownloadResponseMethod();
        string cloudData;
        if (downloadResponse == null || downloadResponse.Data == null)
        {
            cloudData = "[]";  // Represents an empty JSON array
        }
        else
        {
            using var reader = new StreamReader(downloadResponse.Data);
            cloudData = await CloudDataMethod(reader);
        }
        //Retrieve server data
        string? serverData = ServerDataMethod();

        // Determine which files are new in cloud compared to server
        List<FileData> onlyCloudFiles = CloudHasMoreData(cloudData, serverData ?? throw new ArgumentNullException(nameof(serverData)));
        string jsonCloudFiles = JsonSerializer.Serialize(onlyCloudFiles, s_jsonOptions);

        // Determine which files are new on the server compared to cloud
        List<FileData> onlyServerFiles = OnlyServerFileMethod(cloudData, serverData);
        string jsonServerFiles = JsonSerializer.Serialize(onlyServerFiles, s_jsonOptions);


        //send these to server then server broadcast these files to all connected clients
        if (onlyCloudFiles.Count != 0)
        {
            _logServiceViewModel.UpdateLogDetails("Cloud has more Data than server. Sending JSON file to server....");
            UpdateServerWithCloudData(jsonCloudFiles);
            string dataFromCloud = NotificationOfCloudData(onlyCloudFiles);
            _logServiceViewModel.ShowNotification(dataFromCloud);
            _logServiceViewModel.UpdateLogDetails(dataFromCloud);
        }

        //send the files to cloud using upload function.
        if (onlyServerFiles.Count != 0)
        {
            _logServiceViewModel.UpdateLogDetails("Server has more data than cloud. Sending JSON to cloud");
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonServerFiles));
            ServiceResponse<string>? response = await ResponseMethod(stream);
            _logServiceViewModel.UpdateLogDetails(response.Data ?? "");
        }
    }
    // Method to upload data to the cloud using CloudService

    public async Task<ServiceResponse<string>> ResponseMethod(MemoryStream stream)
    {
        return await _cloudService.UploadAsync("ServerFiles.json", stream, "application/json");
    }

    private static List<FileData> OnlyServerFileMethod(string cloudData, string serverData)
    {
        return ServerHasMoreData(cloudData, serverData);
    }

    private string? ServerDataMethod()
    {
        return _serverViewModel.GetServerData() as string;

        // Assume this is a JSON string
    }

    private static async Task<string> CloudDataMethod(StreamReader reader)
    {
        return await reader.ReadToEndAsync();
    }

    private async Task<ServiceResponse<Stream>> DownloadResponseMethod()
    {
        return await _cloudService.DownloadAsync("ServerFiles.json");
    }

    /// <summary>
    /// Helper method to remove entries with "N/A" or null values from the list of files.
    /// </summary>

    public static List<FileData> RemoveNAEntries(List<FileData> files)
    {
        // Remove any entries where Name or Id or any other critical field contains "N/A" or is null.
        return files
            .Where(file => file.Name != null && !file.Name.Contains("N/A")
                           && file.Id != null
                           && file.Id.All(id => id != "N/A"))
            .ToList();
    }
    /// <summary>
    /// Compares server data against cloud data to find files that need to be uploaded to the cloud.
    /// </summary>

    public static List<FileData> ServerHasMoreData(string cloudData, string serverData)
    {
        // Deserialize the JSON strings into lists of FileData objects
        List<FileData>? cloudFiles = JsonSerializer.Deserialize<List<FileData>>(cloudData) ?? [];
        List<FileData>? serverFiles = JsonSerializer.Deserialize<List<FileData>>(serverData) ?? [];

        // Remove entries with "N/A" values from both cloud and server files
        cloudFiles = RemoveNAEntries(cloudFiles);
        serverFiles = RemoveNAEntries(serverFiles);

        // Initialize a new list to hold files that need to be added to the cloud
        var newFilesToCloud = new List<FileData>();

        // Iterate over each file in the server list
        foreach (FileData serverFile in serverFiles)
        {
            // Check if the server file exists in the cloud
            foreach (int index in Enumerable.Range(0, serverFile.Name?.Count ?? 0))
            {
                bool fileExists = cloudFiles.Any(cloudFile =>
                    cloudFile.Name != null && cloudFile.Name.Contains(serverFile.Name?[index] ?? "") &&
                    cloudFile.FileVersion != null && cloudFile.FileVersion.Contains(serverFile.FileVersion?[index] ?? ""));

                // If the file does not exist in the cloud, add it to the list
                if (!fileExists)
                {
                    newFilesToCloud.Add(new FileData {
                        Id = serverFile.Id != null && serverFile.Id.Count > index
                            ? [serverFile.Id[index]]
                            : ["N/A"],
                        Name = serverFile.Name != null && serverFile.Name.Count > index
                            ? [serverFile.Name[index]]
                            : ["N/A"],
                        Description = serverFile.Description != null && serverFile.Description.Count > index
                            ? [serverFile.Description[index]]
                            : ["N/A"],
                        FileVersion = serverFile.FileVersion != null && serverFile.FileVersion.Count > index
                            ? [serverFile.FileVersion[index]]
                            : ["N/A"],
                        LastModified = serverFile.LastModified != null && serverFile.LastModified.Count > index
                            ? [serverFile.LastModified[index]]
                            : ["N/A"],
                        CreatorName = serverFile.CreatorName != null && serverFile.CreatorName.Count > index
                            ? [serverFile.CreatorName[index]]
                            : ["N/A"],
                        CreatorMail = serverFile.CreatorMail != null && serverFile.CreatorMail.Count > index
                            ? [serverFile.CreatorMail[index]]
                            : ["N/A"]
                    });
                }

            }
        }

        return newFilesToCloud;
    }
    /// <summary>
    /// Compares cloud data against server data to find files that need to be send to the server.
    /// </summary>

    public static List<FileData> CloudHasMoreData(string cloudData, string serverData)
    {
        // Deserialize cloud and server data into lists of FileData objects
        List<FileData>? cloudFiles = JsonSerializer.Deserialize<List<FileData>>(cloudData);
        List<FileData>? serverFiles = JsonSerializer.Deserialize<List<FileData>>(serverData);

        // Remove entries with "N/A" values from both cloud and server files
        cloudFiles = RemoveNAEntries(cloudFiles ?? []);
        serverFiles = RemoveNAEntries(serverFiles ?? []);

        // Ensure both lists are not null
        cloudFiles ??= [];
        serverFiles ??= [];

        foreach (FileData cloudFile in cloudFiles)
        {
            // Find indices in cloudFile.Name that should be removed because they exist in serverFiles
            List<int> indicesToRemove = cloudFile.Name?
            .Select((name, index) => new { name, index })
            .Where(item => serverFiles
                .Any(serverFile => serverFile.Name != null && serverFile.Name.Contains(item.name) &&
                                    serverFile.FileVersion != null && serverFile.FileVersion.Contains(cloudFile.FileVersion?[item.index] ?? "")))
            .Select(item => item.index)
            .ToList() ?? [];

            // Remove the items at these indices across all fields in cloudFile
            foreach (int index in indicesToRemove.OrderByDescending(i => i))
            {
                // Remove data at the specified index in each field, if they are not null and have enough elements
                if (cloudFile.Name?.Count > index)
                {
                    cloudFile.Name.RemoveAt(index);
                }

                if (cloudFile.Id?.Count > index)
                {
                    cloudFile.Id.RemoveAt(index);
                }

                if (cloudFile.Description?.Count > index)
                {
                    cloudFile.Description.RemoveAt(index);
                }

                if (cloudFile.FileVersion?.Count > index)
                {
                    cloudFile.FileVersion.RemoveAt(index);
                }

                if (cloudFile.LastModified?.Count > index)
                {
                    cloudFile.LastModified.RemoveAt(index);
                }

                if (cloudFile.CreatorName?.Count > index)
                {
                    cloudFile.CreatorName.RemoveAt(index);
                }
            }
        }

        // Return only cloud files with remaining unique entries
        return cloudFiles.Where(file => file.Name != null && file.Name.Count != 0).ToList();

    }

    //save the file from cloud to server

    private void UpdateServerWithCloudData(string file)
    {
        SaveFileToServer(file);
        _logServiceViewModel.UpdateLogDetails("Server updated with each file new data from cloud");
    }

    private void SaveFileToServer(string file)
    {
        string destinationPath = SaveFileToServerMethod();
        File.WriteAllText(destinationPath, file);
        _logServiceViewModel.UpdateLogDetails("File successfully saved on server");

        string? content = Utils.ReadBinaryFile(destinationPath) ?? throw new Exception("Failed to read file");
        string? serializedContent = Utils.SerializeObject(content) ?? throw new Exception("Failed to serialize content");
        var fileContentToSend = new FileContent("CloudFiles.json", serializedContent);

        var fileContentsToSend = new List<FileContent>();
        fileContentsToSend?.Add(fileContentToSend);

        var dataPacketToSend = new DataPacket(
                        DataPacket.PacketType.Broadcast,
                        [fileContentToSend]
                        );

        // Serialize packet
        string? serializedPacket = Utils.SerializeObject(dataPacketToSend);
        if (serializedPacket != null)
        {
            _serverViewModel.GetServer().Broadcast(serializedPacket);
        }

    }
    private string NotificationOfCloudData(List<FileData> cloudFiles)
    {
        // Create the combined message for all files
        string combinedMessage = "";
        foreach (FileData cloudFile in cloudFiles)
        {
            combinedMessage += $"A new tool {cloudFile.Name?[0]} " +
                               $"of version: {cloudFile.FileVersion?[0]} " +
                               $"is available! Contact this mail id: {cloudFile.CreatorMail?[0]} for more info\n";
        }

        // Log the combined message
        return combinedMessage;

    }

    private static string SaveFileToServerMethod()
    {
        return Path.Combine(AppConstants.ToolsDirectory, $"{CloudFilesName}.json");
    }
}
