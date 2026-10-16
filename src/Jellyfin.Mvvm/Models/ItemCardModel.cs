using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Jellyfin.Mvvm.Preferences;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Mvvm.Models;

/// <summary>
/// A card shown in rows and grids.
/// </summary>
public partial class ItemCardModel : ObservableObject
{
    /// <summary>
    /// The poster card image height used for image requests.
    /// </summary>
    /// <remarks>
    /// Card images are requested at 1.6 times the card height in JellyfinConverters. The image cache is bounded by
    /// count, so a larger request for one shape costs memory for every cached image of that shape.
    /// </remarks>
    public const int PosterRequestHeight = 360;

    /// <summary>
    /// The thumb card image height used for image requests.
    /// </summary>
    public const int ThumbRequestHeight = 240;

    /// <summary>
    /// The square card image height used for image requests.
    /// </summary>
    public const int SquareRequestHeight = 288;

    [ObservableProperty]
    private bool _isPlayed;

    [ObservableProperty]
    private bool _isFavorite;

    [ObservableProperty]
    private double? _progress;

    [ObservableProperty]
    private int? _unplayedCount;

    /// <summary>
    /// Gets the item or person id.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the item, or <c>null</c> for non-item cards such as people.
    /// </summary>
    public BaseItemDto? Item { get; init; }

    /// <summary>
    /// Gets the card kind.
    /// </summary>
    public CardKind Kind { get; init; } = CardKind.Item;

    /// <summary>
    /// Gets the card title.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the card subtitle.
    /// </summary>
    public string? Subtitle { get; init; }

    /// <summary>
    /// Gets the image url.
    /// </summary>
    public string? ImageUrl { get; init; }

    /// <summary>
    /// Gets the blurhash shown while the image loads.
    /// </summary>
    public string? ImageBlurHash { get; init; }

    /// <summary>
    /// Gets the card shape.
    /// </summary>
    public CardShape Shape { get; init; }

    /// <summary>
    /// Gets the text shown when there is no image.
    /// </summary>
    public string? PlaceholderText { get; init; }

    /// <summary>
    /// Gets the position to start playback at, for chapter cards.
    /// </summary>
    public long? StartPositionTicks { get; init; }

    /// <summary>
    /// Gets a value indicating whether the card has an image.
    /// </summary>
    public bool HasImage => !string.IsNullOrEmpty(ImageUrl);

    /// <summary>
    /// Gets a value indicating whether the progress bar is shown.
    /// </summary>
    public bool HasProgress => Progress is > 0;

    /// <summary>
    /// Gets a value indicating whether the unplayed count is shown.
    /// </summary>
    public bool HasUnplayedCount => UnplayedCount is > 0 && !IsPlayed;

    /// <summary>
    /// Gets the backdrop image url, used when the card is focused.
    /// </summary>
    public string? BackdropUrl { get; init; }

    /// <summary>
    /// Create a card for an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="shape">The card shape.</param>
    /// <param name="imageUrlService">The image url service.</param>
    /// <param name="preferences">The user interface preferences.</param>
    /// <param name="titleStyle">How the title is built.</param>
    /// <returns>The card.</returns>
    public static ItemCardModel FromItem(
        BaseItemDto item,
        CardShape shape,
        IImageUrlService imageUrlService,
        UserInterfacePreferences preferences,
        CardTitleStyle titleStyle = CardTitleStyle.Default)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(imageUrlService);
        ArgumentNullException.ThrowIfNull(preferences);

        var (title, subtitle) = GetTitles(item, titleStyle);
        var image = imageUrlService.GetCardImage(item, shape, GetImageRequestHeight(shape), preferences.UseSeriesThumbnails);
        var card = new ItemCardModel
        {
            Id = item.Id ?? Guid.Empty,
            Item = item,
            Title = title,
            Subtitle = subtitle,
            Shape = shape,
            ImageUrl = image?.Url,
            ImageBlurHash = image?.BlurHash,
            BackdropUrl = imageUrlService.GetBackdropUrl(item),
            PlaceholderText = item.Name,
        };

