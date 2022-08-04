using Jellyfin.Maui.Models;

namespace Jellyfin.Maui.Services;

/// <inheritdoc />
public class LibraryService : ILibraryService
{
    private readonly IItemsClient _itemsClient;
    private readonly ITvShowsClient _tvShowsClient;
    private readonly IUserLibraryClient _userLibraryClient;
    private readonly IUserViewsClient _userViewsClient;
    private readonly IDisplayPreferencesClient _displayPreferencesClient;

    private readonly Guid _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryService"/> class.
    /// </summary>
    /// <param name="itemsClient">Instance of the <see cref="IItemsClient"/> interface.</param>
    /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
    /// <param name="tvShowsClient">Instance of the <see cref="ITvShowsClient"/> interface.</param>
    /// <param name="userLibraryClient">Instance of the <see cref="IUserLibraryClient"/> interface.</param>
    /// <param name="userViewsClient">Instance of the <see cref="IUserViewsClient"/> interface.</param>
    /// <param name="displayPreferencesClient">Instance of the <see cref="IDisplayPreferencesClient"/> interface.</param>
    public LibraryService(
        IItemsClient itemsClient,
        IStateService stateService,
        ITvShowsClient tvShowsClient,
        IUserLibraryClient userLibraryClient,
        IUserViewsClient userViewsClient,
        IDisplayPreferencesClient displayPreferencesClient)
    {
        _itemsClient = itemsClient;
        _tvShowsClient = tvShowsClient;
        _userLibraryClient = userLibraryClient;
        _userViewsClient = userViewsClient;
        _displayPreferencesClient = displayPreferencesClient;

        _userId = stateService.GetCurrentUser().Id;
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetLibrariesAsync(CancellationToken cancellationToken)
    {
        var views = await _userViewsClient.GetUserViewsAsync(_userId, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return views is null ? Array.Empty<BaseItemDto>() : views.Items;
    }

    /// <inheritdoc />
    public async ValueTask<BaseItemDto?> GetLibraryAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _itemsClient.GetItemsAsync(
                userId: _userId,
                ids: new[] { id },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return result.Items.Count == 0 ? null : result.Items[0];
    }

    /// <inheritdoc />
    public async ValueTask<BaseItemDto?> GetItemAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _userLibraryClient.GetItemAsync(
                _userId,
                id,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<BaseItemDtoQueryResult> GetLibraryItemsAsync(
        BaseItemDto library,
        int limit,
        int startIndex,
        CancellationToken cancellationToken)
    {
        return await _itemsClient.GetItemsAsync(
                userId: _userId,
                recursive: true,
                sortOrder: new[] { SortOrder.Ascending },
                parentId: library.Id,
                includeItemTypes: new[] { GetViewType(library.CollectionType) },
                sortBy: new[] { "SortName" },
                imageTypeLimit: 1,
                enableImageTypes: new[] { ImageType.Primary, ImageType.Banner, ImageType.Thumb },
                limit: limit,
                startIndex: startIndex,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetNextUpAsync(CancellationToken cancellationToken)
    {
        var result = await _tvShowsClient.GetNextUpAsync(
                    userId: _userId,
                    limit: 24,
                    fields: new[] { ItemFields.PrimaryImageAspectRatio },
                    imageTypeLimit: 1,
                    enableImageTypes: new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb },
                    enableTotalRecordCount: false,
                    disableFirstEpisode: false,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

        return result.Items;
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetContinueWatchingAsync(CancellationToken cancellationToken)
    {
        var result = await _itemsClient.GetResumeItemsAsync(
                userId: _userId,
                limit: 24,
                fields: new[] { ItemFields.PrimaryImageAspectRatio },
                imageTypeLimit: 1,
                enableImageTypes: new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb },
                enableTotalRecordCount: false,
                mediaTypes: new[] { "Video" },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result.Items;
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetRecentlyAddedAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        return await _userLibraryClient.GetLatestMediaAsync(
                userId: _userId,
                limit: 24,
                fields: new[] { ItemFields.PrimaryImageAspectRatio },
                imageTypeLimit: 1,
                enableImageTypes: new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb },
                parentId: libraryId,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<BaseItemDtoQueryResult> GetSeasonsAsync(Guid seriesId, CancellationToken cancellationToken)
    {
        return await _tvShowsClient.GetSeasonsAsync(
                seriesId,
                _userId,
                new[] { ItemFields.PrimaryImageAspectRatio },
                imageTypeLimit: 1,
                enableImageTypes: new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<BaseItemDtoQueryResult> GetNextUpAsync(Guid seriesId, CancellationToken cancellationToken)
    {
        return await _tvShowsClient.GetNextUpAsync(
                _userId,
                parentId: seriesId,
                fields: new[] { ItemFields.PrimaryImageAspectRatio },
                imageTypeLimit: 1,
                enableImageTypes: new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask<BaseItemDtoQueryResult> GetEpisodesAsync(Guid seriesId, Guid seasonId, CancellationToken cancellationToken)
    {
        return await _tvShowsClient.GetEpisodesAsync(
                seriesId,
                _userId,
                new[] { ItemFields.PrimaryImageAspectRatio },
                seasonId: seasonId,
                imageTypeLimit: 1,
                enableImageTypes: new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask<DisplayPreferencesModel> GetDisplayPreferencesAsync(CancellationToken cancellationToken)
    {
        var displayPreferences = await _displayPreferencesClient.GetDisplayPreferencesAsync("usersettings", _userId, "emby", cancellationToken)
            .ConfigureAwait(false);

        var homeSections = new List<DisplayPreferencesModel.HomeSection>();
        homeSections.AddRange(Enum.GetValues<DisplayPreferencesModel.HomeSection>());
        foreach (var customPreference in displayPreferences.CustomPrefs)
        {
            var section = GetHomeSection(customPreference.Key, customPreference.Value);
            if (section is not null)
            {
                homeSections.Add(section.Value);
            }
        }

        return new DisplayPreferencesModel(homeSections);
    }

    private static BaseItemKind GetViewType(string collectionType)
    {
        return collectionType switch
        {
            "tvshows" => BaseItemKind.Series,
            "movies" => BaseItemKind.Movie,
            "books" => BaseItemKind.Book,
            "music" => BaseItemKind.Audio,
            _ => BaseItemKind.Folder
        };
    }

    private static DisplayPreferencesModel.HomeSection? GetHomeSection(string preference, string value)
    {
        if (preference.StartsWith("homesection", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals("smalllibrarytiles", value, StringComparison.OrdinalIgnoreCase)
                || string.Equals("librarybuttons", value, StringComparison.OrdinalIgnoreCase))
            {
                return DisplayPreferencesModel.HomeSection.LibraryTiles;
            }

            if (string.Equals("resume", value, StringComparison.OrdinalIgnoreCase))
            {
                return DisplayPreferencesModel.HomeSection.Resume;
            }

            if (string.Equals("nextup", value, StringComparison.OrdinalIgnoreCase))
            {
                return DisplayPreferencesModel.HomeSection.NextUp;
            }

            if (string.Equals("latestmedia", value, StringComparison.OrdinalIgnoreCase))
            {
                return DisplayPreferencesModel.HomeSection.LatestMedia;
            }
        }

        return null;
    }
}
