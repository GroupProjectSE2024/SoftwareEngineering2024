using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using WhiteboardGUI.Models;
using WhiteboardGUI.Services;


namespace Whiteboard;

[TestClass]
public class MoveShapeZIndexingTests
{
    private ObservableCollection<IShape> _shapes;
    private MoveShapeZIndexing _zIndexingService;

    [TestInitialize]
    public void TestInitialize()
    {
        // Initialize WPF Dispatcher for testing
        if (Application.Current == null)
        {
            new Application();
        }

        // Initialize shapes collection and MoveShapeZIndexing service
        _shapes = new ObservableCollection<IShape>();
        _zIndexingService = new MoveShapeZIndexing(_shapes);
    }

    /// <summary>
    /// Helper method to invoke the private method using reflection.
    /// </summary>
    private Geometry InvokeGetShapeStrokeGeometry(IShape shape)
    {
        // Get the private method info
        MethodInfo? methodInfo = typeof(MoveShapeZIndexing).GetMethod("GetShapeStrokeGeometry", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new MissingMethodException("The method 'GetShapeStrokeGeometry' was not found.");

        // Invoke the private method and return the result
        return (Geometry)methodInfo.Invoke(_zIndexingService, new object[] { shape });
    }

    #region MoveShapeBack Tests

    [TestMethod]
    public void MoveShapeBack_ValidShape_ShouldMoveToBack()
    {
        // Arrange
        var shape1 = new CircleShape { ShapeId = Guid.NewGuid(), CenterX = 50, CenterY = 50, RadiusX = 25, RadiusY = 25, Color = "Red" };
        var shape2 = new ScribbleShape
        {
            ShapeId = Guid.NewGuid(),
            UserID = 1.0,
            Color = "Blue",
            StrokeThickness = 2.0,
            LastModifierID = 1.0,
            IsSelected = false,
            Points = new List<Point>
            {
                new Point(10, 10),
                new Point(20, 20),
                new Point(30, 15),
                new Point(40, 25),
                new Point(50, 20)
            },
            ZIndex = 1
        };
        _shapes.Add(shape1);
        _shapes.Add(shape2);

        // Act
        _zIndexingService.MoveShapeBack(shape2);

        // Assert
        Assert.AreEqual(shape2, _shapes[0]);
        Assert.AreEqual(0, shape2.ZIndex);
        Assert.AreEqual(1, shape1.ZIndex);
    }

    [TestMethod]
    public void MoveShapeBack_NullShape_ShouldNotChangeCollection()
    {
        // Arrange
        var shape1 = new CircleShape { ShapeId = Guid.NewGuid(), CenterX = 50, CenterY = 50, RadiusX = 25, RadiusY = 25, Color = "Red" };
        _shapes.Add(shape1);
        var originalOrder = new ObservableCollection<IShape>(_shapes);

        // Act
        _zIndexingService.MoveShapeBack(null);

        // Assert
        CollectionAssert.AreEqual(originalOrder, _shapes);
    }

    [TestMethod]
    public void MoveShapeBack_ShapeNotInCollection_ShouldNotChangeCollection()
    {
        // Arrange
        var shape1 = new CircleShape { ShapeId = Guid.NewGuid(), CenterX = 50, CenterY = 50, RadiusX = 25, RadiusY = 25, Color = "Red" };
        var shape2 = new LineShape { ShapeId = Guid.NewGuid(), StartX = 0, StartY = 0, EndX = 100, EndY = 100, Color = "Blue" };
        _shapes.Add(shape1);

        // Act
        _zIndexingService.MoveShapeBack(shape2);

        // Assert
        Assert.AreEqual(shape1, _shapes[0]);
    }

    #endregion

    #region MoveShapeBackward Tests

    [TestMethod]
    public void MoveShapeBackward_WhenOverlapping_ShouldMoveBehindOverlappingShape()
    {
        // Arrange
        var shape1 = new CircleShape { ShapeId = Guid.NewGuid(), CenterX = 50, CenterY = 50, RadiusX = 25, RadiusY = 25, Color = "Red" };
        var shape2 = new CircleShape { ShapeId = Guid.NewGuid(), CenterX = 60, CenterY = 60, RadiusX = 25, RadiusY = 25, Color = "Blue" };
        _shapes.Add(shape1);
        _shapes.Add(shape2);

        // Act
        _zIndexingService.MoveShapeBackward(shape2);

        // Assert
        Assert.AreEqual(shape1, _shapes[0]);
        Assert.AreEqual(_shapes[1], shape2);
    }

    [TestMethod]
    public void MoveShapeBackward_WhenNoOverlap_ShouldMoveToBottom()
    {
        // Arrange
        var shape1 = new CircleShape { ShapeId = Guid.NewGuid(), CenterX = 50, CenterY = 50, RadiusX = 25, RadiusY = 25, Color = "Red" };
        var shape2 = new CircleShape { ShapeId = Guid.NewGuid(), CenterX = 200, CenterY = 200, RadiusX = 25, RadiusY = 25, Color = "Blue" };
        _shapes.Add(shape1);
        _shapes.Add(shape2);

        // Act
        _zIndexingService.MoveShapeBackward(shape2);

        // Allow dispatcher to process
        Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);

        // Assert
        Assert.AreEqual(shape1, _shapes[0]);
        Assert.AreEqual(_shapes[1], shape2);
        //Assert.AreEqual(2, shape1.ZIndex);
    }

