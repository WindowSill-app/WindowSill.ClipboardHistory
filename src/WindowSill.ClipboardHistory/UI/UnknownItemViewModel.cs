using Windows.ApplicationModel.DataTransfer;
using WindowSill.API;
using WindowSill.ClipboardHistory.Utils;

namespace WindowSill.ClipboardHistory.UI;

internal sealed class UnknownItemViewModel : ClipboardHistoryItemViewModelBase
{
    private readonly SillListViewButtonItem _view;

    private UnknownItemViewModel(IProcessInteractionService processInteractionService, ClipboardHistoryItem item, FavoritesService favoritesService)
        : base(processInteractionService, item, favoritesService)
    {
        _view = new SillListViewButtonItem(base.PasteCommand);
        _view.DataContext = this;

        _view.Content = "/WindowSill.ClipboardHistory/Misc/UnsupportedFormat".GetLocalizedString();
        _view.PreviewFlyoutContent = string.Join(", ", item.Content.AvailableFormats);
    }

    internal static (ClipboardHistoryItemViewModelBase, SillListViewItem) CreateView(IProcessInteractionService processInteractionService, ClipboardHistoryItem item, FavoritesService favoritesService)
    {
        var viewModel = new UnknownItemViewModel(processInteractionService, item, favoritesService);
        return (viewModel, viewModel._view);
    }
}
