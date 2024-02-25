using Jellyfin.Mvvm.Models;

namespace Jellyfin.Mvvm.Services;

/// <inheritdoc />
public class LibraryService : ILibraryService
{
    private readonly JellyfinApiClient _jellyfinApiClient;

    private readonly Guid _userId;

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
        _userId = stateService.GetCurrentUser().Id ?? throw new InvalidOperationException();
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetLibrariesAsync(CancellationToken cancellationToken)
    {
        var views = await _jellyfinApiClient.Users[_userId].Views.GetAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return views?.Items is null ? Array.Empty<BaseItemDto>() : views.Items;
    }

    /// <inheritdoc />
    public async ValueTask<BaseItemDto?> GetLibraryAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _jellyfinApiClient.Users[_userId].Items.GetAsync(
            c => c.QueryParameters.Ids = [id], cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (result?.Items is null)
        {
            return null;
        }

        return result.Items.Count == 0 ? null : result.Items[0];
    }

    /// <inheritdoc />
    public async ValueTask<BaseItemDto?> GetItemAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _jellyfinApiClient.Users[_userId].Items[id].GetAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<BaseItemDtoQueryResult?> GetLibraryItemsAsync(
        BaseItemDto library,
        int limit,
        int startIndex,
        CancellationToken cancellationToken)
    {
        return await _jellyfinApiClient.Users[_userId].Items.GetAsync(
                c =>
                {
                    c.QueryParameters.Recursive = true;
                    c.QueryParameters.SortOrder = [SortOrder.Ascending.ToString()];
                    c.QueryParameters.ParentId = library.Id;
                    c.QueryParameters.IncludeItemTypes = [GetViewType(library.CollectionType)];
                    c.QueryParameters.SortBy = ["SortName"];
                    c.QueryParameters.ImageTypeLimit = 1;
                    c.QueryParameters.EnableImageTypes = [ImageType.Primary.ToString(), ImageType.Banner.ToString(), ImageType.Thumb.ToString()];
                    c.QueryParameters.Limit = limit;
                    c.QueryParameters.StartIndex = startIndex;
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetNextUpAsync(CancellationToken cancellationToken)
    {
        var result = await _jellyfinApiClient.Shows.NextUp.GetAsync(
                c =>
                {
                    c.QueryParameters.UserId = _userId;
                    c.QueryParameters.Limit = 24;
                    c.QueryParameters.Fields = [ItemFields.PrimaryImageAspectRatio.ToString()];
                    c.QueryParameters.ImageTypeLimit = 1;
                    c.QueryParameters.EnableImageTypes = [ImageType.Primary.ToString(), ImageType.Banner.ToString(), ImageType.Thumb.ToString()];
                    c.QueryParameters.EnableTotalRecordCount = false;
                    c.QueryParameters.DisableFirstEpisode = true;
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result?.Items ?? [];
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetContinueWatchingAsync(CancellationToken cancellationToken)
    {
        var result = await _jellyfinApiClient.Users[_userId].Items.Resume.GetAsync(
                c =>
                {
                    c.QueryParameters.Limit = 24;
                    c.QueryParameters.Fields = [ItemFields.PrimaryImageAspectRatio.ToString()];
                    c.QueryParameters.ImageTypeLimit = 1;
                    c.QueryParameters.EnableImageTypes = [ImageType.Primary.ToString(), ImageType.Backdrop.ToString(), ImageType.Thumb.ToString()];
                    c.QueryParameters.EnableTotalRecordCount = false;
                    c.QueryParameters.MediaTypes = ["Video"];
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return result?.Items ?? [];
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<BaseItemDto>> GetRecentlyAddedAsync(Guid libraryId, CancellationToken cancellationToken)
    {
        var items = await _jellyfinApiClient.Users[_userId].Items.Latest.GetAsync(
                c =>
                {
                    c.QueryParameters.Limit = 24;
                    c.QueryParameters.Fields = [ItemFields.PrimaryImageAspectRatio.ToString()];
                    c.QueryParameters.ImageTypeLimit = 1;
                    c.QueryParameters.EnableImageTypes = [ImageType.Primary.ToString(), ImageType.Backdrop.ToString(), ImageType.Thumb.ToString()];
                    c.QueryParameters.ParentId = libraryId;
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return items ?? [];
    }

    /// <inheritdoc />
    public async ValueTask<BaseItemDtoQueryResult?> GetSeasonsAsync(Guid seriesId, CancellationToken cancellationToken)
    {
        return await _jellyfinApiClient.Shows[seriesId].Seasons.GetAsync(
                c =>
                {
                    c.QueryParameters.UserId = _userId;
                    c.QueryParameters.Fields = [ItemFields.PrimaryImageAspectRatio.ToString()];
                    c.QueryParameters.ImageTypeLimit = 1;
                    c.QueryParameters.EnableImageTypes = [ImageType.Primary.ToString(), ImageType.Backdrop.ToString(), ImageType.Thumb.ToString()];
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<BaseItemDtoQueryResult?> GetNextUpAsync(Guid seriesId, CancellationToken cancellationToken)
    {
        return await _jellyfinApiClient.Shows.NextUp.GetAsync(
                c =>
                {
                    c.QueryParameters.UserId = _userId;
                    c.QueryParameters.ParentId = seriesId;
                    c.QueryParameters.Fields = [ItemFields.PrimaryImageAspectRatio.ToString()];
                    c.QueryParameters.ImageTypeLimit = 1;
                    c.QueryParameters.EnableImageTypes = [ImageType.Primary.ToString(), ImageType.Backdrop.ToString(), ImageType.Thumb.ToString()];
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask<BaseItemDtoQueryResult?> GetEpisodesAsync(Guid seriesId, Guid seasonId, CancellationToken cancellationToken)
    {
        return await _jellyfinApiClient.Shows[seriesId].Episodes.GetAsync(
                c =>
                {
                    c.QueryParameters.UserId = _userId;
                    c.QueryParameters.SeasonId = seasonId;
                    c.QueryParameters.Fields = [ItemFields.PrimaryImageAspectRatio.ToString()];
                    c.QueryParameters.ImageTypeLimit = 1;
                    c.QueryParameters.EnableImageTypes = [ImageType.Primary.ToString(), ImageType.Backdrop.ToString(), ImageType.Thumb.ToString()];
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask<DisplayPreferencesModel?> GetDisplayPreferencesAsync(CancellationToken cancellationToken)
    {
        var displayPreferences = await _jellyfinApiClient.DisplayPreferences["usersettings"].GetAsync(
                c =>
                {
                    c.QueryParameters.UserId = _userId;
                    c.QueryParameters.Client = "emby";
                },
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (displayPreferences?.CustomPrefs is null)
        {
            return null;
        }

        var homeSections = new List<DisplayPreferencesModel.HomeSection>();
        foreach (var customPreference in displayPreferences.CustomPrefs.AdditionalData)
        {
            var section = GetHomeSection(customPreference.Key, customPreference.Value.ToString()!);
            if (section is not null)
            {
                homeSections.Add(section.Value);
            }
        }

        return new DisplayPreferencesModel(homeSections);
    }

    /* TODO
    private static BaseItemKind GetViewType(BaseItemDto_CollectionType? collectionType)
    {
        return collectionType switch
        {
            BaseItemDto_CollectionType.Tvshows => BaseItemKind.Series,
            BaseItemDto_CollectionType.Movies => BaseItemKind.Movie,
            BaseItemDto_CollectionType.Books => BaseItemKind.Book,
            BaseItemDto_CollectionType.Music => BaseItemKind.Audio,
            _ => BaseItemKind.Folder
        };
    }
    */

    private static string GetViewType(BaseItemDto_CollectionType? collectionType)
    {
        return collectionType switch
        {
            BaseItemDto_CollectionType.Tvshows => "Series",
            BaseItemDto_CollectionType.Movies => "Movie",
            BaseItemDto_CollectionType.Books => "Book",
            BaseItemDto_CollectionType.Music => "Audio",
            _ => "Folder"
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
