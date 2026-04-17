using VinhKhanhGuide.Mobile.Services;
using VinhKhanhGuide.Mobile.Models;
using VinhKhanhGuide.Mobile.ViewModels;

namespace VinhKhanhGuide.Mobile;

public partial class MainPage : ContentPage
{
    private readonly StallListViewModel _viewModel;
    private readonly ExternalEntryStatusService _externalEntryStatusService;
#if DEBUG
    private readonly IExternalEntryService? _externalEntryService;
    private Entry? _payloadEntry;
    private Label? _payloadStatusLabel;
#endif

    public MainPage()
    {
        InitializeComponent();
        BindingContext = _viewModel = ServiceHelper.GetRequiredService<StallListViewModel>();
        _externalEntryStatusService = ServiceHelper.GetRequiredService<ExternalEntryStatusService>();
        ExternalEntryBanner.BindingContext = _externalEntryStatusService;
#if DEBUG
        if (DemoUiOptions.ShowDebugTools)
        {
            _externalEntryService = ServiceHelper.GetRequiredService<IExternalEntryService>();
            AddDebugTools();
        }
#endif
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadAsync();
    }

    private async void OnPreviewNarrationClicked(object? sender, EventArgs e)
    {
        if (sender is Button { CommandParameter: StallSummary stall })
        {
            await _viewModel.PreviewNarrationAsync(stall);
        }
    }

#if DEBUG
    private async void OnOpenPayloadClicked(object? sender, EventArgs e)
    {
        var result = await _externalEntryService!.ProcessAsync(
            _payloadEntry?.Text,
            ExternalEntrySourceType.Manual);

        if (_payloadStatusLabel is not null)
        {
            _payloadStatusLabel.Text = result.UserMessage;
            _payloadStatusLabel.TextColor = result.Succeeded && !result.IsDuplicate
                ? Colors.ForestGreen
                : Color.FromArgb("#6B6258");
        }
    }

    private void OnUseSamplePayloadClicked(object? sender, EventArgs e)
    {
        if (_payloadEntry is not null)
        {
            _payloadEntry.Text = "vinhkhanhguide://stall/1?autoplay=1&lang=en";
        }

        if (_payloadStatusLabel is not null)
        {
            _payloadStatusLabel.Text = "Sample payload loaded.";
            _payloadStatusLabel.TextColor = Colors.DimGray;
        }
    }

    private void AddDebugTools()
    {
        _payloadEntry = new Entry
        {
            Placeholder = "stall:1 or vinhkhanhguide://stall/1?autoplay=1&lang=en",
            ReturnType = ReturnType.Go,
            FontSize = 13
        };

        _payloadStatusLabel = new Label
        {
            FontSize = 12,
            TextColor = Colors.DimGray
        };

        var debugToolsFrame = new Frame
        {
            Padding = 10,
            BorderColor = Color.FromArgb("#DDD6C8"),
            BackgroundColor = Color.FromArgb("#FAF7F1"),
            HasShadow = false,
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Label
                    {
                        Text = "Debug Tools",
                        FontSize = 12,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#6B6258")
                    },
                    new Label
                    {
                        Text = "Manual payload entry for simulator and deep-link checks.",
                        FontSize = 11,
                        TextColor = Color.FromArgb("#8C8274")
                    },
                    new Label
                    {
                        Text = "Demo Flow: 1. Open a location  2. View details  3. Listen to narration",
                        FontSize = 11,
                        TextColor = Color.FromArgb("#8C8274")
                    },
                    _payloadEntry,
                    new HorizontalStackLayout
                    {
                        Spacing = 10,
                        Children =
                        {
                            new Button
                            {
                                Text = "Open location",
                                FontSize = 13,
                                Padding = new Thickness(12, 6)
                            }.Assign(OnOpenPayloadClicked),
                            new Button
                            {
                                Text = "Sample",
                                FontSize = 13,
                                Padding = new Thickness(12, 6)
                            }.Assign(OnUseSamplePayloadClicked)
                        }
                    },
                    _payloadStatusLabel
                }
            }
        };

        MainToolsStack.Children.Add(debugToolsFrame);
    }
#endif

    public static bool IsManualPayloadToolVisible => DemoUiOptions.ShowDebugTools;
}

internal static class ButtonExtensions
{
    public static Button Assign(this Button button, EventHandler handler)
    {
        button.Clicked += handler;
        return button;
    }
}
