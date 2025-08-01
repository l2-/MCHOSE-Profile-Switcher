using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace UI.Components;

public class ProcessToImageConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        // here you can return DrawingImage based on value that represents name field of your structure
        // just for example the piece of your code:
        if (value is string str && !string.IsNullOrEmpty(value as string))
        {
            if (!File.Exists(str)) return null;
            var icon = Icon.ExtractAssociatedIcon(str);
            if (icon is null) return null;
            return Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
        else
        {
            // if value is null or not of string type
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
