/**************************************************************************************************
 * Filename    : DarkModeColorConverter.cs
 *
 * Author      : Rachit Jain and Kshitij Ghodake
 *
 * Product     : WhiteBoard
 * 
 * Project     : Dark Mode Support
 *
 * Description : Implements a multi-value converter that adjusts color values based on dark mode
 *               settings. This ensures that UI elements maintain appropriate contrast and visibility
 *               when dark mode is enabled, enhancing the user experience.
 *************************************************************************************************/


using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WhiteboardGUI.Converters;

/// <summary>
/// A multi-value converter that adjusts color values based on dark mode settings.
/// </summary>
public class DarkModeColorConverter : IMultiValueConverter
{
    /// <summary>
    /// Converts an array of values (color string and dark mode flag) into a SolidColorBrush.
    /// </summary>
    /// <param name="values">
    /// An array containing:
    /// - <c>values[0]</c>: The color represented as a string.
    /// - <c>values[1]</c>: A boolean indicating whether dark mode is enabled.
    /// </param>
    /// <param name="targetType">The target type of the binding (ignored).</param>
    /// <param name="parameter">An optional parameter (ignored).</param>
    /// <param name="culture">The culture to use during conversion (ignored).</param>
    /// <returns>
    /// A <see cref="SolidColorBrush"/> with the adjusted color for dark mode,
    /// or <see cref="DependencyProperty.UnsetValue"/> if the conversion fails.
    /// </returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        string? colorString = values[0] as string;
        bool isDarkMode = (bool)values[1];
        try
        {
            // Attempt to convert the color string to a Color object
            if (ColorConverter.ConvertFromString(colorString) is Color originalColor)
            {
                // Adjust color for dark mode if enabled
                if (isDarkMode)
                {
                    if (originalColor == Colors.Black)
                    {
                        return new SolidColorBrush(Colors.White);
                    }
                }
                // Return the original color as a SolidColorBrush
                return new SolidColorBrush(originalColor);
            }

            // Return unset value if conversion fails
            return DependencyProperty.UnsetValue;
        }
        catch
        {
            // Log an error if color conversion fails
            Debug.WriteLine("InvalidColor");
            return DependencyProperty.UnsetValue;
        }
    }

    /// <summary>
    /// Not implemented. Converts a value back to the original source values.
    /// </summary>
    /// <param name="value">The value produced by the binding target (ignored).</param>
    /// <param name="targetTypes">The array of target types (ignored).</param>
    /// <param name="parameter">An optional parameter (ignored).</param>
    /// <param name="culture">The culture to use during conversion (ignored).</param>
    /// <returns>Throws a <see cref="NotImplementedException"/>.</returns>
    /// <exception cref="NotImplementedException">Always thrown as this method is not implemented.</exception>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