        card.UpdateUserData(item.UserData, preferences);
        return card;
    }

    /// <summary>
    /// Create a card for a person.
    /// </summary>
    /// <param name="person">The person.</param>
    /// <param name="imageUrlService">The image url service.</param>
    /// <returns>The card.</returns>
    public static ItemCardModel FromPerson(BaseItemPerson person, IImageUrlService imageUrlService)
    {
        ArgumentNullException.ThrowIfNull(person);
        ArgumentNullException.ThrowIfNull(imageUrlService);

        var image = imageUrlService.GetPersonImage(person, PosterRequestHeight);
        return new ItemCardModel
        {
            Id = person.Id ?? Guid.Empty,
            Kind = CardKind.Person,
            Title = person.Name,
            Subtitle = string.IsNullOrEmpty(person.Role) ? person.Type?.ToString() : person.Role,
            Shape = CardShape.Poster,
            ImageUrl = image?.Url,
            ImageBlurHash = image?.BlurHash,
            PlaceholderText = person.Name,
        };
    }

    /// <summary>
    /// Create a card for a chapter.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="chapter">The chapter.</param>
    /// <param name="index">The chapter index.</param>
    /// <param name="imageUrlService">The image url service.</param>
    /// <returns>The card.</returns>
    public static ItemCardModel FromChapter(BaseItemDto item, ChapterInfo chapter, int index, IImageUrlService imageUrlService)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(chapter);
        ArgumentNullException.ThrowIfNull(imageUrlService);

        var start = TimeSpan.FromTicks(chapter.StartPositionTicks ?? 0);
        return new ItemCardModel
        {
            Id = item.Id ?? Guid.Empty,
            Item = item,
            Kind = CardKind.Chapter,
            Title = chapter.Name,
            Subtitle = Formatting.FormatDuration(start),
            Shape = CardShape.Thumb,
            ImageUrl = imageUrlService.GetChapterImageUrl(item.Id ?? Guid.Empty, index, chapter.ImageTag, ThumbRequestHeight * 16 / 9),
            PlaceholderText = chapter.Name,
            StartPositionTicks = chapter.StartPositionTicks,
        };
    }

    /// <summary>
    /// Gets the image request height for a card shape.
    /// </summary>
    /// <param name="shape">The card shape.</param>
    /// <returns>The height in pixels.</returns>
    public static int GetImageRequestHeight(CardShape shape) => shape switch
    {
        CardShape.Thumb => ThumbRequestHeight,
        CardShape.Square => SquareRequestHeight,
        _ => PosterRequestHeight
    };

    /// <summary>
    /// Update the user data indicators.
    /// </summary>
    /// <param name="userData">The user data.</param>
    /// <param name="preferences">The user interface preferences.</param>
    public void UpdateUserData(UserItemDataDto? userData, UserInterfacePreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        IsFavorite = userData?.IsFavorite == true;

        var behavior = preferences.WatchedIndicatorBehavior;
        var showIndicators = behavior switch
        {
            WatchedIndicatorBehavior.Never => false,
            WatchedIndicatorBehavior.EpisodesOnly => Item?.Type == BaseItemDto_Type.Episode,
            _ => true
        };

        IsPlayed = showIndicators && userData?.Played == true;
        UnplayedCount = showIndicators && behavior != WatchedIndicatorBehavior.HideUnwatched ? userData?.UnplayedItemCount : null;

        var hasProgressMedia = Item?.MediaType is BaseItemDto_MediaType.Video or BaseItemDto_MediaType.Book;
        Progress = hasProgressMedia && userData?.PlayedPercentage is { } percentage and > 0
            ? Math.Clamp(percentage / 100.0, 0, 1)
            : null;
    }

    /// <summary>
    /// Gets the title and subtitle for an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="style">The title style.</param>
    /// <returns>The title and subtitle.</returns>
    public static (string? Title, string? Subtitle) GetTitles(BaseItemDto item, CardTitleStyle style = CardTitleStyle.Default)
    {
        ArgumentNullException.ThrowIfNull(item);

        switch (item.Type)
        {
            case BaseItemDto_Type.Episode when style == CardTitleStyle.Default:
                return (item.SeriesName ?? item.Name, Formatting.FormatEpisodeTitle(item));
            case BaseItemDto_Type.Episode:
                return (Formatting.FormatEpisodeNumber(item) is { Length: > 0 } number ? $"{number} {item.Name}" : item.Name, Formatting.FormatRuntime(item.RunTimeTicks));
            case BaseItemDto_Type.Season when style == CardTitleStyle.Default:
                return (item.SeriesName ?? item.Name, item.Name);
            case BaseItemDto_Type.Season:
                return (item.Name, null);
            case BaseItemDto_Type.Series:
                return (item.Name, Formatting.FormatYearRange(item));
            case BaseItemDto_Type.MusicAlbum:
                return (item.Name, item.AlbumArtist);
            case BaseItemDto_Type.Audio:
                return (item.Name, item.AlbumArtist ?? item.Album);
            case BaseItemDto_Type.CollectionFolder or BaseItemDto_Type.UserView or BaseItemDto_Type.Folder or BaseItemDto_Type.Playlist:
                return (item.Name, null);
            case BaseItemDto_Type.MusicArtist or BaseItemDto_Type.Person:
                return (item.Name, null);
            default:
                return (item.Name, item.ProductionYear?.ToString(CultureInfo.CurrentCulture));
        }
    }

    partial void OnProgressChanged(double? value) => OnPropertyChanged(nameof(HasProgress));

    partial void OnUnplayedCountChanged(int? value) => OnPropertyChanged(nameof(HasUnplayedCount));

    partial void OnIsPlayedChanged(bool value) => OnPropertyChanged(nameof(HasUnplayedCount));
}

/// <summary>
/// The kind of card.
/// </summary>
public enum CardKind
{
    /// <summary>
    /// An item card.
    /// </summary>
    Item = 0,

    /// <summary>
    /// A person card.
    /// </summary>
    Person = 1,

    /// <summary>
    /// A chapter card.
    /// </summary>
    Chapter = 2
}

/// <summary>
/// How card titles are built.
/// </summary>
public enum CardTitleStyle
{
    /// <summary>
    /// Episodes and seasons show the series name as the title.
    /// </summary>
    Default = 0,

    /// <summary>
    /// Episodes and seasons show their own name, used inside a series.
    /// </summary>
    WithinSeries = 1
}
