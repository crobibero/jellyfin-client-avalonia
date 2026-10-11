using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;
using Jellyfin.Sdk;

namespace Jellyfin.Mvvm.Tests;

public class ImageUrlServiceTests
{
    private static readonly Guid _itemId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid _seriesId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Theory]
    [InlineData(CardShape.Poster)]
    [InlineData(CardShape.Thumb)]
    [InlineData(CardShape.Square)]
    public void GetCardImage_WithoutTags_ReturnsNull(CardShape shape)
    {
        var item = new BaseItemDto
        {
            Id = _itemId,
            Type = BaseItemDto_Type.Episode,
            SeriesId = _seriesId,
            ParentThumbItemId = _seriesId,
            ParentBackdropItemId = _seriesId,
            AlbumId = _seriesId,
            BackdropImageTags = [],
            ParentBackdropImageTags = [],
        };

        Assert.Null(CreateService().GetCardImage(item, shape, 360));
    }

    [Theory]
    [InlineData(CardShape.Poster)]
    [InlineData(CardShape.Thumb)]
    [InlineData(CardShape.Square)]
    public void GetCardImage_WithEmptyTags_ReturnsNull(CardShape shape)
    {
        var item = new BaseItemDto
        {
            Id = _itemId,
            Type = BaseItemDto_Type.Episode,
            ImageTags = CreateTags(("Primary", string.Empty), ("Thumb", string.Empty)),
            SeriesId = _seriesId,
            SeriesPrimaryImageTag = string.Empty,
            SeriesThumbImageTag = string.Empty,
            BackdropImageTags = [string.Empty],
        };

        Assert.Null(CreateService().GetCardImage(item, shape, 360));
    }

    [Fact]
    public void GetCardImage_SkipsEmptyTagsAndFallsBack()
    {
        var item = new BaseItemDto
        {
            Id = _itemId,
            Type = BaseItemDto_Type.Episode,
            SeriesId = _seriesId,
            SeriesThumbImageTag = string.Empty,
            ImageTags = CreateTags(("Primary", "episode")),
        };

        var image = CreateService().GetCardImage(item, CardShape.Thumb, 360);

        Assert.NotNull(image);
        Assert.Contains($"/Items/{_itemId:N}/Images/Primary", image.Url, StringComparison.Ordinal);
        Assert.Contains("tag=episode", image.Url, StringComparison.Ordinal);
    }

    [Fact]
    public void ImageUrls_WithoutTags_ReturnNull()
    {
        var service = CreateService();
        var item = new BaseItemDto { Id = _itemId, ParentLogoItemId = _seriesId, ParentBackdropItemId = _seriesId };

        Assert.Null(service.GetPrimaryImage(item, 360));
        Assert.Null(service.GetLogoUrl(item));
        Assert.Null(service.GetBackdropUrl(item));
        Assert.Null(service.GetPersonImage(new BaseItemPerson { Id = _itemId }, 360));
        Assert.Null(service.GetChapterImageUrl(_itemId, 0, null, 480));
        Assert.Null(service.GetUserImageUrl(_itemId, string.Empty));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetImageUrl_WithoutTag_ReturnsNull(string? tag)
    {
        Assert.Null(CreateService().GetImageUrl(_itemId, ImageType.Primary, tag, maxHeight: 360));
    }

    [Fact]
    public void GetImageUrl_WithoutItemId_ReturnsNull()
    {
        Assert.Null(CreateService().GetImageUrl(Guid.Empty, ImageType.Primary, "tag", maxHeight: 360));
    }

    [Fact]
    public void GetCardImage_ReturnsBlurHashForSelectedImage()
    {
        var item = new BaseItemDto
        {
            Id = _itemId,
            Type = BaseItemDto_Type.Movie,
            ImageTags = CreateTags(("Primary", "primary"), ("Thumb", "thumb")),
            ImageBlurHashes = new BaseItemDto_ImageBlurHashes
            {
                Primary = new BaseItemDto_ImageBlurHashes_Primary { AdditionalData = new Dictionary<string, object> { ["primary"] = "primary-hash" } },
                Thumb = new BaseItemDto_ImageBlurHashes_Thumb { AdditionalData = new Dictionary<string, object> { ["thumb"] = "thumb-hash" } },
            },
        };

        var service = CreateService();

        Assert.Equal("primary-hash", service.GetCardImage(item, CardShape.Poster, 360)?.BlurHash);
        Assert.Equal("thumb-hash", service.GetCardImage(item, CardShape.Thumb, 360)?.BlurHash);
    }

    [Fact]
    public void GetCardImage_ReturnsBlurHashForInheritedImage()
    {
        var item = new BaseItemDto
        {
            Id = _itemId,
            Type = BaseItemDto_Type.Episode,
            SeriesId = _seriesId,
            SeriesPrimaryImageTag = "series",
            ImageTags = CreateTags(("Primary", "episode")),
            ImageBlurHashes = new BaseItemDto_ImageBlurHashes
            {
                Primary = new BaseItemDto_ImageBlurHashes_Primary
                {
                    AdditionalData = new Dictionary<string, object> { ["episode"] = "episode-hash", ["series"] = "series-hash" },
                },
            },
        };

        var image = CreateService().GetCardImage(item, CardShape.Poster, 360);

        Assert.NotNull(image);
        Assert.Contains($"/Items/{_seriesId:N}/Images/Primary", image.Url, StringComparison.Ordinal);
        Assert.Equal("series-hash", image.BlurHash);
    }

    [Fact]
    public void GetCardImage_WithoutBlurHash_ReturnsImage()
    {
        var item = new BaseItemDto { Id = _itemId, Type = BaseItemDto_Type.Movie, ImageTags = CreateTags(("Primary", "primary")) };

        var image = CreateService().GetCardImage(item, CardShape.Poster, 360);

        Assert.NotNull(image);
        Assert.Null(image.BlurHash);
    }

    [Fact]
    public void GetPersonImage_ReturnsBlurHash()
    {
        var person = new BaseItemPerson
        {
            Id = _itemId,
            PrimaryImageTag = "person",
            ImageBlurHashes = new BaseItemPerson_ImageBlurHashes
            {
                Primary = new BaseItemPerson_ImageBlurHashes_Primary { AdditionalData = new Dictionary<string, object> { ["person"] = "person-hash" } },
            },
        };

        Assert.Equal("person-hash", CreateService().GetPersonImage(person, 360)?.BlurHash);
    }

    private static ImageUrlService CreateService()
    {
        var settings = new JellyfinSdkSettings();
        settings.SetServerUrl("http://localhost:8096");
        return new ImageUrlService(settings);
    }

    private static BaseItemDto_ImageTags CreateTags(params (string Type, string Tag)[] tags)
        => new() { AdditionalData = tags.ToDictionary(t => t.Type, t => (object)t.Tag) };
}
