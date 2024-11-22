/******************************************************************************
* Filename    = TestToolListViewModel.cs
*
* Author      = Garima Ranjan 
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Unit tests for ToolListViewModel.cs
*****************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Updater;
using ViewModel.UpdaterViewModel;

namespace TestsUpdater;

[TestClass]
public class TestToolListViewModel
{
    private Mock<IToolAssemblyLoader>? _mockDllLoader;
    private ToolListViewModel? _viewModel;
    // contains both v1 and v2 of a tool
    private string _testFolderPath = @"../../../TestingFolder";

    // contains v2 of a tool
    private readonly string _copyTestFolderPath = @"../../../CopyTestFolder";

    private class TestTraceListener : TraceListener
    {
        public List<string> Messages { get; } = [];

        public override void Write(string? message)
        {
            if (message != null)
            {
                Messages.Add(message);
            }
        }

        public override void WriteLine(string? message)
        {
            if (message != null)
            {
                Messages.Add(message);
            }
        }
    }

    private TestTraceListener? _traceListener;

    [TestInitialize]
    public void Setup()
    {
        _mockDllLoader = new Mock<IToolAssemblyLoader>();
        _ = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testFolderPath);

        // Add custom trace listener for capturing trace messages
        _traceListener = new TestTraceListener();
        Trace.Listeners.Add(_traceListener);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (_traceListener != null)
        {
            Trace.Listeners.Remove(_traceListener);
        }
    }

    [TestMethod]
    public void TestLoadAvailableToolsUseToolsDirectoryInTAppsConstant()
    {
        _viewModel = new ToolListViewModel();
        _viewModel.LoadAvailableTools();
    }

    [TestMethod]
    public void TestLoadAvailableToolsShouldPopulateAvailableToolsListWhenToolsAreAvailable()
    {
        string copyTestFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _copyTestFolderPath);

        _viewModel = new ToolListViewModel(copyTestFolderPath);
        _viewModel.LoadAvailableTools(copyTestFolderPath);

        Assert.IsNotNull(_viewModel.AvailableToolsList);
        Assert.AreEqual(1, _viewModel.AvailableToolsList.Count);

        Tool tool = _viewModel.AvailableToolsList[0];
        Assert.AreEqual("OtherExample", tool.Name);
        Assert.AreEqual("2.0.0", tool.Version);
    }

    [TestMethod]
    public void TestLoadAvailableToolsShouldReplaceOlderVersionWhenNewerVersionExists()
    {
        // TestingFolder contains both v1 and v2 of the same Tool
        string testFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testFolderPath);

        var viewModel = new ToolListViewModel(testFolderPath);

        viewModel.LoadAvailableTools(testFolderPath);
        ObservableCollection<Tool>? updatedTools = viewModel.AvailableToolsList;

        // Assert: Verify that the newer version replaced the older one
        Assert.AreEqual(1, updatedTools?.Count);
        if (updatedTools != null)
        {
            Tool updatedTool = updatedTools.First();
            Assert.AreEqual("OtherExample", updatedTool.Name);
            Assert.AreEqual("2.0.0", updatedTool.Version);
        }
    }

    [TestMethod]
    public void TestLoadAvailableToolsShouldFirePropertyChangedWhenToolsAreUpdated()
    {
        string testFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testFolderPath);

        _viewModel = new ToolListViewModel(testFolderPath);

        bool wasPropertyChangedFired = false;
        _viewModel.PropertyChanged += (sender, e) => {
            if (e.PropertyName == nameof(_viewModel.AvailableToolsList))
            {
                wasPropertyChangedFired = true;
            }
        };

        _viewModel.LoadAvailableTools(testFolderPath);

        Assert.IsTrue(wasPropertyChangedFired);
    }

    [TestMethod]
    public void TestLoadAvailableToolsShouldGenerateExpectedTraceLines()
    {
        // Arrange
        string testFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _testFolderPath);

        _viewModel = new ToolListViewModel(testFolderPath);

        // Act
        _viewModel.LoadAvailableTools(testFolderPath);

        // Assert: Verify trace messages
        Assert.IsTrue(_traceListener!.Messages.Any(msg => msg.Contains("[Updater] Replaced older version with new version for tool")),
            "Expected trace message for replacing older version was not found.");
        Assert.IsTrue(_traceListener.Messages.Any(msg => msg.Contains("[Updater] Added new tool")),
            "Expected trace message for adding new tool was not found.");
        Assert.IsTrue(_traceListener.Messages.Any(msg => msg.Contains("Available Tools information updated successfully")),
            "Expected trace message for successful update was not found.");
    }
}
