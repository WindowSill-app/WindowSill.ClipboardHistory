using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using WindowSill.API;

namespace WindowSill.ClipboardHistory.UI;

internal sealed partial class EmptyOrDisabledItemViewModel : ObservableObject
{
    private readonly SillView _view;
    private readonly TextBlock _emptyClipboardTextBlock = new();
    private readonly SillOrientedStackPanel _clipboardHistoryDisabledStackPanel = new();

    private EmptyOrDisabledItemViewModel()
    {
        _view = new SillView()
            .DataContext(
                this,
                (view, viewModel) => view
                    .Content(
                        new Grid()
                            .Margin(x => x.ThemeResource("SillCommandContentMargin"))
                            .Children(
                                _clipboardHistoryDisabledStackPanel
                                    .Spacing(8)
                                    .Children(
                                        new HyperlinkButton()
                                            .HorizontalAlignment(HorizontalAlignment.Stretch)
                                            .VerticalAlignment(VerticalAlignment.Stretch)
                                            .HorizontalContentAlignment(HorizontalAlignment.Center)
                                            .Padding(x => x.ThemeResource("SillButtonPadding"))
                                            .FontSize(x => x.ThemeResource("SillFontSize"))
                                            .Command(() => viewModel.OpenWindowsClipboardHistoryCommand)
                                            .Content(
                                                new TextBlock()
                                                    .TextWrapping(TextWrapping.WrapWholeWords)
                                                    .TextAlignment(TextAlignment.Center)
                                                    .HorizontalAlignment(HorizontalAlignment.Center)
                                                    .VerticalAlignment(VerticalAlignment.Center)
                                                    .Text("/WindowSill.ClipboardHistory/Misc/EnableClipboardHistory".GetLocalizedString())
                                            )
                                    ),

                                _emptyClipboardTextBlock
                                    .TextWrapping(TextWrapping.WrapWholeWords)
                                    .TextAlignment(TextAlignment.Center)
                                    .HorizontalAlignment(HorizontalAlignment.Center)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .FontSize(x => x.ThemeResource("SillFontSize"))
                                    .Text("/WindowSill.ClipboardHistory/Misc/EmptyClipboard".GetLocalizedString())
                            )
                    )
            );

        Clipboard.HistoryEnabledChanged += Clipboard_HistoryEnabledChanged;
        UpdateUI();
    }

    internal static SillView CreateView()
    {
        return new EmptyOrDisabledItemViewModel()._view;
    }

    [RelayCommand]
    private async Task OpenWindowsClipboardHistoryAsync()
    {
        await Launcher.LaunchUriAsync(new Uri("ms-settings:clipboard"));
        UpdateUI();
    }

    private void Clipboard_HistoryEnabledChanged(object? sender, object e)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (Clipboard.IsHistoryEnabled())
        {
            _clipboardHistoryDisabledStackPanel.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            _emptyClipboardTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }
        else
        {
            _clipboardHistoryDisabledStackPanel.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
            _emptyClipboardTextBlock.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }
    }
}
