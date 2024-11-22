/******************************************************************************
* Filename    = TestToolAssemblyLoader.cs
*
* Author      = Garima Ranjan 
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Unit tests for ToolAssemblyLoader.cs
*****************************************************************************/
using Updater;

namespace TestsUpdater;

/// <summary>
/// Test class for testing the ToolAssemblyLoader class.
/// </summary>
[TestClass]
public class TestToolAssemblyLoader
{
    private readonly string _emptyTestFolderPath = @"EmptyTestingFolder";
    private readonly string _nonExistingFolder = @"DoesNotExistTestingFolder";
    private readonly string _testFolderPath = @"../../../TestingFolder";
    private ToolAssemblyLoader? _loader;
    /// <summary>
    /// Set up the testing environment by ensuring the empty test folder is created and clean.
    /// </summary>

    [TestInitialize]
    public void SetUp()
    {
        // Ensure the test directory exists and is clean
        if (Directory.Exists(_emptyTestFolderPath))
        {
            Directory.Delete(_emptyTestFolderPath, true);
        }
        Directory.CreateDirectory(_emptyTestFolderPath);
    }
    /// <summary>
    /// Clean up the testing environment by deleting the empty test folder after tests.
    /// </summary>

    [TestCleanup]
    public void CleanUp()
    {
        // Clean up test files
        if (Directory.Exists(_emptyTestFolderPath))
        {
            Directory.Delete(_emptyTestFolderPath, true);
        }
    }
    /// <summary>
    /// Test case to verify that an empty folder returns an empty dictionary when loading tools.
    /// </summary>

    [TestMethod]
    public void TestLoadToolsFromFolderEmptyFolderReturnsEmptyDictionary()
    {
        if (Directory.Exists(_nonExistingFolder))
        {
            Directory.Delete(_nonExistingFolder, true);
        }
        _loader = new ToolAssemblyLoader();
        Dictionary<string, List<string>> result = _loader.LoadToolsFromFolder(_nonExistingFolder);
        Assert.AreEqual(0, result.Count, "Expected empty dictionary for an empty folder.");
    }
    /// <summary>
    /// Test case to verify that non-DLL files are ignored when loading tools.
    /// </summary>

    [TestMethod]
    public void TestLoadToolsFromFolderIgnoresNonDllFiles()
    {
        _loader = new ToolAssemblyLoader();
        File.WriteAllText(Path.Combine(_emptyTestFolderPath, "test.txt"), "This is a test file.");
        Dictionary<string, List<string>> result = _loader.LoadToolsFromFolder(_emptyTestFolderPath);
        Assert.AreEqual(0, result.Count, "Expected empty dictionary when no DLL files are present.");
    }
    /// <summary>
    /// Test case to verify that valid DLL files are loaded correctly and return expected tool properties.
    /// </summary>

    [TestMethod]
    public void TestLoadToolsFromFolderValidDllWithIToolReturnsToolProperties()
    {
        _loader = new ToolAssemblyLoader();
        string lastUpdatedDate = DateTime.Today.ToString("yyyy-MM-dd");

        // Constructing the full path to the TestingFolder
        string testFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testFolderPath);
        _ = Path.Combine(testFolderPath, "ValidTool.dll");

        // Load tools from the folder
        Dictionary<string, List<string>> result = _loader.LoadToolsFromFolder(testFolderPath);

        // Verifying that the keys exist and check their values
        Assert.IsTrue(result.TryGetValue("Id", out List<string>? ids), "Key 'Id' not found.");
        Assert.AreEqual("4", ids.FirstOrDefault(), "Expected Id was not found.");

        Assert.IsTrue(result.TryGetValue("Name", out List<string>? names), "Key 'Name' not found.");
        Assert.AreEqual("OtherExample", names.FirstOrDefault(), "Expected Name was not found.");

        Assert.IsTrue(result.TryGetValue("Description", out List<string>? descriptions), "Key 'Description' not found.");
        Assert.AreEqual("OtherExample Description", descriptions.FirstOrDefault(), "Expected Description was not found.");

        Assert.IsTrue(result.TryGetValue("Version", out List<string>? versions), "Key 'Version' not found.");
        Assert.AreEqual("1.0.0", versions.FirstOrDefault(), "Expected Version was not found.");

        Assert.IsTrue(result.TryGetValue("IsDeprecated", out List<string>? isDeprecations), "Key 'IsDeprecated' not found.");
        Assert.AreEqual("True", isDeprecations.FirstOrDefault(), "Expected IsDeprecated value was not found.");

        Assert.IsTrue(result.TryGetValue("CreatorName", out List<string>? creatorNames), "Key 'CreatorName' not found.");
        Assert.AreEqual("OtherExample Creator", creatorNames.FirstOrDefault(), "Expected CreatorName was not found.");

        Assert.IsTrue(result.TryGetValue("LastUpdated", out List<string>? lastUpdatedDates), "Key 'LastUpdated' not found.");
        Assert.AreEqual(lastUpdatedDate, lastUpdatedDates.FirstOrDefault(), "Expected LastUpdated was not found.");

        Assert.IsTrue(result.TryGetValue("LastModified", out List<string>? lastModifiedDates), "Key 'LastModified' not found.");
        Assert.AreEqual("2023-11-10", lastModifiedDates.FirstOrDefault(), "Expected LastModified was not found.");

        Assert.IsTrue(result.TryGetValue("CreatorEmail", out List<string>? creatorEmails), "Key 'CreatorEmail' not found.");
        Assert.AreEqual("creatorcca@example.com", creatorEmails.FirstOrDefault(), "Expected CreatorEmail was not found.");
    }
    /// <summary>
    /// Test case to verify that an exception is thrown when attempting to load an invalid DLL file.
    /// </summary>
    [TestMethod]
    public void TestLoadToolsFromFolderHandlesInvalidDllFiles()
    {
        _loader = new ToolAssemblyLoader();
        string testFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../CopyTestFolder");
        _ = Path.Combine(testFolderPath, "InvalidDLL.dll");

        // Attempting to load tools from the folder containing the invalid DLL
        Dictionary<string, List<string>> result = _loader.LoadToolsFromFolder(testFolderPath);
        Assert.AreEqual(1, result["Id"].Count, "Invalid DLL files should not populate the toolPropertyMap.");
    }
}
