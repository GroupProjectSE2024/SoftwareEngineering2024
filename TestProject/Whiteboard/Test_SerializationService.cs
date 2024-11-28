using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WhiteboardGUI.Models;
using WhiteboardGUI.Services;

namespace Whiteboard;

[TestClass]
public class Test_SerializationService
{
    private CircleShape _circleShape;
    private LineShape _lineShape;
    private ScribbleShape _scribbleShape;
    private TextShape _textShape;
    private ObservableCollection<IShape> _shapes;
    private SnapShot _snapShot;

    [TestInitialize]
    public void Setup()
    {
        _circleShape = new CircleShape
        {
            ShapeId = Guid.NewGuid(),
            UserID = 1234.0,
            Color = "Red",
            StrokeThickness = 2.0,
            CenterX = 50,
            CenterY = 50,
            RadiusX = 20,
            RadiusY = 20
        };

        _lineShape = new LineShape
        {
            ShapeId = Guid.NewGuid(),
            UserID = 1234.0,
            Color = "Blue",
            StrokeThickness = 1.5,
            StartX = 10,
            StartY = 10,
            EndX = 100,
            EndY = 100
        };

        _scribbleShape = new ScribbleShape
        {
            ShapeId = Guid.NewGuid(),
            UserID = 1234.0,
            Color = "Green",
            StrokeThickness = 1.0,
            Points = new List<System.Windows.Point> { new(0, 0), new(10, 10), new(20, 20) }
        };

        _textShape = new TextShape
        {
            ShapeId = Guid.NewGuid(),
            UserID = 1234.0,
            Color = "Black",
            StrokeThickness = 0.5,
            Text = "Hello World",
            FontSize = 12,
            X = 30,
            Y = 30
        };

        _shapes = new ObservableCollection<IShape> { _circleShape, _lineShape, _scribbleShape, _textShape };

        _snapShot = new SnapShot
        {
            _userID = "1234",
            _fileName = "testSnapshot",
            _shapes = _shapes
        };
    }

    [TestMethod]
    public void SerializeShape_ShouldReturnJsonString()
    {
        string serializedCircle = SerializationService.SerializeShape(_circleShape);
        string serializedLine = SerializationService.SerializeShape(_lineShape);

        Assert.IsNotNull(serializedCircle);
        Assert.IsTrue(serializedCircle.Contains("\"ShapeType\":\"Circle\""));
        Assert.IsNotNull(serializedLine);
        Assert.IsTrue(serializedLine.Contains("\"ShapeType\":\"Line\""));
    }

    [TestMethod]
    public void DeserializeShape_ShouldReturnCorrectShapeType()
    {
        string serializedCircle = SerializationService.SerializeShape(_circleShape);
        string serializedLine = SerializationService.SerializeShape(_lineShape);

        IShape deserializedCircle = SerializationService.DeserializeShape(serializedCircle);
        IShape deserializedLine = SerializationService.DeserializeShape(serializedLine);

        Assert.IsInstanceOfType(deserializedCircle, typeof(CircleShape));
        Assert.IsInstanceOfType(deserializedLine, typeof(LineShape));
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void DeserializeShape_ShouldThrowException_ForUnsupportedShape()
    {
        string unsupportedShapeJson = "{\"ShapeType\":\"UnknownShape\"}";
        SerializationService.DeserializeShape(unsupportedShapeJson);
    }

    [TestMethod]
    public void SerializeShapes_ShouldReturnJsonString()
    {
        string serializedShapes = SerializationService.SerializeShapes(_shapes);

        Assert.IsNotNull(serializedShapes);
        Assert.IsTrue(serializedShapes.Contains("\"ShapeType\":\"Circle\""));
        Assert.IsTrue(serializedShapes.Contains("\"ShapeType\":\"Line\""));
        Assert.IsTrue(serializedShapes.Contains("\"ShapeType\":\"Scribble\""));
        Assert.IsTrue(serializedShapes.Contains("\"ShapeType\":\"TextShape\""));
    }

    [TestMethod]
    public void DeserializeShapes_ShouldReturnObservableCollectionOfShapes()
    {
        string serializedShapes = SerializationService.SerializeShapes(_shapes);

        ObservableCollection<IShape> deserializedShapes = SerializationService.DeserializeShapes(serializedShapes);

        Assert.IsNotNull(deserializedShapes);
        Assert.AreEqual(4, deserializedShapes.Count);
        Assert.IsInstanceOfType(deserializedShapes[0], typeof(CircleShape));
        Assert.IsInstanceOfType(deserializedShapes[1], typeof(LineShape));
        Assert.IsInstanceOfType(deserializedShapes[2], typeof(ScribbleShape));
        Assert.IsInstanceOfType(deserializedShapes[3], typeof(TextShape));
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public void DeserializeShapes_ShouldThrowException_ForUnsupportedShapeInCollection()
    {
        string invalidShapeJson = "[{\"ShapeType\":\"UnknownShape\"}]";
        SerializationService.DeserializeShapes(invalidShapeJson);
    }

    [TestMethod]
    public void DeserializeShapes_ShouldHandleEmptyList()
    {
        string emptyListJson = "[]";
        ObservableCollection<IShape> deserializedShapes = SerializationService.DeserializeShapes(emptyListJson);

        Assert.IsNotNull(deserializedShapes);
        Assert.AreEqual(0, deserializedShapes.Count);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonReaderException))]
    public void DeserializeShape_ShouldThrowException_ForInvalidJson()
    {
        string invalidJson = "{InvalidJson}";
        SerializationService.DeserializeShape(invalidJson);
    }

    [TestMethod]
    [ExpectedException(typeof(JsonReaderException))]
    public void DeserializeShapes_ShouldThrowException_ForInvalidJson()
    {
        string invalidJson = "{InvalidJson}";
        SerializationService.DeserializeShapes(invalidJson);
    }

    [TestMethod]
    public void SerializeSnapShot_ShouldReturnJsonString()
    {
        string serializedSnapShot = SerializationService.SerializeSnapShot(_snapShot);

        Assert.IsNotNull(serializedSnapShot);
        Assert.IsTrue(serializedSnapShot.Contains("\"_fileName\":\"testSnapshot\""));
    }

    [TestMethod]
    public void DeserializeSnapShot_ShouldReturnCorrectObject()
    {
        string serializedSnapShot = SerializationService.SerializeSnapShot(_snapShot);
        SnapShot deserializedSnapShot = SerializationService.DeserializeSnapShot(serializedSnapShot);

        Assert.IsNotNull(deserializedSnapShot);
        Assert.AreEqual(_snapShot._fileName, deserializedSnapShot._fileName);
        Assert.AreEqual(_snapShot._userID, deserializedSnapShot._userID);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void DeserializeSnapShot_ShouldThrowException_ForNullInput()
    {
        SerializationService.DeserializeSnapShot(null);
    }

    [TestMethod]
    public void DeserializeSnapShot_ShouldReturnNull_ForEmptyString()
    {
        SnapShot snapShot = SerializationService.DeserializeSnapShot(string.Empty);
        Assert.IsNull(snapShot);
    }
}