    [TestMethod]
    public void MoveShapeBackward_NullShape_ShouldNotChangeCollection()
    {
        // Arrange
        var shape1 = new CircleShape { ShapeId = Guid.NewGuid(), CenterX = 50, CenterY = 50, RadiusX = 25, RadiusY = 25, Color = "Red" };
        _shapes.Add(shape1);
        var originalOrder = new ObservableCollection<IShape>(_shapes);

        // Act
        _zIndexingService.MoveShapeBackward(null);

        // Assert
        CollectionAssert.AreEqual(originalOrder, _shapes);
    }

    [TestMethod]
    public void MoveShapeBackward_ShapeNotInCollection_ShouldNotChangeCollection()
    {
        // Arrange
        var shape1 = new CircleShape { ShapeId = Guid.NewGuid(), CenterX = 50, CenterY = 50, RadiusX = 25, RadiusY = 25, Color = "Red" };
        var shape2 = new LineShape { ShapeId = Guid.NewGuid(), StartX = 0, StartY = 0, EndX = 100, EndY = 100, Color = "Blue" };
        _shapes.Add(shape1);

        // Act
        _zIndexingService.MoveShapeBackward(shape2);

        // Assert
        Assert.AreEqual(shape1, _shapes[0]);
    }

    #endregion

    #region UpdateZIndices Test

    [TestMethod]
    public void UpdateZIndices_ShouldSetCorrectZIndices()
    {
        // Arrange
        var shape1 = new CircleShape { ShapeId = Guid.NewGuid(), CenterX = 50, CenterY = 50, RadiusX = 25, RadiusY = 25, Color = "Red" };
        var shape2 = new LineShape { ShapeId = Guid.NewGuid(), StartX = 0, StartY = 0, EndX = 100, EndY = 100, Color = "Blue" };
        _shapes.Add(shape1);
        _shapes.Add(shape2);

        // Act
        _zIndexingService.UpdateZIndices();

        // Assert
        Assert.AreEqual(0, shape1.ZIndex);
        Assert.AreEqual(1, shape2.ZIndex);
    }
    #endregion

    [TestMethod]
    public void GetShapeStrokeGeometry_WhenWideningFails_ShouldReturnNull()
    {
        // Arrange
        var shape = new CircleShape
        {
            ShapeId = Guid.NewGuid(),
            CenterX = 50,
            CenterY = 50,
            RadiusX = 25,
            RadiusY = 25,
            StrokeThickness = double.NaN, // Invalid thickness to force an exception
            Color = "Rede"
        };

        // Act
        Geometry geometry = InvokeGetShapeStrokeGeometry(shape);

        // Assert
        Assert.IsNull(geometry); // Should return null due to exception handling
    }

    [TestMethod]
    public void GetShapeStrokeGeometry_UnsupportedShape_ShouldReturnNull()
    {
        // Arrange
        var unsupportedShape = new TextboxModel
        {
            ShapeId = Guid.NewGuid(),
            X = 10,
            Y = 10,
            Width = 100,
            Height = 50,
            StrokeThickness = 2,
            Color = "Orange"
        };

        // Act
        Geometry geometry = InvokeGetShapeStrokeGeometry(unsupportedShape);

        // Assert
        Assert.IsNull(geometry); // Unsupported shapes should return null
    }

    [TestMethod]
    public void MoveShapeBackward_WhenShapeIsAlreadyAtBottom_ShouldNotChangeOrder()
    {
        // Arrange
        var shape1 = new CircleShape { ShapeId = Guid.NewGuid(), CenterX = 50, CenterY = 50, RadiusX = 25, RadiusY = 25, Color = "Red" };
        var shape2 = new LineShape { ShapeId = Guid.NewGuid(), StartX = 0, StartY = 0, EndX = 100, EndY = 100, Color = "Blue" };
        _shapes.Add(shape1);
        _shapes.Add(shape2);

        // Act
        _zIndexingService.MoveShapeBackward(shape1); // shape1 is already at index 0

        // Assert
        Assert.AreEqual(shape1, _shapes[0]);
        Assert.AreEqual(shape2, _shapes[1]);
    }

