using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Mvvm.ViewModels.Facades;

/// <summary>
/// A view model showing cards, with a details header for the focused card.
/// </summary>
public abstract partial class BrowseViewModel : BaseViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFocusedCard))]
    private ItemCardModel? _focusedCard;

    [ObservableProperty]
    private string? _focusedTitle;

    [ObservableProperty]
    private string? _focusedSubtitle;

    [ObservableProperty]
    private string? _focusedOverview;

    [ObservableProperty]
    private string? _focusedLogoUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowseViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">The navigation service.</param>
    /// <param name="applicationService">The application service.</param>
    /// <param name="backdropService">The backdrop service.</param>
    /// <param name="cardFactory">The card factory.</param>
    protected BrowseViewModel(
        INavigationService navigationService,
        IApplicationService applicationService,
        IBackdropService backdropService,
        CardFactory cardFactory)
        : base(navigationService, applicationService)
    {
        BackdropService = backdropService;
        CardFactory = cardFactory;
    }

    /// <inheritdoc />
    public override bool UsesBackdrop => true;

    /// <summary>
    /// Gets the info chips of the focused card. They change with every focus move, so they are updated in place.
    /// </summary>
    public ObservableCollection<InfoChip> FocusedInfo { get; } = [];

    /// <summary>
    /// Gets a value indicating whether a card is focused.
    /// </summary>
    public bool HasFocusedCard => FocusedCard is not null;

    /// <summary>
    /// Gets the backdrop service.
    /// </summary>
    protected IBackdropService BackdropService { get; }

    /// <summary>
    /// Gets the card factory.
    /// </summary>
    protected CardFactory CardFactory { get; }

    /// <inheritdoc />
    protected override ValueTask OnReturnedAsync()
    {
        BackdropService.SetBackdrop(FocusedCard?.BackdropUrl);
        return base.OnReturnedAsync();
    }

    [RelayCommand]
    private void FocusCard(ItemCardModel? card)
    {
        if (card is null || ReferenceEquals(card, FocusedCard))
        {
            return;
        }

        FocusedCard = card;
        var item = card.Item;
        if (item is null)
        {
            FocusedTitle = card.Title;
            FocusedSubtitle = card.Subtitle;
            FocusedOverview = null;
            FocusedInfo.Clear();
            FocusedLogoUrl = null;
        }
        else
        {
            FocusedTitle = item.Type is BaseItemDto_Type.Episode or BaseItemDto_Type.Season ? item.SeriesName ?? item.Name : item.Name;
            FocusedSubtitle = item.Type switch
            {
                BaseItemDto_Type.Episode => Formatting.FormatEpisodeTitle(item),
                BaseItemDto_Type.Season => item.Name,
                _ => null
            };
            FocusedOverview = item.Overview;
            InfoChip.UpdateInPlace(FocusedInfo, InfoRowBuilder.Build(item, DateTime.Now, includeMediaDetails: false));
            FocusedLogoUrl = CardFactory.ImageUrls.GetLogoUrl(item);
        }

        BackdropService.SetBackdrop(card.BackdropUrl);
    }
}
