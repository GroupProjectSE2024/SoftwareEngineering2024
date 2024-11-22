/******************************************************************************
* Filename    = TestCloudViewModel.cs
*
* Author      = Karumudi Harika
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Unit Tests for CloudViewModel.cs
*****************************************************************************/
using ViewModel.UpdaterViewModel;
using SECloud.Services;
using Moq;
using Updater;

namespace TestsUpdater;

/// <summary>
/// Unit test class for CLoud. This class contains tests that simulate cloud events 
///between server and cloud.
/// </summary>
[TestClass]
public class TestCloudViewModel
{
    private Mock<CloudViewModel>? _cloudViewModel;
    private Mock<LogServiceViewModel>? _mockLogServiceViewModel;
    private Mock<ServerViewModel>? _mockServerViewModel;
    private Mock<CloudService>? _cloudService;
    private Mock<ToolAssemblyLoader>? _loader;
    private Mock<Server>? _server;
    /// <summary>
    /// Setup method to initialize the cloud instance before each test.
    /// </summary>

    [TestInitialize]
    public void Setup()
    {
        // Mock the dependencies
        _mockLogServiceViewModel = new Mock<LogServiceViewModel>();
        _cloudService = new Mock<CloudService>();
        _server = new Mock<Server>();
        _loader = new Mock<ToolAssemblyLoader>();
        _mockServerViewModel = new Mock<ServerViewModel>(_mockLogServiceViewModel, _loader, _server);
        // Create an instance of CloudViewModel with mocked dependencies
        _cloudViewModel = new Mock<CloudViewModel>(_mockLogServiceViewModel, _mockServerViewModel);
    }
    /// <summary>
    /// Tests the removal of invalid entries from the list, specifically those with "N/A" values in the Name or Id fields.
    /// </summary>

    [TestMethod]
    public void TestRemoveNAEntries()
    {
        // Arrange
        var files = new List<CloudViewModel.FileData>
        {
            new () { Name = ["ValidFile"], Id = ["1"] },
            new () { Name = ["N/A"], Id = ["2"] },
            new () { Name = ["AnotherValidFile"], Id = ["N/A"] }
        };

        // Act
        List<CloudViewModel.FileData> result = CloudViewModel.RemoveNAEntries(files);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("ValidFile", result[0]?.Name?[0]);
    }
    /// <summary>
    /// Tests if the ServerHasMoreData method correctly identifies files that are only present on the server, not in the cloud.
    /// </summary>

    [TestMethod]
    public void TestServerHasMoreDataAndIdentifiesServerOnlyFiles()
    {
        // Arrange
        string cloudData = "[]"; // Empty cloud
        string serverData = "[{\"Name\": [\"ServerFile\"], \"Id\": [\"1\"]}]";

        // Act
        List<CloudViewModel.FileData> result = CloudViewModel.ServerHasMoreData(cloudData, serverData);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("ServerFile", result[0]?.Name?[0]);
        Assert.AreEqual("1", result[0]?.Id?[0]);
    }
    /// <summary>
    /// Tests if the CloudHasMoreData method correctly identifies files that are only present in the cloud, not on the server.
    /// </summary>

    [TestMethod]
    public void TestCloudHasMoreDataAndIdentifiesCloudOnlyFiles()
    {
        // Arrange
        string serverData = "[]"; // Empty server
        string cloudData = "[{\"Name\": [\"CloudFile\"], \"Id\": [\"2\"]}]";

        // Act
        List<CloudViewModel.FileData> result = CloudViewModel.CloudHasMoreData(cloudData, serverData);

        // Assert
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("CloudFile", result[0]?.Name?[0]);
        Assert.AreEqual("2", result[0]?.Id?[0]);
    }
    /// <summary>
    /// Tests if the ServerHasMoreData method correctly filters out files that are only present on the server and not the cloud.
    /// </summary>

    [TestMethod]
    public void ServerHasMoreDataAndFiltersCorrectFiles()
    {
        // Arrange: Setup mock data for cloud and server
        string cloudData = "[{\"Id\": [\"1\"], \"Name\": [\"File1\"], \"FileVersion\": [\"1.0\"]}]";
        string serverData = "[{\"Id\": [\"2\"], \"Name\": [\"File2\"], \"FileVersion\": [\"1.0\"]}]";

        // Act: Find files missing in the cloud
        List<CloudViewModel.FileData> result = CloudViewModel.ServerHasMoreData(cloudData, serverData);

        // Assert: Ensure the correct files are identified for upload to the cloud
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("File2", result[0]?.Name?[0]);
    }
    /// <summary>
    /// Tests if the CloudHasMoreData method correctly filters out files that are only present in the cloud and not on the server.
    /// </summary>

    [TestMethod]
    public void CloudHasMoreDataAndFiltersCorrectFiles()
    {
        // Arrange: Setup mock data for cloud and server
        string cloudData = "[{\"Id\": [\"1\"], \"Name\": [\"File1\"], \"FileVersion\": [\"1.0\"]}]";
        string serverData = "[{\"Id\": [\"1\"], \"Name\": [\"File1\"], \"FileVersion\": [\"1.0\"]}]";

        // Act: Find files unique to the cloud
        List<CloudViewModel.FileData> result = CloudViewModel.CloudHasMoreData(cloudData, serverData);

        // Assert: Ensure there are no extra cloud files (as both cloud and server have the same data)
        Assert.AreEqual(0, result.Count);
    }
}