    [TestMethod]
    public void GetShapeStrokeGeometry_LineShape_ShouldReturnNonNullGeometry()
    {
        // Arrange
        var lineShape = new LineShape
        {
            ShapeId = Guid.NewGuid(),
            StartX = 10,
            StartY = 10,
            EndX = 100,
            EndY = 100,
            StrokeThickness = 2,
            Color = "Blue"
        };

        // Act
        Geometry geometry = InvokeGetShapeStrokeGeometry(lineShape);

        // Assert
        Assert.IsNotNull(geometry); // Geometry should not be null for a valid LineShape
    }

    [TestMethod]
    public void GetShapeStrokeGeometry_ScribbleShape_WithPoints_ShouldReturnNonNullGeometry()
    {
        // Arrange
        var scribbleShape = new ScribbleShape
        {
            ShapeId = Guid.NewGuid(),
            Points = new List<Point>
    {
        new Point(0, 0),
        new Point(50, 50),
        new Point(100, 0)
    },
            StrokeThickness = 2,
            Color = "Green"
        };

        // Act
        Geometry geometry = InvokeGetShapeStrokeGeometry(scribbleShape);

        // Assert
        Assert.IsNotNull(geometry); // Geometry should not be null when ScribbleShape has points
    }

    [TestMethod]
    public void GetShapeStrokeGeometry_ScribbleShape_NoPoints_ShouldReturnNull()
    {
        // Arrange
        var scribbleShape = new ScribbleShape
        {
            ShapeId = Guid.NewGuid(),
            Points = new List<Point>(), // No points
            StrokeThickness = 2,
            Color = "Green"
        };

        // Act
        Geometry geometry = InvokeGetShapeStrokeGeometry(scribbleShape);

        // Assert
        Assert.IsNull(geometry); // Geometry should be null when ScribbleShape has no points
    }

    [TestMethod]
    public void MoveShapeBackward_WithOverlappingLineShape_ShouldMoveBehindOverlappingShape()
    {
        // Arrange
        var lineShape = new LineShape
        {
            ShapeId = Guid.NewGuid(),
            StartX = 10,
            StartY = 10,
            EndX = 100,
            EndY = 100,
            StrokeThickness = 2,
            Color = "Blue"
        };
        var scribbleShape = new ScribbleShape
        {
            ShapeId = Guid.NewGuid(),
            Points = new List<Point>
    {
        new Point(50, 50),
        new Point(150, 150)
    },
            StrokeThickness = 2,
            Color = "Green"
        };
        _shapes.Add(lineShape);
        _shapes.Add(scribbleShape);

        // Act
        _zIndexingService.MoveShapeBackward(scribbleShape);

        // Assert
        Assert.AreEqual(scribbleShape, _shapes[0]);
        Assert.AreEqual(0, scribbleShape.ZIndex);
        Assert.AreEqual(1, lineShape.ZIndex);
    }

    [TestMethod]
    public void MoveShapeBackward_WithNonOverlappingLineShape_ShouldMoveNotMove()
    {
        // Arrange
        var lineShape = new LineShape
        {
            ShapeId = Guid.NewGuid(),
            StartX = 200,
            StartY = 200,
            EndX = 300,
            EndY = 300,
            StrokeThickness = 2,
            Color = "Blue"
        };
        var scribbleShape = new ScribbleShape
        {
            ShapeId = Guid.NewGuid(),
            Points = new List<Point>
    {
        new Point(0, 0),
        new Point(50, 50)
    },
            StrokeThickness = 2,
            Color = "Green"
        };
        _shapes.Add(scribbleShape);
        _shapes.Add(lineShape);

        // Act
        _zIndexingService.MoveShapeBackward(lineShape);

        // Assert
        Assert.AreEqual(lineShape, _shapes[1]);
        Assert.AreEqual(scribbleShape, _shapes[0]);
    }

    [TestMethod]
    public void MoveShapeBackward_WithEmptyScribbleShape_ShouldMoveNotMove()
    {
        // Arrange
        var lineShape = new LineShape
        {
            ShapeId = Guid.NewGuid(),
            StartX = 100,
            StartY = 100,
            EndX = 200,
            EndY = 200,
            StrokeThickness = 2,
            Color = "Blue"
        };
        var scribbleShape = new ScribbleShape
        {
            ShapeId = Guid.NewGuid(),
            Points = new List<Point>(), // Empty points
            StrokeThickness = 2,
            Color = "Green"
        };
        _shapes.Add(lineShape);
        _shapes.Add(scribbleShape);

        // Act
        _zIndexingService.MoveShapeBackward(scribbleShape);

        // Assert
        Assert.AreEqual(scribbleShape, _shapes[1]);
        Assert.AreEqual(lineShape, _shapes[0]);
    }

}
