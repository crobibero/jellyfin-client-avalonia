using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Mvvm.Services;
using Jellyfin.Mvvm.ViewModels.Facades;

namespace Jellyfin.Mvvm.ViewModels;

/// <summary>
/// Item view model.
/// </summary>
public partial class ItemViewModel : BaseItemViewModel
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
    private string? _director;

    [ObservableProperty]
    private string? _runtime;

    [ObservableProperty]
    private string? _endTime;

    [ObservableProperty]
    private string? _videoStream;

    [ObservableProperty]
    private string? _audioStream;

    [ObservableProperty]
    private BaseItemDto? _nextUpItem;

    [ObservableProperty]
    private BaseItemDto? _parentShow;

    [ObservableProperty]
    private BaseItemDto? _parentSeason;

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
    /// <param name="applicationService">Instance of the <see cref="IApplicationService"/> interface.</param>
    public ItemViewModel(ILibraryService libraryService, INavigationService navigationService, IApplicationService applicationService)
        : base(navigationService, applicationService)
    {
        _libraryService = libraryService;
        _navigationService = navigationService;
    }

    /// <inheritdoc/>
    public override async ValueTask InitializeAsync()
    {
        Item = await _libraryService.GetItemAsync(ItemId)
           .ConfigureAwait(true);

        if (Item is null)
        {
            return;
        }

        PopulateBase();

        switch (Item.Type)
        {
            case BaseItemKind.Series:
                PopulateSeriesViewAsync().SafeFireAndForget();
                break;
            case BaseItemKind.Season:
                PopulateCurrentShowAsync().SafeFireAndForget();
                PopulateSeasonEpisodesAsync().SafeFireAndForget();
                break;
            case BaseItemKind.Episode:
                PopulateCurrentShowAsync().SafeFireAndForget();
                PopulateCurrentSeasonAsync().SafeFireAndForget();
                break;
        }
    }

    private void Reset()
    {
        Title = null;
        SubTitle = null;
        Description = null;
        Director = null;
        Runtime = null;
        EndTime = null;
        VideoStream = null;
        AudioStream = null;
        NextUpItem = null;
        ParentShow = null;
        ParentSeason = null;
        Seasons = null;
        Episodes = null;
        People = null;
    }

    private void PopulateBase()
    {
        Reset();

        Title = Item!.Type switch
        {
            BaseItemKind.Episode => Item.SeriesName,
            BaseItemKind.Season => Item.SeriesName,
            _ => Item.Name
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
        var seasonResult = await _libraryService.GetSeasonsAsync(Item!.Id).ConfigureAwait(false);
        Seasons = seasonResult.Items;

        var nextUpResult = await _libraryService.GetNextUpAsync(Item.Id).ConfigureAwait(false);
        if (nextUpResult.Items.Count > 0)
        {
            NextUpItem = nextUpResult.Items[0];
        }
    }

    private async ValueTask PopulateSeasonEpisodesAsync()
    {
        if (Item?.SeriesId is null)
        {
            return;
        }

        var episodeResult = await _libraryService.GetEpisodesAsync(Item.SeriesId.Value, Item.Id).ConfigureAwait(false);
        Episodes = episodeResult.Items;
    }

    private async ValueTask PopulateCurrentSeasonAsync()
    {
        if (Item?.SeasonId is null)
        {
            return;
        }

        ParentSeason = await _libraryService.GetItemAsync(Item.SeasonId.Value).ConfigureAwait(false);
    }

    private async ValueTask PopulateCurrentShowAsync()
    {
        if (Item?.SeriesId is null)
        {
            return;
        }

        ParentShow = await _libraryService.GetItemAsync(Item.SeriesId.Value).ConfigureAwait(false);
    }

    [RelayCommand]
    private void PlayItem()
    {
        _navigationService.NavigateToPlayer(ItemId);
    }
}
