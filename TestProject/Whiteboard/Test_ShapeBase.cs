using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using WhiteboardGUI.Models;

namespace Whiteboard;

[TestClass]
public class Test_ShapeBase
{
    // A derived class for testing purposes
    public class TestShape : ShapeBase
    {
        public override string ShapeType => "TestShape";
        public override Rect GetBounds()
        {
            return new Rect(0, 0, 100, 100);
        }

        public override IShape Clone()
        {
            return new TestShape {
                ShapeId = this.ShapeId,
                Color = this.Color,
                StrokeThickness = this.StrokeThickness,
                UserID = this.UserID,
                LastModifierID = this.LastModifierID,
                ZIndex = this.ZIndex,
                IsSelected = this.IsSelected
            };
        }
    }

    [TestMethod]
    public void ShapeBase_Constructor_ShouldInitializeDefaultValues()
    {
        // Arrange & Act
        var shape = new TestShape();

        // Assert
        Assert.AreEqual(Guid.Empty, shape.ShapeId);
        Assert.AreEqual("#000000", shape.Color);
        Assert.AreEqual(0, shape.StrokeThickness);
        Assert.AreEqual(0, shape.UserID);
        Assert.AreEqual(0, shape.LastModifierID);
        Assert.AreEqual(0, shape.ZIndex);
        Assert.AreEqual(false, shape.IsSelected);
    }

    [TestMethod]
    public void ShapeBase_PropertyChanged_ShouldNotifyOnValueChange()
    {
        // Arrange
        var shape = new TestShape();
        bool eventTriggered = false;
        shape.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(shape.Color))
            {
                eventTriggered = true;
            }
        };

        // Act
        shape.Color = "#FF0000";

        // Assert
        Assert.IsTrue(eventTriggered);
    }

    [TestMethod]
    public void ShapeBase_SetZIndex_ShouldTriggerPropertyChanged()
    {
        // Arrange
        var shape = new TestShape();
        bool eventTriggered = false;
        shape.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(shape.ZIndex))
            {
                eventTriggered = true;
            }
        };

        // Act
        shape.ZIndex = 5;

        // Assert
        Assert.IsTrue(eventTriggered);
    }

    [TestMethod]
    public void ShapeBase_SetIsSelected_ShouldTriggerPropertyChanged()
    {
        // Arrange
        var shape = new TestShape();
        bool eventTriggered = false;
        shape.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(shape.IsSelected))
            {
                eventTriggered = true;
            }
        };

        // Act
        shape.IsSelected = true;

        // Assert
        Assert.IsTrue(eventTriggered);
    }

    [TestMethod]
    public void ShapeBase_Clone_ShouldReturnEqualShape()
    {
        // Arrange
        var originalShape = new TestShape
        {
            ShapeId = Guid.NewGuid(),
            Color = "#00FF00",
            StrokeThickness = 2,
            UserID = 1,
            LastModifierID = 2,
            ZIndex = 10,
            IsSelected = true
        };

        // Act
        IShape clonedShape = originalShape.Clone();

        // Assert
        Assert.AreNotSame(originalShape, clonedShape);
        Assert.AreEqual(originalShape.ShapeId, clonedShape.ShapeId);
        Assert.AreEqual(originalShape.Color, clonedShape.Color);
        Assert.AreEqual(originalShape.StrokeThickness, clonedShape.StrokeThickness);
        Assert.AreEqual(originalShape.UserID, clonedShape.UserID);
        Assert.AreEqual(originalShape.LastModifierID, clonedShape.LastModifierID);
        Assert.AreEqual(originalShape.ZIndex, clonedShape.ZIndex);
        Assert.AreEqual(originalShape.IsSelected, clonedShape.IsSelected);
    }

    [TestMethod]
    public void ShapeBase_SetShapeId_ShouldTriggerPropertyChanged()
    {
        // Arrange
        var shape = new TestShape();
        bool eventTriggered = false;
        shape.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(shape.ShapeId))
            {
                eventTriggered = true;
            }
        };

        // Act
        shape.ShapeId = Guid.NewGuid();

        // Assert
        Assert.IsTrue(eventTriggered);
    }
}
