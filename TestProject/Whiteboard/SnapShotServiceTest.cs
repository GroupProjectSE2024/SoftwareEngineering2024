using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SECloud.Services;
using SECloud.Models;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using WhiteboardGUI.Models;
using WhiteboardGUI.Services;
using Microsoft.Extensions.Logging;
using SECloud.Interfaces;
using FileCloner.ViewModels;
using WhiteboardGUI.ViewModel;

namespace Whiteboard;

[TestClass]
public class SnapShotServiceTests
{
    private Mock<ICloud> _mockCloudService;
    private Mock<NetworkingService> _mockNetworkingService;
    private Mock<RenderingService> _mockRenderingService;
    private Mock<UndoRedoService> _mockUndoRedoService;
    private SnapShotService _snapShotService;
    private ObservableCollection<IShape> _shapes;
    private WhiteboardGUI.ViewModel.MainPageViewModel _mainPageViewModel;

    [TestInitialize]
    public void Setup()
    {

        // Initialize shapes collection
        _shapes = new ObservableCollection<IShape>();
        _mainPageViewModel = new WhiteboardGUI.ViewModel.MainPageViewModel();
        // Mock dependencies
        _mockNetworkingService = new Mock<NetworkingService>(new ReceivedDataService(0, _mainPageViewModel),1);
        _mockUndoRedoService = new Mock<UndoRedoService>();
        _mockCloudService = new Mock<ICloud>();
         

        // Create a real RenderingService
        _mockRenderingService = new Mock<RenderingService>(_mockNetworkingService.Object, _mockUndoRedoService.Object, _shapes,0,"",_mainPageViewModel);

        // Initialize SnapShotService with mocked ICloud
        _snapShotService = new SnapShotService(
            _mockNetworkingService.Object,
            _mockRenderingService.Object,
            _shapes,
            _mockUndoRedoService.Object,
            "email"
        );

        // Use reflection to replace the private cloudService field with the mock
        FieldInfo? cloudServiceField = typeof(SnapShotService).GetField("_cloudService", BindingFlags.NonPublic | BindingFlags.Instance);
        cloudServiceField.SetValue(_snapShotService, _mockCloudService.Object);
    }

