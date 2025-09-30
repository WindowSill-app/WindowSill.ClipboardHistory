using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Windows.ApplicationModel.DataTransfer;
using WindowSill.API;

namespace WindowSill.ClipboardHistory.UI;

internal sealed class HtmlItemViewModel : ClipboardHistoryItemViewModelBase
{
    private readonly ILogger _logger;
    private readonly SillListViewButtonItem _view;

    private HtmlItemViewModel(IProcessInteractionService processInteractionService, ClipboardHistoryItem item)
        : base(processInteractionService, item)
    {
        _logger = this.Log();
        _view = new SillListViewButtonItem(base.PasteCommand);
        _view.DataContext = this;

        InitializeAsync().Forget();
    }

    internal static (ClipboardHistoryItemViewModelBase, SillListViewItem) CreateView(IProcessInteractionService processInteractionService, ClipboardHistoryItem item)
    {
        var viewModel = new HtmlItemViewModel(processInteractionService, item);
        return (viewModel, viewModel._view);
    }

    private async Task InitializeAsync()
    {
        try
        {
            Guard.IsNotNull(Data);
            string text = await Data.GetHtmlFormatAsync();
            _view.Content
                = text
                .Substring(0, Math.Min(text.Length, 256))
                .Trim()
                .Replace("\r\n", "⏎")
                .Replace("\n\r", "⏎")
                .Replace('\r', '⏎')
                .Replace('\n', '⏎');

            _view.PreviewFlyoutContent = text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to initialize {nameof(HtmlItemViewModel)} control.");
        }
    }
}
