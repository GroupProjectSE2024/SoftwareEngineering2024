using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhiteboardGUI.Converters;

namespace Whiteboard;

[TestClass]
public class Test_DarkModeColorConverter
{
    private DarkModeColorConverter _converter;

    [TestInitialize]
    public void Setup()
    {
        _converter = new DarkModeColorConverter();
    }

    [TestMethod]
    public void Convert_ReturnsWhiteBrush_WhenDarkModeAndBlackColor()
    {
        // Arrange
        object[] values = { "Black", true };
        CultureInfo culture = CultureInfo.InvariantCulture;

        // Act
        object result = _converter.Convert(values, typeof(SolidColorBrush), null, culture);

        // Assert
        Assert.IsInstanceOfType(result, typeof(SolidColorBrush));
        Assert.AreEqual(Colors.White, ((SolidColorBrush)result).Color);
    }

    [TestMethod]
    public void Convert_ReturnsOriginalBrush_WhenLightMode()
    {
        // Arrange
        object[] values = { "Red", false };
        CultureInfo culture = CultureInfo.InvariantCulture;

        // Act
        object result = _converter.Convert(values, typeof(SolidColorBrush), null, culture);

        // Assert
        Assert.IsInstanceOfType(result, typeof(SolidColorBrush));
        Assert.AreEqual(Colors.Red, ((SolidColorBrush)result).Color);
    }

    [TestMethod]
    public void Convert_ReturnsOriginalBrush_WhenDarkModeAndNonBlackColor()
    {
        // Arrange
        object[] values = { "Blue", true };
        CultureInfo culture = CultureInfo.InvariantCulture;

        // Act
        object result = _converter.Convert(values, typeof(SolidColorBrush), null, culture);

        // Assert
        Assert.IsInstanceOfType(result, typeof(SolidColorBrush));
        Assert.AreEqual(Colors.Blue, ((SolidColorBrush)result).Color);
    }

    [TestMethod]
    public void Convert_ReturnsUnsetValue_WhenColorStringIsInvalid()
    {
        // Arrange
        object[] values = { "InvalidColor", true };
        CultureInfo culture = CultureInfo.InvariantCulture;

        // Act
        object result = _converter.Convert(values, typeof(SolidColorBrush), null, culture);

        // Assert
        Assert.AreEqual(DependencyProperty.UnsetValue, result);
    }

    [TestMethod]
    [ExpectedException(typeof(NullReferenceException))]
    public void Convert_ThrowsArgumentNullException_WhenValuesAreNull()
    {
        // Arrange
        object[] values = null;
        CultureInfo culture = CultureInfo.InvariantCulture;

        // Act
        _converter.Convert(values, typeof(SolidColorBrush), null, culture);
    }

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Arrange
        object value = new SolidColorBrush(Colors.White);
        CultureInfo culture = CultureInfo.InvariantCulture;

        // Act
        _converter.ConvertBack(value, new Type[] { typeof(string), typeof(bool) }, null, culture);
    }
}