    [TestMethod]
    public async Task UploadSnapShot_ShouldUploadSuccessfully()
    {
        // Arrange: Mock the UploadAsync method
        _mockCloudService.Setup(cs => cs.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
                         .ReturnsAsync(new ServiceResponse<string> { Data = "Success", Success = true });

        string snapShotFileName = "test_snapshot";
        var shapes = new ObservableCollection<IShape>
        {
            new LineShape { StartX = 10, StartY = 20, EndX = 30, EndY = 40 }
        };

        // Act
        await _snapShotService.UploadSnapShot(snapShotFileName, shapes, true);

        // Assert: Verify the snapshot was added to the Snaps dictionary
        FieldInfo? snapShotDownloadItemsField = typeof(SnapShotService).GetField("_snapShotDownloadItems", BindingFlags.NonPublic | BindingFlags.Instance);
        var snapShotDownloadItems = (List<SnapShotDownloadItem>)snapShotDownloadItemsField.GetValue(_snapShotService);
        Assert.IsTrue(snapShotDownloadItems.Any(item => item.FileName == snapShotFileName));

        // Verify UploadAsync was called
        _mockCloudService.Verify(cs => cs.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task UploadSnapShot_ShouldFailWhenLimitExceeded()
    {
        // Arrange: Mock UploadAsync to simulate immediate response
        _mockCloudService.Setup(cs => cs.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
                         .ReturnsAsync(new ServiceResponse<string> { Data = "Success", Success = true });

        // Ensure Dispatcher is initialized
        if (System.Windows.Application.Current == null)
        {
            new System.Windows.Application();
        }

        // Clear the OnSnapShotUploaded event to prevent any hanging invocation
        FieldInfo? onSnapShotUploadedField = typeof(SnapShotService).GetField("OnSnapShotUploaded", BindingFlags.NonPublic | BindingFlags.Instance);
        onSnapShotUploadedField.SetValue(_snapShotService, null);

        // Add enough entries to exceed the limit
        FieldInfo? snapShotDownloadItemsField = typeof(SnapShotService).GetField("_snapShotDownloadItems", BindingFlags.NonPublic | BindingFlags.Instance);
        var snapShotDownloadItems = new List<SnapShotDownloadItem>();

        for (int i = 0; i < 10; i++)
        {
            DateTime dateTime = DateTime.Now;
            snapShotDownloadItems.Add(new SnapShotDownloadItem($"snapshot{i}", DateTime.Now));
        }
        snapShotDownloadItemsField.SetValue(_snapShotService, snapShotDownloadItems);

        string snapShotFileName = "test_snapshot";

        // Act
        await _snapShotService.UploadSnapShot(snapShotFileName, _shapes, true);

        // Assert: Verify that the number of snaps does not exceed the limit
        var updatedSnaps = (List<SnapShotDownloadItem>)snapShotDownloadItemsField.GetValue(_snapShotService);
        Assert.IsTrue(updatedSnaps.Count <= 5);
        Assert.IsTrue(updatedSnaps.Any(item => item.FileName == snapShotFileName));
    }

    [TestMethod]
    public void DownloadSnapShot_ShouldClearShapesAndAddNewOnes()
    {
        // Arrange
        DateTime dateTime = DateTime.Now;
        var selectedDownloadItem = new SnapShotDownloadItem("snapshot1", dateTime);
        var snapShotShapes = new ObservableCollection<IShape>
    {
        new LineShape { StartX = 10, StartY = 20, EndX = 30, EndY = 40 }
    };

        // Access and set private Snaps dictionary
        FieldInfo? snapShotDownloadItemsField = typeof(SnapShotService).GetField("_snapShotDownloadItems", BindingFlags.NonPublic | BindingFlags.Instance);

        var snapShotDownloadItems = new List<SnapShotDownloadItem> {
                new SnapShotDownloadItem("snapshot1",dateTime),
                new SnapShotDownloadItem("snapshot2",DateTime.Now)
                };

        snapShotDownloadItemsField.SetValue(_snapShotService, snapShotDownloadItems);

        var mockStream = new MemoryStream();
        var mockSnapShot = new SnapShot {
            _shapes = snapShotShapes
        };
        string serializedSnapShot = SerializationService.SerializeSnapShot(mockSnapShot);
        var writer = new StreamWriter(mockStream);
        writer.Write(serializedSnapShot);
        writer.Flush();
        mockStream.Position = 0;

        // Mock RenderShape calls
        _mockRenderingService.Setup(rs => rs.RenderShape(It.IsAny<IShape>(), "DOWNLOAD"));
        _mockRenderingService.Setup(rs => rs.RenderShape(null, "CLEAR"));
        _mockCloudService.Setup(da => da.DownloadAsync(It.IsAny<string>()))
            .ReturnsAsync(new ServiceResponse<Stream>{
                Success = true,
                Data = mockStream
            });

        // Act
        _snapShotService.DownloadSnapShot(selectedDownloadItem);

        // Assert
        Assert.AreEqual(snapShotShapes.Count, _shapes.Count, "Shapes should match the downloaded snapshot.");
        Assert.AreEqual(1, _shapes.Count);
        _mockRenderingService.Verify(rs => rs.RenderShape(It.IsAny<IShape>(), "DOWNLOAD"), Times.Exactly(snapShotShapes.Count));
        _mockRenderingService.Verify(rs => rs.RenderShape(null, "CLEAR"), Times.Once);
        Assert.AreEqual(0, _mockUndoRedoService.Object._redoList.Count, "Redo list should be cleared.");
        Assert.AreEqual(0, _mockUndoRedoService.Object._undoList.Count, "Undo list should be cleared.");
    }

    [TestMethod]
    public void IsValidFilename_ShouldReturnCorrectValidation()
    {
        // Arrange
        FieldInfo? snapShotDownloadItemsField = typeof(SnapShotService).GetField("_snapShotDownloadItems", BindingFlags.NonPublic | BindingFlags.Instance);
        var snapShotDownloadItems = new List<SnapShotDownloadItem>
    {
        { new SnapShotDownloadItem("valid_filename",DateTime.Now) }
    };
        snapShotDownloadItemsField.SetValue(_snapShotService, snapShotDownloadItems);

        // Act & Assert
        Assert.IsFalse(_snapShotService.IsValidFilename("valid_filename"), "Filename already exists, should return false.");
        Assert.IsTrue(_snapShotService.IsValidFilename("new_filename"), "Filename does not exist, should return true.");
    }

    [TestMethod]
    public async Task GetSnapShot_ShouldReturnCorrectSnapshot()
    {
        // Arrange
        FieldInfo? snapShotDownloadItemsField = typeof(SnapShotService).GetField("_snapShotDownloadItems", BindingFlags.NonPublic | BindingFlags.Instance);

        var snapShotShapes = new ObservableCollection<IShape>
    {
        new LineShape { StartX = 10, StartY = 20, EndX = 30, EndY = 40 }
    };
        DateTime dateTime = DateTime.Now;
        var snapShotDownloadItems = new List<SnapShotDownloadItem> { new SnapShotDownloadItem("SnapName", dateTime) };

        snapShotDownloadItemsField.SetValue(_snapShotService, snapShotDownloadItems);

        var mockStream = new MemoryStream();
        var mockSnapShot = new SnapShot {
            _shapes = snapShotShapes
        };
        string serializedSnapShot = SerializationService.SerializeSnapShot(mockSnapShot);
        var writer = new StreamWriter(mockStream);
        writer.Write(serializedSnapShot);
        writer.Flush();
        mockStream.Position = 0;

        _mockCloudService.Setup(da => da.DownloadAsync(It.IsAny<string>()))
            .ReturnsAsync(new ServiceResponse<Stream> {
                Success = true,
                Data = mockStream
            });

        // Use reflection to invoke the private getSnapShot method
        MethodInfo? getSnapShotMethod = typeof(SnapShotService).GetMethod("GetSnapShot", BindingFlags.NonPublic | BindingFlags.Instance);
        var task = (Task<ObservableCollection<IShape>>)getSnapShotMethod.Invoke(_snapShotService, new object[] { new SnapShotDownloadItem("SnapName", dateTime) });
        ObservableCollection<IShape> result = await task;
        // Assert
        Assert.IsNotNull(result, "getSnapShot should return a valid snapshot.");
        Assert.AreEqual(snapShotShapes.Count, result.Count, "Snapshot should match the expected shapes.");
    }

    [TestMethod]
    public void AddShapes_ShouldAddShapesToShapesCollection()
    {
        // Arrange
        var snapShotShapes = new ObservableCollection<IShape>
    {
        new LineShape { StartX = 10, StartY = 20, EndX = 30, EndY = 40 },
        new LineShape { StartX = 50, StartY = 60, EndX = 70, EndY = 80 }
    };

        // Use reflection to invoke the private addShapes method
        MethodInfo? addShapesMethod = typeof(SnapShotService).GetMethod("AddShapes", BindingFlags.NonPublic | BindingFlags.Instance);

        // Mock RenderShape calls
        _mockRenderingService.Setup(rs => rs.RenderShape(It.IsAny<IShape>(), "DOWNLOAD"));

        // Act
        addShapesMethod.Invoke(_snapShotService, new object[] { snapShotShapes });

        // Assert
        Assert.AreEqual(snapShotShapes.Count, _shapes.Count, "Shapes collection should match the snapshot shapes.");
        _mockRenderingService.Verify(rs => rs.RenderShape(It.IsAny<IShape>(), "DOWNLOAD"), Times.Exactly(snapShotShapes.Count));
    }

    [TestMethod]
    public void CheckLimit_ShouldDeleteExcessSnapshots()
    {
        // Arrange: Set up Snaps with a large number of snapshots
        FieldInfo? snapShotDownloadItemsField = typeof(SnapShotService).GetField("_snapShotDownloadItems", BindingFlags.NonPublic | BindingFlags.Instance);

        var snapShotDownloadItems = new List<SnapShotDownloadItem>();

        for (int i = 0; i < 10; i++)
        {
            snapShotDownloadItems.Add(new SnapShotDownloadItem(i.ToString(), DateTime.Now));
        }
        snapShotDownloadItemsField.SetValue(_snapShotService, snapShotDownloadItems);

        // Act: Call CheckLimit
        _snapShotService.GetType()
                        .GetMethod("CheckLimit", BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(_snapShotService, null);

        // Assert: Verify that there are only 5 snapshots left
        int remainingSnaps = ((List<SnapShotDownloadItem>)snapShotDownloadItemsField.GetValue(_snapShotService)).Count;
        Assert.IsTrue(remainingSnaps <= 5);
    }

    [TestMethod]
    public async Task GetSnaps_ShouldReturnValidSnapshotFileNames()
    {
        DateTime dateTime1 = DateTime.Now;
        DateTime dateTime2 = DateTime.Now;
        // Arrange: Create and serialize SnapShot objects
        var snapshot1 = new SnapShot {
            _userID = "user_1",
            _fileName = "snapshot1",
            _dateTime = dateTime1,
            _shapes = new ObservableCollection<IShape>
    {
        new LineShape { StartX = 10, StartY = 20, EndX = 30, EndY = 40 }
    }
        };

        var snapshot2 = new SnapShot {
            _userID = "user_2",
            _fileName = "snapshot2",
            _dateTime = dateTime2,
            _shapes = new ObservableCollection<IShape>
    {
        new LineShape { StartX = 50, StartY = 60, EndX = 70, EndY = 80 }
    }
        };

        var jsonMatches = new List<JsonSearchMatch>
{
    new JsonSearchMatch
    {
        FileName = "snapshot1.json",
        Content = JsonDocument.Parse(SerializationService.SerializeSnapShot(snapshot1)).RootElement
    },
    new JsonSearchMatch
    {
        FileName = "snapshot2.json",
        Content = JsonDocument.Parse(SerializationService.SerializeSnapShot(snapshot2)).RootElement
    }
};

        _mockCloudService.Setup(cs => cs.SearchJsonFilesAsync(It.IsAny<string>(), It.IsAny<string>()))
                         .ReturnsAsync(new ServiceResponse<JsonSearchResponse> {
                             Data = new JsonSearchResponse { Matches = jsonMatches },
                             Success = true
                         });

        // Act
        List<SnapShotDownloadItem> result = await _snapShotService.GetSnaps("", true);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(item => item.FileName == "snapshot1"));
        Assert.IsTrue(result.Any(item => item.FileName == "snapshot2"));
    }

}
