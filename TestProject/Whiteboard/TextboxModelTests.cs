using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Windows;
using WhiteboardGUI.Models;

namespace Whiteboard;

[TestClass]
public class TextboxModelTests
{
    [TestMethod]
    public void TextboxModel_Clone_ShouldCreateEqualShapeWithIsSelectedFalse()
    {
        // Arrange
        var originalShape = new TextboxModel
        {
            ShapeId = Guid.NewGuid(),
            UserID = 1.0,
            Color = "Red",
            StrokeThickness = 1.5,
            LastModifierID = 2.0,
            IsSelected = true, // Original shape is selected
            Text = "Sample Text",
            Width = 200,
            Height = 100,
            X = 50,
            Y = 75,
            FontSize = 14,
            ZIndex = 5
        };

        // Act
        var clonedShape = originalShape.Clone() as TextboxModel;

        // Assert
        Assert.IsNotNull(clonedShape, "Cloned shape should not be null.");
        Assert.AreEqual(originalShape.ShapeId, clonedShape.ShapeId, "ShapeId should be equal.");
        Assert.AreEqual(originalShape.UserID, clonedShape.UserID, "UserID should be equal.");
        Assert.AreEqual(originalShape.Color, clonedShape.Color, "Color should be equal.");
        Assert.AreEqual(originalShape.StrokeThickness, clonedShape.StrokeThickness, "StrokeThickness should be equal.");
        Assert.AreEqual(originalShape.LastModifierID, clonedShape.LastModifierID, "LastModifierID should be equal.");
        Assert.AreEqual(originalShape.Text, clonedShape.Text, "Text should be equal.");
        Assert.AreEqual(originalShape.Width, clonedShape.Width, "Width should be equal.");
        Assert.AreEqual(originalShape.Height, clonedShape.Height, "Height should be equal.");
        Assert.AreEqual(originalShape.X, clonedShape.X, "X should be equal.");
        Assert.AreEqual(originalShape.Y, clonedShape.Y, "Y should be equal.");
        Assert.AreEqual(originalShape.FontSize, clonedShape.FontSize, "FontSize should be equal.");
        Assert.AreEqual(originalShape.ZIndex, clonedShape.ZIndex, "ZIndex should be equal.");
        Assert.IsFalse(clonedShape.IsSelected, "Cloned shape's IsSelected should be false.");
    }

    [TestMethod]
    public void TextboxModel_Clone_ShouldNotAffectOriginalWhenClonedPropertiesAreModified()
    {
        // Arrange
        var originalShape = new TextboxModel
        {
            ShapeId = Guid.NewGuid(),
            UserID = 1.0,
            Color = "Blue",
            StrokeThickness = 2.0,
            LastModifierID = 2.0,
            IsSelected = true,
            Text = "Original Text",
            Width = 150,
            Height = 75,
            X = 25,
            Y = 35,
            FontSize = 12,
            ZIndex = 3
        };

        // Act
        var clonedShape = originalShape.Clone() as TextboxModel;
        clonedShape.Text = "Modified Text";
        clonedShape.Width = 300;
        clonedShape.Height = 150;
        clonedShape.X = 50;
        clonedShape.Y = 70;
        clonedShape.FontSize = 16;
        clonedShape.IsSelected = true; // Even if set to true, original should remain unaffected

        // Assert
        // Original shape should remain unchanged
        Assert.AreEqual("Original Text", originalShape.Text, "Original shape's Text should remain unchanged.");
        Assert.AreEqual(150, originalShape.Width, "Original shape's Width should remain unchanged.");
        Assert.AreEqual(75, originalShape.Height, "Original shape's Height should remain unchanged.");
        Assert.AreEqual(25, originalShape.X, "Original shape's X should remain unchanged.");
        Assert.AreEqual(35, originalShape.Y, "Original shape's Y should remain unchanged.");
        Assert.AreEqual(12, originalShape.FontSize, "Original shape's FontSize should remain unchanged.");
        Assert.IsTrue(originalShape.IsSelected, "Original shape's IsSelected should remain true.");

        // Cloned shape should reflect the changes
        Assert.AreEqual("Modified Text", clonedShape.Text, "Cloned shape's Text should be updated.");
        Assert.AreEqual(300, clonedShape.Width, "Cloned shape's Width should be updated.");
        Assert.AreEqual(150, clonedShape.Height, "Cloned shape's Height should be updated.");
        Assert.AreEqual(50, clonedShape.X, "Cloned shape's X should be updated.");
        Assert.AreEqual(70, clonedShape.Y, "Cloned shape's Y should be updated.");
        Assert.AreEqual(16, clonedShape.FontSize, "Cloned shape's FontSize should be updated.");
        Assert.IsTrue(clonedShape.IsSelected, "Cloned shape's IsSelected should be true.");
    }

