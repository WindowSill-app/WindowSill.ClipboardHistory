using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using WindowSill.API;
using WindowSill.ClipboardHistory.Utils;

namespace WindowSill.ClipboardHistory.UI;

internal abstract partial class ClipboardHistoryItemViewModelBase
    : ObservableObject,
    IEquatable<ClipboardHistoryItemViewModelBase>,
    IEquatable<ClipboardHistoryItem>
{
    private readonly IProcessInteractionService _processInteractionService;
    private readonly ClipboardHistoryItem _item;
    private readonly FavoritesService _favoritesService;
    private string? _contentHash;

    [ObservableProperty]
    private bool _isFavorite;

    protected ClipboardHistoryItemViewModelBase(
        IProcessInteractionService processInteractionService,
        ClipboardHistoryItem item,
        FavoritesService favoritesService)
        : base()
    {
        Guard.IsNotNull(processInteractionService);
        Guard.IsNotNull(item);
        Guard.IsNotNull(favoritesService);
        _processInteractionService = processInteractionService;
        _item = item;
        _favoritesService = favoritesService;
        Data = item.Content;
        
        InitializeFavoriteStatusAsync().Forget();
    }

    public DataPackageView Data { get; }

    public string? ContentHash => _contentHash;

    private async Task InitializeFavoriteStatusAsync()
    {
        _contentHash = await FavoritesService.ComputeContentHashAsync(_item);
        if (_contentHash is not null)
        {
            IsFavorite = _favoritesService.IsFavorite(_contentHash);
        }
    }

    public bool Equals(ClipboardHistoryItemViewModelBase? other)
    {
        return other is not null && Equals(other._item);
    }

    public bool Equals(ClipboardHistoryItem? other)
    {
        return other is not null && string.Equals(_item.Id, other.Id, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ClipboardHistoryItemViewModelBase)
            || Equals(obj as ClipboardHistoryItem);
    }

    public override int GetHashCode()
    {
        return _item.Id.GetHashCode();
    }

    [RelayCommand]
    private async Task PasteAsync()
    {
        await ThreadHelper.RunOnUIThreadAsync(async () =>
        {
            Clipboard.SetHistoryItemAsContent(_item);

            await _processInteractionService.SimulateKeysOnLastActiveWindow(
                VirtualKey.LeftControl,
                VirtualKey.V);
        });
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        await ThreadHelper.RunOnUIThreadAsync(() =>
        {
            Clipboard.DeleteItemFromHistory(_item);
        });
    }

    [RelayCommand]
    private async Task ClearAsync()
    {
        await ThreadHelper.RunOnUIThreadAsync(() =>
        {
            Clipboard.ClearHistory();
        });
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        if (_contentHash is not null)
        {
            _favoritesService.ToggleFavorite(_contentHash);
            IsFavorite = _favoritesService.IsFavorite(_contentHash);
        }
    }
}
