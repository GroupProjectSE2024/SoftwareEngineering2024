using System.Windows;
using Updater;
using ViewModel.UpdaterViewModel;

namespace TestsUpdater;

/// <summary>
/// Unit tests for LogServiceViewModel.
/// Tests include validation for property changes, visibility logic, and notification methods.
/// </summary>
[TestClass]
public class TestLogServiceViewModel
{
    private LogServiceViewModel _viewModel = new(); // Explicitly initializing to avoid nullability issues

    /// <summary>
    /// Setup method to initialize the ViewModel before each test.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _viewModel = new LogServiceViewModel(); // Explicitly reinitializing for each test
    }

    /// <summary>
    /// Test to verify that LogDetails updates correctly when UpdateLogDetails is called.
    /// </summary>
    [TestMethod]
    public void TestUpdateLogDetails()
    {
        // Arrange
        string testMessage = "Test log entry";
        string timestamp = DateTime.Now.ToString("HH:mm:ss dd-MM-yyyy");

        // Act
        _viewModel.UpdateLogDetails(testMessage);

        // Assert
        Assert.IsTrue(_viewModel.LogDetails.Contains(testMessage), "LogDetails should contain the test message.");
        Assert.IsTrue(_viewModel.LogDetails.Contains(timestamp), "LogDetails should contain the timestamp.");
    }

    /// <summary>
    /// Test to verify that NotificationMessage and NotificationVisible update correctly when ShowNotification is called.
    /// </summary>
    [TestMethod]
    public void TestShowNotification()
    {
        // Arrange
        string testNotification = "This is a test notification.";

        // Act
        _viewModel.ShowNotification(testNotification);

        // Assert
        Assert.AreEqual(testNotification, _viewModel.NotificationMessage, "NotificationMessage should match the input message.");
        Assert.IsTrue(_viewModel.NotificationVisible, "NotificationVisible should be true after showing a notification.");
    }

    /// <summary>
    /// Test to verify that notification visibility is reset when HideNotification is invoked.
    /// </summary>
    [TestMethod]
    public void TestHideNotification()
    {
        // Arrange
        _viewModel.ShowNotification("Test Notification");

        // Act
        System.Reflection.MethodInfo? hideNotificationMethod = typeof(LogServiceViewModel)
            .GetMethod("HideNotification", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (hideNotificationMethod != null)
        {
            hideNotificationMethod.Invoke(_viewModel, null);

            // Assert
            Assert.IsFalse(_viewModel.NotificationVisible, "NotificationVisible should be false after hiding the notification.");
        }
        else
        {
            Assert.Fail("HideNotification method not found.");
        }
    }


    /// <summary>
    /// Test to verify the behavior of IsLogExpanded and the corresponding LogDetailsVisibility.
    /// </summary>
    [TestMethod]
    public void TestIsLogExpandedBehavior()
    {
        // Act
        _viewModel.IsLogExpanded = true;

        // Assert
        Assert.AreEqual(Visibility.Visible, _viewModel.LogDetailsVisibility, "LogDetailsVisibility should be Visible when IsLogExpanded is true.");

        // Act
        _viewModel.IsLogExpanded = false;

        // Assert
        Assert.AreEqual(Visibility.Collapsed, _viewModel.LogDetailsVisibility, "LogDetailsVisibility should be Collapsed when IsLogExpanded is false.");
    }

    /// <summary>
    /// Test to verify ToolsDirectoryMessage returns the expected value.
    /// </summary>
    [TestMethod]
    public void TestToolsDirectoryMessage()
    {
        // Arrange
        string expectedMessage = $"New Tools can be added in {AppConstants.ToolsDirectory}";

        // Assert
        Assert.AreEqual(expectedMessage, _viewModel.ToolsDirectoryMessage, "ToolsDirectoryMessage should match the expected value.");
    }
}
