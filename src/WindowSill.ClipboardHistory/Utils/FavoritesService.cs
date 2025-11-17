using System.Security.Cryptography;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using WindowSill.API;

namespace WindowSill.ClipboardHistory.Utils;

internal sealed class FavoritesService
{
    private readonly ISettingsProvider _settingsProvider;
    private readonly HashSet<string> _favoriteHashes = new();
    private static readonly SettingDefinition<string> FavoriteItemsSetting = new(string.Empty, typeof(FavoritesService).Assembly);

    public FavoritesService(ISettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;
        LoadFavorites();
    }

    /// <summary>
    /// Computes a content hash for a clipboard item to uniquely identify it by content
    /// </summary>
    public static async Task<string?> ComputeContentHashAsync(ClipboardHistoryItem item)
    {
        try
        {
            var content = item.Content;
            var sb = new StringBuilder();

            // Include all available formats to make the hash unique
            foreach (var format in content.AvailableFormats.OrderBy(f => f))
            {
                sb.Append(format);
                sb.Append('|');

                // Try to get the actual content for common formats
                try
                {
                    if (format == StandardDataFormats.Text)
                    {
                        string text = await content.GetTextAsync();
                        sb.Append(text);
                    }
                    else if (format == StandardDataFormats.Html)
                    {
                        string html = await content.GetHtmlFormatAsync();
                        sb.Append(html);
                    }
                    else if (format == StandardDataFormats.Rtf)
                    {
                        string rtf = await content.GetRtfAsync();
                        sb.Append(rtf);
                    }
                    else if (format == StandardDataFormats.WebLink)
                    {
                        Uri uri = await content.GetWebLinkAsync();
                        sb.Append(uri.ToString());
                    }
                    else if (format == StandardDataFormats.ApplicationLink)
                    {
                        Uri uri = await content.GetApplicationLinkAsync();
                        sb.Append(uri.ToString());
                    }
                }
                catch
                {
                    // If we can't get the content for a format, skip it
                }

                sb.Append('|');
            }

            // Also include timestamp to ensure uniqueness (items copied at different times are different)
            sb.Append(item.Timestamp.Ticks);

            string contentString = sb.ToString();
            if (string.IsNullOrEmpty(contentString))
            {
                return null;
            }

            // Compute SHA256 hash
            byte[] bytes = Encoding.UTF8.GetBytes(contentString);
            byte[] hash = SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
        catch
        {
            return null;
        }
    }

    public bool IsFavorite(string contentHash)
    {
        return !string.IsNullOrEmpty(contentHash) && _favoriteHashes.Contains(contentHash);
    }

    public void ToggleFavorite(string contentHash)
    {
        if (string.IsNullOrEmpty(contentHash))
        {
            return;
        }

        if (_favoriteHashes.Contains(contentHash))
        {
            _favoriteHashes.Remove(contentHash);
        }
        else
        {
            _favoriteHashes.Add(contentHash);
        }

        SaveFavorites();
    }

    private void LoadFavorites()
    {
        string savedFavorites = _settingsProvider.GetSetting(FavoriteItemsSetting);
        if (!string.IsNullOrEmpty(savedFavorites))
        {
            var hashes = savedFavorites.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var hash in hashes)
            {
                _favoriteHashes.Add(hash);
            }
        }
    }

    private void SaveFavorites()
    {
        string favoritesString = string.Join(';', _favoriteHashes);
        _settingsProvider.SetSetting(FavoriteItemsSetting, favoritesString);
    }
}