    [TestMethod]
    public void TextboxModel_GetBounds_ShouldReturnCorrectRectangle()
    {
        // Arrange
        var textShape = new TextboxModel
        {
            X = 100,
            Y = 200,
            Width = 300,
            Height = 150,
            Text = "Test",
            FontSize = 20
        };

        // Act
        Rect bounds = textShape.GetBounds();

        // Assert
        Assert.AreEqual(textShape.X, bounds.Left, "Bounds.Left should be equal to X.");
        Assert.AreEqual(textShape.Y, bounds.Top, "Bounds.Top should be equal to Y.");
        Assert.AreEqual(textShape.Width, bounds.Width, "Bounds.Width should be equal to Width.");
        Assert.AreEqual(textShape.Height, bounds.Height, "Bounds.Height should be equal to Height.");
    }



    [TestMethod]
    public void TextboxModel_GetBounds_ShouldReflectUpdatedDimensions()
    {
        // Arrange
        var textShape = new TextboxModel
        {
            X = 50,
            Y = 75,
            Width = 100,
            Height = 50,
            Text = "Update Test",
            FontSize = 14
        };

        // Act
        // Update dimensions
        textShape.Width = 200;
        textShape.Height = 100;

        Rect bounds = textShape.GetBounds();

        // Assert
        Assert.AreEqual(50, bounds.Left, "Bounds.Left should be equal to X.");
        Assert.AreEqual(75, bounds.Top, "Bounds.Top should be equal to Y.");
        Assert.AreEqual(200, bounds.Width, "Bounds.Width should be updated correctly.");
        Assert.AreEqual(100, bounds.Height, "Bounds.Height should be updated correctly.");
    }

    [TestMethod]
    public void TextboxModel_Clone_ShouldBeIndependentOfOriginal()
    {
        // Arrange
        var originalShape = new TextboxModel
        {
            ShapeId = Guid.NewGuid(),
            UserID = 1.0,
            Color = "Black",
            StrokeThickness = 1.0,
            LastModifierID = 2.0,
            IsSelected = true,
            Text = "Original Text",
            Width = 200,
            Height = 100,
            X = 100,
            Y = 150,
            FontSize = 16,
            ZIndex = 4
        };

        // Act
        var clonedShape = originalShape.Clone() as TextboxModel;
        clonedShape.Text = "Cloned Text";
        clonedShape.Width = 300;
        clonedShape.Height = 150;
        clonedShape.X = 150;
        clonedShape.Y = 200;
        clonedShape.FontSize = 18;
        clonedShape.IsSelected = true; // Should not affect original

        // Assert
        // Original shape should remain unchanged
        Assert.AreEqual("Original Text", originalShape.Text, "Original shape's Text should remain unchanged.");
        Assert.AreEqual(200, originalShape.Width, "Original shape's Width should remain unchanged.");
        Assert.AreEqual(100, originalShape.Height, "Original shape's Height should remain unchanged.");
        Assert.AreEqual(100, originalShape.X, "Original shape's X should remain unchanged.");
        Assert.AreEqual(150, originalShape.Y, "Original shape's Y should remain unchanged.");
        Assert.AreEqual(16, originalShape.FontSize, "Original shape's FontSize should remain unchanged.");
        Assert.IsTrue(originalShape.IsSelected, "Original shape's IsSelected should remain true.");

        // Cloned shape should reflect the changes
        Assert.AreEqual("Cloned Text", clonedShape.Text, "Cloned shape's Text should be updated.");
        Assert.AreEqual(300, clonedShape.Width, "Cloned shape's Width should be updated.");
        Assert.AreEqual(150, clonedShape.Height, "Cloned shape's Height should be updated.");
        Assert.AreEqual(150, clonedShape.X, "Cloned shape's X should be updated.");
        Assert.AreEqual(200, clonedShape.Y, "Cloned shape's Y should be updated.");
        Assert.AreEqual(18, clonedShape.FontSize, "Cloned shape's FontSize should be updated.");
        Assert.IsTrue(clonedShape.IsSelected, "Cloned shape's IsSelected should be true.");
    }
}
