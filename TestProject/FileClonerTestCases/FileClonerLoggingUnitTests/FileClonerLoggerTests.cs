/******************************************************************************
 * Filename    = FileClonerLoggerUnitTests.cs
 *
 * Author(s)   = Neeraj Krishna N
 * 
 * Project     = FileClonerTestCases
 *
 * Description = UnitTests for FileClonerLogger
 *****************************************************************************/
using FileCloner.FileClonerLogging;

namespace FileClonerTestCases;


/// <summary>
/// Unit tests for the FileClonerLogger class.
/// Ensures logging behavior, including logging information and error messages, works as expected.
/// </summary>
[TestClass]
public class FileClonerLoggerTests
{
    private FileClonerLogger? _logger;
    private string? _logFile;

    /// <summary>
    /// Sets up the test environment before each test method.
    /// Initializes the logger with a specific module name.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _logger = new("FileClonerLoggerUnitTests");
    }

    /// <summary>
    /// Tests the logging functionality of the FileClonerLogger class.
    /// Verifies both informational and error logs are written correctly to the log file.
    /// </summary>
    [TestMethod]
    public void LoggingTest()
    {
        // Ensure logger instance is initialized
        Assert.IsNotNull(_logger);

        // Test writing an informational log
        string infoLog = "Logging Info to Log File";
        _logger.Log(infoLog);
        _logFile = _logger.LogFile;

        // Read the log file and verify the content includes the informational log
        string logFileContents = File.ReadAllText(_logFile);
        bool result = logFileContents.Contains(infoLog);
        Assert.IsTrue(result, "Log file does not contain the expected informational log entry");

        // Test writing an error log
        string errorLog = "Error Log info to log file";
        _logger.Log(errorLog, isErrorMessage: true);

        // Read the updated log file and verify it includes the error log with the "ERROR" prefix
        logFileContents = File.ReadAllText(_logFile);
        result = logFileContents.Contains(errorLog) && logFileContents.Contains("ERROR");
        Assert.IsTrue(result, "Log file does not contain the expected error log entry");
    }

    /// <summary>
    /// Cleans up the test environment after each test method.
    /// Deletes the log file created during the test.
    /// </summary>
    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_logFile))
        {
            File.Delete(_logFile);
        }
    }
}

