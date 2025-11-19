using WindowSill.API;

namespace WindowSill.ClipboardHistory.Utils;

internal sealed class FavoriteTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is bool isFavorite)
        {
            return isFavorite
                ? "/WindowSill.ClipboardHistory/Misc/RemoveFromFavorites".GetLocalizedString()
                : "/WindowSill.ClipboardHistory/Misc/AddToFavorites".GetLocalizedString();
        }
        return "/WindowSill.ClipboardHistory/Misc/AddToFavorites".GetLocalizedString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}
