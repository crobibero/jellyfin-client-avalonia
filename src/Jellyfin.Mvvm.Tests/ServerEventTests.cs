using System.Text.Json;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Playback;
using Jellyfin.Mvvm.Preferences;
using Jellyfin.Mvvm.Services;
using Jellyfin.Sdk;
using Microsoft.Kiota.Serialization.Json;

namespace Jellyfin.Mvvm.Tests;

public class ServerEventTests
{
    [Fact]
    public void GetArgument_ReadsStringArgumentsFromServerMessage()
    {
        var command = ReadGeneralCommand("""
            {"MessageType":"GeneralCommand","MessageId":"0a1b2c3d4e5f60718293a4b5c6d7e8f9","Data":{"Name":"DisplayMessage","ControllingUserId":"11111111111111111111111111111111","Arguments":{"Header":"Hello","Text":"From the dashboard","TimeoutMs":"5000"}}}
            """);

        Assert.Equal(GeneralCommand_Name.DisplayMessage, command.Name);
        Assert.Equal("Hello", RemoteControlService.GetArgument(command, "Header"));
        Assert.Equal("From the dashboard", RemoteControlService.GetArgument(command, "Text"));
        Assert.Equal(5000, RemoteControlService.GetIntArgument(command, "TimeoutMs"));
        Assert.Null(RemoteControlService.GetArgument(command, "Missing"));
    }

    [Theory]
    [InlineData("\"35\"", 35)]
    [InlineData("35", 35)]
    [InlineData("\"-1\"", -1)]
    [InlineData("\"loud\"", null)]
    public void GetIntArgument_ParsesNumbers(string value, int? expected)
    {
        var command = ReadGeneralCommand(
            """{"MessageType":"GeneralCommand","Data":{"Name":"SetVolume","Arguments":{"Volume":""" + value + "}}}");

        Assert.Equal(expected, RemoteControlService.GetIntArgument(command, "Volume"));
    }

    [Theory]
    [InlineData("2026-10-12")]
    [InlineData("2026-10-12T10:30:00Z")]
    [InlineData("2026-10-12T10:30:00.5Z")]
    public void GetArgument_PreservesDateLikeStrings(string text)
    {
        var command = ReadGeneralCommand(
            """{"MessageType":"GeneralCommand","Data":{"Name":"DisplayMessage","Arguments":{"Text":""" + "\"" + text + "\"" + "}}}");

        Assert.Equal(text, RemoteControlService.GetArgument(command, "Text"));
    }

    [Theory]
    [InlineData(null, 0)]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    [InlineData(10, 2)]
    public void GetStartIndex_MapsRequestedIndexPastMissingItems(int? startIndex, int expected)
    {
        Guid a = Guid.NewGuid(), b = Guid.NewGuid(), c = Guid.NewGuid(), d = Guid.NewGuid();
        BaseItemDto[] items = [new() { Id = a }, new() { Id = c }, new() { Id = d }];

        Assert.Equal(expected, RemoteControlService.GetStartIndex([a, b, c, d], startIndex, items));
    }

    [Fact]
    public void SupportedCommands_MatchHandledCommandNames()
    {
        // Capabilities use GeneralCommandType, received commands use GeneralCommand_Name.
        foreach (var command in RemoteControlService.SupportedCommands)
        {
            Assert.True(Enum.TryParse<GeneralCommand_Name>(command.ToString(), out _), command.ToString());
        }

        Assert.Equal(RemoteControlService.SupportedCommands.Count, RemoteControlService.SupportedCommands.Distinct().Count());
    }

    [Fact]
    public void UpdateUserData_UpdatesMatchingItemCards()
    {
        var watchedId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var cardFactory = new CardFactory(CreateImageUrlService(), new TestPreferencesService());
        var cards = cardFactory.Create(
        [
            new BaseItemDto { Id = watchedId, Type = BaseItemDto_Type.Movie, MediaType = BaseItemDto_MediaType.Video },
            new BaseItemDto { Id = otherId, Type = BaseItemDto_Type.Movie, MediaType = BaseItemDto_MediaType.Video },
        ]);

        cardFactory.UpdateUserData(cards, new UserDataChangeInfo
        {
            UserDataList =
            [
                new UserItemDataDto { ItemId = watchedId, Played = true, IsFavorite = true, PlayedPercentage = 50 },
                new UserItemDataDto { ItemId = null, Played = true },
            ],
        });

        Assert.True(cards[0].IsPlayed);
        Assert.True(cards[0].IsFavorite);
        Assert.Equal(0.5, cards[0].Progress);
        Assert.True(cards[0].Item!.UserData!.Played);
        Assert.False(cards[1].IsPlayed);
        Assert.Null(cards[1].Item!.UserData);
    }

    [Fact]
    public void UpdateUserData_SkipsChapterCards()
    {
        var itemId = Guid.NewGuid();
        var cardFactory = new CardFactory(CreateImageUrlService(), new TestPreferencesService());
        var chapter = new ItemCardModel { Id = itemId, Kind = CardKind.Chapter };

        cardFactory.UpdateUserData([chapter], new UserDataChangeInfo
        {
            UserDataList = [new UserItemDataDto { ItemId = itemId, Played = true }],
        });

        Assert.False(chapter.IsPlayed);
    }

    private static GeneralCommand ReadGeneralCommand(string json)
    {
        using var document = JsonDocument.Parse(json);
        var message = new JsonParseNode(document.RootElement.Clone()).GetObjectValue(GeneralCommandMessage.CreateFromDiscriminatorValue);
        return message?.Data ?? throw new InvalidOperationException("The message has no data");
    }

    private static ImageUrlService CreateImageUrlService()
    {
        var settings = new JellyfinSdkSettings();
        settings.SetServerUrl("http://localhost:8096");
        return new ImageUrlService(settings);
    }

    private sealed class TestPreferencesService : IPreferencesService
    {
        public event EventHandler? PreferencesChanged
        {
            add { }
            remove { }
        }

        public PlaybackPreferences Playback { get; } = new();

        public UserInterfacePreferences UserInterface { get; } = new();

        public ValueTask LoadAsync() => ValueTask.CompletedTask;

        public ValueTask SetPlaybackAsync(PlaybackPreferences preferences) => ValueTask.CompletedTask;

        public ValueTask SetUserInterfaceAsync(UserInterfacePreferences preferences) => ValueTask.CompletedTask;
    }
}
