using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Maui.Services;
using Jellyfin.Maui.ViewModels.Facades;

namespace Jellyfin.Maui.ViewModels;

/// <summary>
/// Item view model.
/// </summary>
public partial class ItemViewModel : BaseIdViewModel
{
    private readonly ILibraryService _libraryService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private string? _subTitle;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _videoStream;

    [ObservableProperty]
    private string? _audioStream;

    [ObservableProperty]
    private BaseItemDto? _nextUpItem;

    [ObservableProperty]
    private IReadOnlyList<BaseItemDto>? _seasons;

    [ObservableProperty]
    private IReadOnlyList<BaseItemDto>? _episodes;

    [ObservableProperty]
    private IReadOnlyList<BaseItemPerson>? _people;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemViewModel"/> class.
    /// </summary>
    /// <param name="libraryService">Instance of the <see cref="ILibraryService"/> interface.</param>
    /// <param name="navigationService">Instance of the <see cref="INavigationService"/> interface.</param>
    public ItemViewModel(ILibraryService libraryService, INavigationService navigationService)
        : base(navigationService)
    {
        _libraryService = libraryService;
        _navigationService = navigationService;
    }

    /// <inheritdoc/>
    public override async ValueTask InitializeAsync()
    {
        var item = await _libraryService.GetItemAsync(Id).ConfigureAwait(false);

        if (item is null)
        {
            _navigationService.NavigateHome();
            return;
        }

        Item = item;
        PopulateBase();
        switch (Item.Type)
        {
            case BaseItemKind.Series:
                PopulateSeriesViewAsync().SafeFireAndForget();
                break;
            case BaseItemKind.Season:
                PopulateSeasonViewAsync().SafeFireAndForget();
                break;
        }
    }

    private void PopulateBase()
    {
        Title = Item.Type switch
        {
            BaseItemKind.Episode => Item.SeriesName,
            BaseItemKind.Season => Item.SeriesName,
            _ => Item.Name?.ToString(CultureInfo.InvariantCulture) ?? string.Empty
        };

        SubTitle = Item.Type switch
        {
            BaseItemKind.Episode => $"S{Item.ParentIndexNumber} E{Item.IndexNumber} {Item.Name}",
            BaseItemKind.Season => Item.SeasonName,
            _ => Item.ProductionYear?.ToString(CultureInfo.InvariantCulture) ?? string.Empty
        };

        Description = Item.Overview;
    }

    private async ValueTask PopulateSeriesViewAsync()
    {
        var seasonResult = await _libraryService.GetSeasonsAsync(Item.Id).ConfigureAwait(false);
        Seasons = seasonResult.Items;

        var nextUpResult = await _libraryService.GetNextUpAsync(Item.Id).ConfigureAwait(false);
        if (nextUpResult.Items.Count > 0)
        {
            NextUpItem = nextUpResult.Items[0];
        }
    }

    private async ValueTask PopulateSeasonViewAsync()
    {
        if (Item.SeriesId is null)
        {
            return;
        }

        var episodeResult = await _libraryService.GetEpisodesAsync(Item.SeriesId.Value, Item.Id).ConfigureAwait(false);
        Episodes = episodeResult.Items;
    }
}
