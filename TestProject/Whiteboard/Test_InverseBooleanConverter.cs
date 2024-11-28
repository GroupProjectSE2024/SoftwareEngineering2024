using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhiteboardGUI.Converters;

namespace Whiteboard;

[TestClass]
public class Test_InverseBooleanConverter
{
    private InverseBooleanConverter _converter;

    [TestInitialize]
    public void Setup()
    {
        _converter = new InverseBooleanConverter();
    }

    [TestMethod]
    public void Convert_ReturnsFalse_WhenValueIsTrue()
    {
        // Arrange
        bool value = true;
        CultureInfo culture = CultureInfo.InvariantCulture;

        // Act
        object result = _converter.Convert(value, typeof(bool), null, culture);

        // Assert
        Assert.IsInstanceOfType(result, typeof(bool));
        Assert.AreEqual(false, result);
    }

    [TestMethod]
    public void Convert_ReturnsTrue_WhenValueIsFalse()
    {
        // Arrange
        bool value = false;
        CultureInfo culture = CultureInfo.InvariantCulture;

        // Act
        object result = _converter.Convert(value, typeof(bool), null, culture);

        // Assert
        Assert.IsInstanceOfType(result, typeof(bool));
        Assert.AreEqual(true, result);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidCastException))]
    public void Convert_ThrowsInvalidCastException_WhenValueIsNotBoolean()
    {
        // Arrange
        string invalidValue = "NotABoolean";
        CultureInfo culture = CultureInfo.InvariantCulture;

        // Act
        _converter.Convert(invalidValue, typeof(bool), null, culture);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Arrange
        bool value = false;
        CultureInfo culture = CultureInfo.InvariantCulture;

        // Act
        _converter.ConvertBack(value, typeof(bool), null, culture);
    }
}
