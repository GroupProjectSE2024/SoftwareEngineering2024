/**************************************************************************************************
 * Filename    : BooleanToTextConverter.cs
 *
 * Author      : Vishnu Nair
 *
 * Product     : WhiteBoard
 * 
 * Project     : Converters
 *
 * Description : Converts boolean values to specified text representations based on provided parameters.
 *               This converter is useful for binding boolean properties to UI elements that require
 *               textual representations, such as button labels or status indicators.
 *************************************************************************************************/

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WhiteboardGUI.Converters;

/// <summary>
/// Converts boolean values to specified text representations based on provided parameters.
/// </summary>
/// <remarks>
/// The converter expects the <paramref name="parameter"/> to be a string in the format 
/// "FalseText|TrueText". It splits this string on the '|' character and returns 
/// <paramref name="parameter"/>[1] if <paramref name="value"/> is <c>true</c>, otherwise returns 
/// <paramref name="parameter"/>[0].
/// </remarks>
public class BooleanToTextConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value to a corresponding text based on the provided parameter.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <param name="targetType">The type of the binding target property (ignored).</param>
    /// <param name="parameter">
    /// A string containing two text values separated by a pipe '|'.
    /// The first value is returned when <paramref name="value"/> is <c>false</c>,
    /// and the second value is returned when <paramref name="value"/> is <c>true</c>.
    /// </param>
    /// <param name="culture">The culture to use in the converter (ignored).</param>
    /// <returns>
    /// Returns the second text value if <paramref name="value"/> is <c>true</c>,
    /// otherwise returns the first text value. If conversion fails, 
    /// returns <see cref="DependencyProperty.UnsetValue"/>.
    /// </returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter == null)
        {
            return DependencyProperty.UnsetValue;
        }
        string[] parameters = parameter.ToString().Split('|');
        if (parameters.Length != 2)
        {
            return DependencyProperty.UnsetValue;
        }

        if (value is bool boolValue)
        {
            return boolValue ? parameters[1] : parameters[0];
        }

        return DependencyProperty.UnsetValue;
    }

    /// <summary>
    /// Not implemented. Throws a <see cref="NotImplementedException"/>.
    /// </summary>
    /// <param name="value">The value produced by the binding target (ignored).</param>
    /// <param name="targetType">The type to convert to (ignored).</param>
    /// <param name="parameter">An optional parameter (ignored).</param>
    /// <param name="culture">The culture to use in the converter (ignored).</param>
    /// <returns>Nothing. Always throws <see cref="NotImplementedException"/>.</returns>
    /// <exception cref="NotImplementedException">Always thrown as this method is not implemented.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
