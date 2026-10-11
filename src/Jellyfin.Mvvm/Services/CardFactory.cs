using Jellyfin.Mvvm.Models;

namespace Jellyfin.Mvvm.Services;

/// <summary>
/// Creates item cards.
/// </summary>
public class CardFactory
{
    private readonly IImageUrlService _imageUrlService;
    private readonly IPreferencesService _preferencesService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CardFactory"/> class.
    /// </summary>
    /// <param name="imageUrlService">The image url service.</param>
    /// <param name="preferencesService">The preferences service.</param>
    public CardFactory(IImageUrlService imageUrlService, IPreferencesService preferencesService)
    {
        _imageUrlService = imageUrlService;
        _preferencesService = preferencesService;
    }

    /// <summary>
    /// Gets the image url service.
    /// </summary>
    public IImageUrlService ImageUrls => _imageUrlService;

    /// <summary>
    /// Gets the default card shape for an item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The card shape.</returns>
    public static CardShape GetDefaultShape(BaseItemDto item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return item.Type switch
        {
            BaseItemDto_Type.Episode or BaseItemDto_Type.CollectionFolder or BaseItemDto_Type.UserView
                or BaseItemDto_Type.Video or BaseItemDto_Type.MusicVideo or BaseItemDto_Type.Photo
                or BaseItemDto_Type.TvChannel or BaseItemDto_Type.Program => CardShape.Thumb,
            BaseItemDto_Type.Audio or BaseItemDto_Type.MusicAlbum or BaseItemDto_Type.MusicArtist
                or BaseItemDto_Type.Playlist or BaseItemDto_Type.AudioBook => CardShape.Square,
            _ => CardShape.Poster
        };
    }

    /// <summary>
    /// Gets the card shape for items in a library.
    /// </summary>
    /// <param name="collectionType">The library collection type.</param>
    /// <returns>The card shape.</returns>
    public static CardShape GetLibraryShape(BaseItemDto_CollectionType? collectionType) => collectionType switch
    {
        BaseItemDto_CollectionType.Music or BaseItemDto_CollectionType.Playlists => CardShape.Square,
        BaseItemDto_CollectionType.Homevideos or BaseItemDto_CollectionType.Photos or BaseItemDto_CollectionType.Musicvideos => CardShape.Thumb,
        _ => CardShape.Poster
    };

    /// <summary>
    /// Create a card.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="shape">The card shape, or <c>null</c> for the default.</param>
    /// <param name="titleStyle">The title style.</param>
    /// <returns>The card.</returns>
    public ItemCardModel Create(BaseItemDto item, CardShape? shape = null, CardTitleStyle titleStyle = CardTitleStyle.Default)
        => ItemCardModel.FromItem(item, shape ?? GetDefaultShape(item), _imageUrlService, _preferencesService.UserInterface, titleStyle);

    /// <summary>
    /// Create cards.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <param name="shape">The card shape, or <c>null</c> for the default.</param>
    /// <param name="titleStyle">The title style.</param>
    /// <returns>The cards.</returns>
    public IReadOnlyList<ItemCardModel> Create(IEnumerable<BaseItemDto> items, CardShape? shape = null, CardTitleStyle titleStyle = CardTitleStyle.Default)
        => items.Select(i => Create(i, shape, titleStyle)).ToList();

    /// <summary>
    /// Create a person card.
    /// </summary>
    /// <param name="person">The person.</param>
    /// <returns>The card.</returns>
    public ItemCardModel Create(BaseItemPerson person) => ItemCardModel.FromPerson(person, _imageUrlService);

    /// <summary>
    /// Refresh the user data indicators of a card.
    /// </summary>
    /// <param name="card">The card.</param>
    /// <param name="userData">The user data.</param>
    public void UpdateUserData(ItemCardModel card, UserItemDataDto? userData)
    {
        ArgumentNullException.ThrowIfNull(card);
        card.UpdateUserData(userData, _preferencesService.UserInterface);
    }
}
