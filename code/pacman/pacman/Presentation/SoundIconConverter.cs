using Microsoft.UI.Xaml.Data;

namespace pacman.Presentation;

public class SoundIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value is true ? "\uE767" : "\uE74F"; // E767 = Volume, E74F = Mute (Segoe MDL2)

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
