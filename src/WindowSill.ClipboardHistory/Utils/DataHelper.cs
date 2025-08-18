using System.Text.RegularExpressions;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using WindowSill.API;

namespace WindowSill.ClipboardHistory.Utils;

internal static partial class DataHelper
{
    internal static Uri GetFaviconGoogleUri(string url, int size)
    {
        string faviconUrl = string.Format("https://www.google.com/s2/favicons?domain={0}&sz={1}", Uri.EscapeDataString(url), size);
        return new Uri(faviconUrl);
    }

    internal static async Task<BitmapImage?> GetBitmapAsync(DataPackageView dataPackageView)
    {
        try
        {
            RandomAccessStreamReference randomAccessStreamReference = await dataPackageView.GetBitmapAsync();
            using IRandomAccessStreamWithContentType randomAccessStreamWithContentType = await randomAccessStreamReference.OpenReadAsync();
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(randomAccessStreamWithContentType);
            bitmap.DecodePixelType = DecodePixelType.Physical;
            bitmap.DecodePixelWidth = bitmap.PixelWidth;
            return bitmap;
        }
        catch (Exception ex)
        {
            typeof(DataHelper).Log().LogError(ex, "Error while retrieving bitmap from data package view.");
            return null;
        }
    }

    internal static (SolidColorBrush background, SolidColorBrush foreground) GetBackgroundAndForegroundBrushes(string colorString)
    {
        if (!colorString.StartsWith('#'))
        {
            colorString = "#" + colorString;
        }

        var color = colorString.ToColor();
        var background = new SolidColorBrush(color);

        // Check the brightness of the color to determine whether the text must be black or white.
        if ((int)Math.Sqrt(color.R * color.R * 0.241 + color.G * color.G * 0.691 + color.B * color.B * 0.068) > 130)
        {
            return (background, new SolidColorBrush(Colors.Black));
        }
        else
        {
            return (background, new SolidColorBrush(Colors.White));
        }
    }

    internal static async Task<DetectedClipboardDataType> GetDetectedClipboardDataTypeAsync(ClipboardHistoryItem item)
    {
        if (item.Content.AvailableFormats.Contains(StandardDataFormats.StorageItems))
        {
            return DetectedClipboardDataType.File;
        }
        else if (item.Content.AvailableFormats.Contains(StandardDataFormats.Text))
        {
            string text = await item.Content.GetTextAsync();
            if (IsHexColor(text))
            {
                return DetectedClipboardDataType.Color;
            }
            else if (IsUri(text))
            {
                return DetectedClipboardDataType.Uri;
            }

            return DetectedClipboardDataType.Text;
        }
        else if (item.Content.AvailableFormats.Contains(StandardDataFormats.Html))
        {
            return DetectedClipboardDataType.Html;
        }
        else if (item.Content.AvailableFormats.Contains(StandardDataFormats.Bitmap))
        {
            return DetectedClipboardDataType.Image;
        }

        return DetectedClipboardDataType.Unknown;
    }

    private static bool IsHexColor(string text)
    {
        return HexColorRegex().IsMatch(text);
    }

    private static bool IsUri(string text)
    {
        return Uri.TryCreate(text, UriKind.Absolute, out Uri? uriResult)
            && uriResult is not null
            && (uriResult.Scheme == Uri.UriSchemeHttp
            || uriResult.Scheme == Uri.UriSchemeHttps
            || uriResult.Scheme == Uri.UriSchemeFtp
            || uriResult.Scheme == Uri.UriSchemeMailto);
    }

    [GeneratedRegex(@"^(#)?([A-Fa-f0-9]{8}|[A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", RegexOptions.Compiled, 250)]
    private static partial Regex HexColorRegex();
}
