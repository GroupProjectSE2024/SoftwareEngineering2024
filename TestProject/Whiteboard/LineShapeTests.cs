using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhiteboardGUI.Models;

namespace Whiteboard;

[TestClass]
public class LineShapeTests
{
    [TestMethod]
    public void LineShape_Clone_ShouldCreateEqualShapeWithIsSelectedFalse()
    {
        // Arrange
        var originalShape = new LineShape
        {
            ShapeId = Guid.NewGuid(),
            UserID = 1.0,
            Color = "Red",
            StrokeThickness = 2.5,
            LastModifierID = 1.0,
            IsSelected = true, // Original shape is selected
            StartX = 100,
            StartY = 150,
            EndX = 200,
            EndY = 250,
            ZIndex = 5
        };

        // Act
        var clonedShape = originalShape.Clone() as LineShape;

        // Assert
        Assert.IsNotNull(clonedShape, "Cloned shape should not be null.");
        Assert.AreEqual(originalShape.ShapeId, clonedShape.ShapeId, "ShapeId should be equal.");
        Assert.AreEqual(originalShape.UserID, clonedShape.UserID, "UserID should be equal.");
        Assert.AreEqual(originalShape.Color, clonedShape.Color, "Color should be equal.");
        Assert.AreEqual(originalShape.StrokeThickness, clonedShape.StrokeThickness, "StrokeThickness should be equal.");
        Assert.AreEqual(originalShape.LastModifierID, clonedShape.LastModifierID, "LastModifierID should be equal.");
        Assert.AreEqual(originalShape.StartX, clonedShape.StartX, "StartX should be equal.");
        Assert.AreEqual(originalShape.StartY, clonedShape.StartY, "StartY should be equal.");
        Assert.AreEqual(originalShape.EndX, clonedShape.EndX, "EndX should be equal.");
        Assert.AreEqual(originalShape.EndY, clonedShape.EndY, "EndY should be equal.");
        Assert.AreEqual(originalShape.ZIndex, clonedShape.ZIndex, "ZIndex should be equal.");
        Assert.IsFalse(clonedShape.IsSelected, "Cloned shape's IsSelected should be false.");
    }
}
