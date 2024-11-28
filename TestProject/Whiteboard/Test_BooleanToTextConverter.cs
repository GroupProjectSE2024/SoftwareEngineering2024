using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhiteboardGUI.Converters;

namespace Whiteboard;

[TestClass]
public class Test_BooleanToTextConverter
{
    private BooleanToTextConverter _converter;

    [TestInitialize]
    public void Setup()
    {
        _converter = new BooleanToTextConverter();
    }

    [TestMethod]
    public void Convert_ReturnsTrueText_WhenValueIsTrue()
    {
        // Arrange
        bool value = true;
        string parameter = "FalseText|TrueText";
        CultureInfo culture = CultureInfo.InvariantCulture;

        // Act
        var result = _converter.Convert(value, typeof(string), parameter, culture);

        // Assert
        Assert.AreEqual("TrueText", result);
    }

    [TestMethod]
    public void Convert_ReturnsFalseText_WhenValueIsFalse()
    {
        // Arrange
        bool value = false;
        string parameter = "FalseText|TrueText";
        CultureInfo culture = CultureInfo.InvariantCulture;

        // Act
        var result = _converter.Convert(value, typeof(string), parameter, culture);

        // Assert
        Assert.AreEqual("FalseText", result);
    }

    //[TestMethod]
    //[ExpectedException(typeof(NullReferenceException))]
    //public void Convert_ThrowsArgumentNullException_WhenParameterIsNull()
    //{
    //    // Arrange
    //    bool value = true;
    //    string parameter = null;
    //    CultureInfo culture = CultureInfo.InvariantCulture;

    //    // Act
    //    _converter.Convert(value, typeof(string), parameter, culture);
    //}

    [TestMethod]
    [ExpectedException(typeof(NotImplementedException))]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Arrange
        string value = "TrueText";
        string parameter = "FalseText|TrueText";
        CultureInfo culture = CultureInfo.InvariantCulture;

        // Act
        _converter.ConvertBack(value, typeof(bool), parameter, culture);
    }
}
