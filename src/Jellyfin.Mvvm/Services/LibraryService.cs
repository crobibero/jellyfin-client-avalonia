using Jellyfin.Mvvm.Models;

namespace Jellyfin.Mvvm.Services;

/// <inheritdoc />
public class LibraryService : ILibraryService
{
    internal static readonly ItemFields[] CardFields =
    [
        ItemFields.PrimaryImageAspectRatio,
        ItemFields.Overview,
        ItemFields.MediaSourceCount,
    ];

    private static readonly ImageType[] _cardImageTypes = [ImageType.Primary, ImageType.Backdrop, ImageType.Thumb, ImageType.Logo];

    private readonly JellyfinApiClient _jellyfinApiClient;
    private readonly IStateService _stateService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryService"/> class.
    /// </summary>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    /// <param name="jellyfinApiClient">The Jellyfin api client.</param>
    public LibraryService(
        IStateService stateService,
        JellyfinApiClient jellyfinApiClient)
    {
        _jellyfinApiClient = jellyfinApiClient;
        _stateService = stateService;
    }

    private Guid UserId => _stateService.GetCurrentUser().Id ?? throw new InvalidOperationException("No user is signed in");

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetLibrariesAsync(CancellationToken cancellationToken)
    {
        var views = await _jellyfinApiClient.UserViews.GetAsync(c => c.QueryParameters.UserId = UserId, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return views?.Items ?? [];
    }

    /// <inheritdoc />
    public async ValueTask<BaseItemDto?> GetItemAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _jellyfinApiClient.Items[id].GetAsync(c => c.QueryParameters.UserId = UserId, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<BaseItemDtoQueryResult> GetItemsAsync(ItemQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var result = await _jellyfinApiClient.Items.GetAsync(
                c =>
                {
                    c.QueryParameters.UserId = UserId;
                    c.QueryParameters.ParentId = query.ParentId;
                    c.QueryParameters.Recursive = query.Recursive;
                    c.QueryParameters.SortBy = [.. query.SortBy];
                    c.QueryParameters.SortOrder = [query.SortOrder];
                    c.QueryParameters.IncludeItemTypes = query.IncludeItemTypes is null ? null : [.. query.IncludeItemTypes];
                    c.QueryParameters.MediaTypes = query.MediaTypes is null ? null : [.. query.MediaTypes];
                    c.QueryParameters.IsPlayed = query.IsPlayed;
                    c.QueryParameters.IsFavorite = query.IsFavorite;
                    c.QueryParameters.GenreIds = query.GenreIds?.Select(g => (Guid?)g).ToArray();
                    c.QueryParameters.PersonIds = query.PersonIds?.Select(g => (Guid?)g).ToArray();
                    c.QueryParameters.ArtistIds = query.ArtistIds?.Select(g => (Guid?)g).ToArray();
                    c.QueryParameters.Ids = query.Ids?.Select(g => (Guid?)g).ToArray();
                    c.QueryParameters.SearchTerm = query.SearchTerm;
                    c.QueryParameters.StartIndex = query.StartIndex;
                    c.QueryParameters.Limit = query.Limit;
                    c.QueryParameters.Fields = query.Fields is null ? CardFields : [.. query.Fields];
                    c.QueryParameters.ImageTypeLimit = 1;
                    c.QueryParameters.EnableImageTypes = _cardImageTypes;
                    c.QueryParameters.EnableTotalRecordCount = true;
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result ?? new BaseItemDtoQueryResult { Items = [], TotalRecordCount = 0 };
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetNextUpAsync(Guid? seriesId, int limit, CancellationToken cancellationToken)
    {
        var result = await _jellyfinApiClient.Shows.NextUp.GetAsync(
                c =>
                {
                    c.QueryParameters.UserId = UserId;
                    c.QueryParameters.SeriesId = seriesId;
                    c.QueryParameters.Limit = limit;
                    c.QueryParameters.Fields = CardFields;
                    c.QueryParameters.ImageTypeLimit = 1;
                    c.QueryParameters.EnableImageTypes = _cardImageTypes;
                    c.QueryParameters.EnableTotalRecordCount = false;
                    c.QueryParameters.EnableResumable = false;
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result?.Items ?? [];
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetResumeAsync(MediaType mediaType, CancellationToken cancellationToken)
    {
        var result = await _jellyfinApiClient.UserItems.Resume.GetAsync(
                c =>
                {
                    c.QueryParameters.UserId = UserId;
                    c.QueryParameters.Limit = 24;
                    c.QueryParameters.Fields = CardFields;
                    c.QueryParameters.ImageTypeLimit = 1;
                    c.QueryParameters.EnableImageTypes = _cardImageTypes;
                    c.QueryParameters.EnableTotalRecordCount = false;
                    c.QueryParameters.MediaTypes = [mediaType];
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result?.Items ?? [];
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetLatestAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        var items = await _jellyfinApiClient.Items.Latest.GetAsync(
                c =>
                {
                    c.QueryParameters.UserId = UserId;
                    c.QueryParameters.Limit = 24;
                    c.QueryParameters.Fields = CardFields;
                    c.QueryParameters.ImageTypeLimit = 1;
                    c.QueryParameters.EnableImageTypes = _cardImageTypes;
                    c.QueryParameters.ParentId = libraryId;
                    c.QueryParameters.GroupItems = true;
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return items ?? [];
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetSeasonsAsync(Guid seriesId, CancellationToken cancellationToken)
    {
        var result = await _jellyfinApiClient.Shows[seriesId].Seasons.GetAsync(
                c =>
                {
                    c.QueryParameters.UserId = UserId;
                    c.QueryParameters.Fields = CardFields;
                    c.QueryParameters.ImageTypeLimit = 1;
                    c.QueryParameters.EnableImageTypes = _cardImageTypes;
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result?.Items ?? [];
    }

    /// <inheritdoc/>
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetEpisodesAsync(Guid seriesId, Guid? seasonId = null, Guid? startItemId = null, CancellationToken cancellationToken = default)
    {
        var result = await _jellyfinApiClient.Shows[seriesId].Episodes.GetAsync(
                c =>
                {
                    c.QueryParameters.UserId = UserId;
                    c.QueryParameters.SeasonId = seasonId;
                    c.QueryParameters.StartItemId = startItemId;
                    c.QueryParameters.Fields = [.. CardFields, ItemFields.MediaStreams];
                    c.QueryParameters.ImageTypeLimit = 1;
                    c.QueryParameters.EnableImageTypes = _cardImageTypes;
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result?.Items ?? [];
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetSimilarAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var result = await _jellyfinApiClient.Items[itemId].Similar.GetAsync(
                c =>
                {
                    c.QueryParameters.UserId = UserId;
                    c.QueryParameters.Limit = 16;
                    c.QueryParameters.Fields = CardFields;
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result?.Items ?? [];
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetSpecialFeaturesAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var result = await _jellyfinApiClient.Items[itemId].SpecialFeatures.GetAsync(c => c.QueryParameters.UserId = UserId, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result ?? [];
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetLocalTrailersAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var result = await _jellyfinApiClient.Items[itemId].LocalTrailers.GetAsync(c => c.QueryParameters.UserId = UserId, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result ?? [];
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<MediaSegmentDto>> GetMediaSegmentsAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var result = await _jellyfinApiClient.MediaSegments[itemId].GetAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result?.Items ?? [];
    }

    /// <inheritdoc />
    public async ValueTask<UserItemDataDto?> SetFavoriteAsync(Guid itemId, bool isFavorite, CancellationToken cancellationToken)
    {
        var builder = _jellyfinApiClient.UserFavoriteItems[itemId];
        return isFavorite
            ? await builder.PostAsync(c => c.QueryParameters.UserId = UserId, cancellationToken: cancellationToken).ConfigureAwait(false)
            : await builder.DeleteAsync(c => c.QueryParameters.UserId = UserId, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<UserItemDataDto?> SetPlayedAsync(Guid itemId, bool isPlayed, CancellationToken cancellationToken)
    {
        var builder = _jellyfinApiClient.UserPlayedItems[itemId];
        return isPlayed
            ? await builder.PostAsync(c => c.QueryParameters.UserId = UserId, cancellationToken: cancellationToken).ConfigureAwait(false)
            : await builder.DeleteAsync(c => c.QueryParameters.UserId = UserId, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask<DisplayPreferencesModel?> GetDisplayPreferencesAsync(CancellationToken cancellationToken)
    {
        var displayPreferences = await _jellyfinApiClient.DisplayPreferences["usersettings"].GetAsync(
                c =>
                {
                    c.QueryParameters.UserId = UserId;
                    c.QueryParameters.Client = "emby";
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return DisplayPreferencesModel.FromCustomPreferences(displayPreferences?.CustomPrefs?.AdditionalData);
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> SearchPeopleAsync(string searchTerm, int limit, CancellationToken cancellationToken)
    {
        var result = await _jellyfinApiClient.Persons.GetAsync(
                c =>
                {
                    c.QueryParameters.UserId = UserId;
                    c.QueryParameters.SearchTerm = searchTerm;
                    c.QueryParameters.Limit = limit;
                    c.QueryParameters.Fields = CardFields;
                    c.QueryParameters.ImageTypeLimit = 1;
                    c.QueryParameters.EnableImageTypes = _cardImageTypes;
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result?.Items ?? [];
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetPlayableItemsAsync(BaseItemDto item, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(item);
        var id = item.Id ?? throw new ArgumentException("Item has no id", nameof(item));

        switch (item.Type)
        {
            case BaseItemDto_Type.Series:
                return await GetEpisodesAsync(id, cancellationToken: cancellationToken).ConfigureAwait(false);
            case BaseItemDto_Type.Season when item.SeriesId is not null:
                return await GetEpisodesAsync(item.SeriesId.Value, id, cancellationToken: cancellationToken).ConfigureAwait(false);
            case BaseItemDto_Type.Episode when item.SeriesId is not null:
                // Queue the remaining episodes of the series so next up works.
                var episodes = await GetEpisodesAsync(item.SeriesId.Value, startItemId: id, cancellationToken: cancellationToken).ConfigureAwait(false);
                return episodes.Count == 0 ? [item] : episodes;
            case BaseItemDto_Type.MusicAlbum or BaseItemDto_Type.Playlist or BaseItemDto_Type.BoxSet or BaseItemDto_Type.MusicArtist or BaseItemDto_Type.Folder or BaseItemDto_Type.CollectionFolder:
                var result = await GetItemsAsync(
                        new ItemQuery
                        {
                            ParentId = item.Type == BaseItemDto_Type.MusicArtist ? null : id,
                            Recursive = true,
                            ArtistIds = item.Type == BaseItemDto_Type.MusicArtist ? [id] : null,
                            SortBy = item.Type is BaseItemDto_Type.Playlist ? [ItemSortBy.Default] : item.Type is BaseItemDto_Type.BoxSet ? [ItemSortBy.PremiereDate] : [ItemSortBy.ParentIndexNumber, ItemSortBy.IndexNumber, ItemSortBy.SortName],
                            MediaTypes = [MediaType.Video, MediaType.Audio],
                            Fields = [.. CardFields, ItemFields.MediaStreams],
                            Limit = 500,
                        },
                        cancellationToken)
                    .ConfigureAwait(false);
                return result.Items ?? [];
            default:
                return [item];
        }
    }
}
