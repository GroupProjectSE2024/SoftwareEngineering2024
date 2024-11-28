using FileCloner.ViewModels;
namespace FileClonerTestCases;

[TestClass]
public class MainPageViewModelTests
{
    private string _testFolderPath;

    [TestInitialize]
    public void Setup()
    {
        // Create a test folder path for isolated testing
        _testFolderPath = Path.Combine(Path.GetTempPath(), "FileClonerTest");
        Directory.CreateDirectory(_testFolderPath);

        // Update Constants.DefaultFolderPath temporarily for testing

    }

    [TestCleanup]
    public void Cleanup()
    {
        // Clean up the test directory after testing
        if (Directory.Exists(_testFolderPath))
        {
            Directory.Delete(_testFolderPath, true);
        }
    }


    [TestMethod]
    public void Constructor_SubscribesToCheckBoxClickEvent()
    {
        // Act
        var viewModel = new MainPageViewModel();

        // Assert
        System.Reflection.EventInfo? eventField = typeof(Node).GetEvent("CheckBoxClickEvent");
        Assert.IsNotNull(eventField, "CheckBoxClickEvent should be subscribed in the constructor.");
    }
}
