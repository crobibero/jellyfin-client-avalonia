using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Sdk;

namespace Jellyfin.Maui.Services
{
    /// <summary>
    /// The library service.
    /// </summary>
    public class LibraryService : ILibraryService
    {
        private readonly IItemsClient _itemsClient;
        private readonly ITvShowsClient _tvShowsClient;
        private readonly IUserLibraryClient _userLibraryClient;
        private readonly IUserViewsClient _userViewsClient;

        private readonly Guid _userId;

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryService"/> class.
        /// </summary>
        /// <param name="itemsClient">Instance of the <see cref="IItemsClient"/> interface.</param>
        /// <param name="stateService">Instance of the <see cref="IStateService"/> interface.</param>
        /// <param name="tvShowsClient">Instance of the <see cref="ITvShowsClient"/> interface.</param>
        /// <param name="userLibraryClient">Instance of the <see cref="IUserLibraryClient"/> interface.</param>
        /// <param name="userViewsClient">Instance of the <see cref="IUserViewsClient"/> interface.</param>
        public LibraryService(
            IItemsClient itemsClient,
            IStateService stateService,
            ITvShowsClient tvShowsClient,
            IUserLibraryClient userLibraryClient,
            IUserViewsClient userViewsClient)
        {
            _itemsClient = itemsClient;
            _tvShowsClient = tvShowsClient;
            _userLibraryClient = userLibraryClient;
            _userViewsClient = userViewsClient;
            _userId = stateService.GetUser().Id;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<BaseItemDto>> GetLibraries()
        {
            var views = await _userViewsClient.GetUserViewsAsync(_userId)
                .ConfigureAwait(false);
            return views is null ? Array.Empty<BaseItemDto>() : views.Items;
        }

        /// <inheritdoc />
        public async Task<BaseItemDto?> GetLibrary(Guid id)
        {
            var result = await _itemsClient.GetItemsAsync(
                    userId: _userId,
                    ids: new[] { id })
                .ConfigureAwait(false);
            return result.Items.Count == 0 ? null : result.Items[0];
        }

        /// <inheritdoc />
        public async Task<BaseItemDtoQueryResult> GetLibraryItems(
            BaseItemDto library,
            int limit,
            int startIndex)
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
                    startIndex: startIndex)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<BaseItemDto>> GetNextUp(IEnumerable<Guid> libraryIds)
        {
            var items = new List<BaseItemDto>();
            foreach (var library in libraryIds)
            {
                var result = await _tvShowsClient.GetNextUpAsync(
                        userId: _userId,
                        limit: 24,
                        fields: new[] { ItemFields.PrimaryImageAspectRatio },
                        imageTypeLimit: 1,
                        enableImageTypes: new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb },
                        parentId: library)
                    .ConfigureAwait(false);
                items.AddRange(result.Items);
            }

            return items;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<BaseItemDto>> GetContinueWatching()
        {
            var result = await _itemsClient.GetResumeItemsAsync(
                    userId: _userId,
                    limit: 24,
                    fields: new[] { ItemFields.PrimaryImageAspectRatio },
                    imageTypeLimit: 1,
                    enableImageTypes: new[] { ImageType.Primary, ImageType.Backdrop, ImageType.Thumb },
                    enableTotalRecordCount: false,
                    mediaTypes: new[] { "Video" })
                .ConfigureAwait(false);

            return result.Items;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<BaseItemDto>> GetRecentlyAdded(Guid libraryId, CancellationToken cancellationToken = default)
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

        private BaseItemKind GetViewType(string collectionType)
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
    }
}