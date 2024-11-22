using Moq;
using ViewModel.UpdaterViewModel;
using Updater;
using System.Diagnostics;

namespace TestsUpdater;

[TestClass]
public class TestServerViewModel
{
    private Mock<LogServiceViewModel>? _mockLogServiceViewModel;
    private Mock<ToolAssemblyLoader>? _mockLoader;
    private ServerViewModel? _serverViewModel;
    private readonly string _testFolderPath = @"../../../TestingFolder";
    private string? _validTestFolderPath;

    [TestInitialize]
    public void TestInitialize()
    {
        _validTestFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testFolderPath);

        // Arrange: Mock LogServiceViewModel and ToolAssemblyLoader
        _mockLogServiceViewModel = new Mock<LogServiceViewModel>();
        _mockLoader = new Mock<ToolAssemblyLoader>();

        // Act: Create an instance of ServerViewModel using the mocks
        _serverViewModel = new ServerViewModel(_mockLogServiceViewModel.Object, _mockLoader.Object);
    }

    [TestMethod]
    public void Constructor_ShouldInitializeServer_WhenCalled()
    {
        Assert.IsNotNull(_serverViewModel);
    }

    [TestMethod]
    public void GetServerInstance()
    {
        if (_serverViewModel != null)
        {
            Server server = _serverViewModel.GetServer();
            Assert.IsNotNull(server);
        }
    }

    [TestMethod]
    public void TestAddLogMessageShouldUpdateLogWhenCalled()
    {
        string logMessage = "Test log message";

        if (_serverViewModel != null && _mockLogServiceViewModel != null)
        {
            _serverViewModel.AddLogMessage(logMessage);

            _mockLogServiceViewModel.Verify(log => log.UpdateLogDetails(It.Is<string>(msg => msg == logMessage)), Times.Once);
        }
    }

    [TestMethod]
    public void TestBroadcastToClientsShouldLogMessageWhenCalled()
    {
        string testFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../CopyTestFolder");
        string filePath = Path.Combine(testFolderPath, "InvalidDLL.dll");

        string fileName = "InvalidDLL.dll";

        if (_serverViewModel != null && _mockLogServiceViewModel != null)
        {
            // Mock the method that might read the file or perform actions on it
            _mockLogServiceViewModel.Setup(log => log.UpdateLogDetails(It.IsAny<string>())).Verifiable();

            _serverViewModel.BroadcastToClients(filePath, fileName);

            // Assert: Verify that the log message "Sending files to all connected clients" was logged
            _mockLogServiceViewModel.Verify(log => log.UpdateLogDetails(It.Is<string>(msg => msg == "Sending files to all connected clients")), Times.Once);
        }
    }

    [TestMethod]
    public void TestGetServerDataShouldLogErrorWhenDirectoryDoesNotExist()
    {
        string invalidFolderPath = @"C:\invalidFolder";

        // Capture Trace output
        using var stringWriter = new System.IO.StringWriter();
        Trace.Listeners.Add(new TextWriterTraceListener(stringWriter));

        if (_serverViewModel != null)
        {
            string? result = _serverViewModel.GetServerData(invalidFolderPath);


            // Check that the error message was logged
            string output = stringWriter.ToString();
            Assert.IsTrue(output.Contains("Server directory not found."), "Expected log message not found.");

            // Also, ensure it returns an empty list (since the folder doesn't exist)
            Assert.AreEqual("[]", result);
        }
    }
}
