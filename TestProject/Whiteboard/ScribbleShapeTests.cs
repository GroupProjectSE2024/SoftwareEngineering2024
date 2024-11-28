using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Windows;
using WhiteboardGUI.Models;

namespace Whiteboard;

[TestClass]
public class ScribbleShapeTests
{
    [TestMethod]
    public void ScribbleShape_Clone_ShouldCreateEqualShapeWithIsSelectedFalse()
    {
        // Arrange
        var originalShape = new ScribbleShape
        {
            ShapeId = Guid.NewGuid(),
            UserID = 1.0,
            Color = "Blue",
            StrokeThickness = 2.0,
            LastModifierID = 1.0,
            IsSelected = true, // Original shape is selected
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

        // Act
        var clonedShape = originalShape.Clone() as ScribbleShape;

        // Assert
        Assert.IsNotNull(clonedShape, "Cloned shape should not be null.");
        Assert.AreEqual(originalShape.ShapeId, clonedShape.ShapeId, "ShapeId should be equal.");
        Assert.AreEqual(originalShape.UserID, clonedShape.UserID, "UserID should be equal.");
        Assert.AreEqual(originalShape.Color, clonedShape.Color, "Color should be equal.");
        Assert.AreEqual(originalShape.StrokeThickness, clonedShape.StrokeThickness, "StrokeThickness should be equal.");
        Assert.AreEqual(originalShape.LastModifierID, clonedShape.LastModifierID, "LastModifierID should be equal.");
        Assert.AreEqual(originalShape.ZIndex, clonedShape.ZIndex, "ZIndex should be equal.");
        Assert.IsFalse(clonedShape.IsSelected, "Cloned shape's IsSelected should be false.");

        // Verify that Points list is a deep copy
        Assert.AreNotSame(originalShape.Points, clonedShape.Points, "Points list should be a different instance.");
        CollectionAssert.AreEqual(originalShape.Points, clonedShape.Points, "Points list should contain the same points.");
    }

    [TestMethod]
    public void ScribbleShape_Clone_ShouldNotAffectOriginalWhenClonedPointsAreModified()
    {
        // Arrange
        var originalShape = new ScribbleShape
        {
            ShapeId = Guid.NewGuid(),
            UserID = 1.0,
            Color = "Blue",
            StrokeThickness = 2.0,
            LastModifierID = 1.0,
            IsSelected = true,
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

        // Act
        var clonedShape = originalShape.Clone() as ScribbleShape;
        clonedShape.AddPoint(new Point(60, 25));

        // Assert
        Assert.AreEqual(5, originalShape.Points.Count, "Original shape's Points should remain unchanged.");
        Assert.AreEqual(6, clonedShape.Points.Count, "Cloned shape's Points should include the new point.");
    }
}
