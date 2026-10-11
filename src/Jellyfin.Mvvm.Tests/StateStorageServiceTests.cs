using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Services;

namespace Jellyfin.Mvvm.Tests;

public class StateStorageServiceTests
{
    [Fact]
    public async Task AddUserAsync_WithNullToken_ClearsPreviouslyStoredToken()
    {
        var service = new StateStorageService(new InMemorySettingsService());
        var userId = Guid.NewGuid();
        var serverId = Guid.NewGuid();

        await service.AddUserAsync(new UserStateModel { Id = userId, ServerId = serverId, Name = "user", Token = "abc" });
        await service.AddUserAsync(new UserStateModel { Id = userId, ServerId = serverId, Name = "user", Token = null });

        var state = await service.GetStoredStateAsync();
        var stored = Assert.Single(state.Users);
        Assert.Null(stored.Token);
    }

    [Fact]
    public async Task AddUserAsync_ExistingUser_KeepsImageTagWhenNotProvided()
    {
        var service = new StateStorageService(new InMemorySettingsService());
        var userId = Guid.NewGuid();
        var serverId = Guid.NewGuid();

        await service.AddUserAsync(new UserStateModel { Id = userId, ServerId = serverId, Name = "user", Token = "abc", ImageTag = "tag" });
        await service.AddUserAsync(new UserStateModel { Id = userId, ServerId = serverId, Name = "user", Token = "def" });

        var state = await service.GetStoredStateAsync();
        var stored = Assert.Single(state.Users);
        Assert.Equal("def", stored.Token);
        Assert.Equal("tag", stored.ImageTag);
    }

    [Theory]
    [InlineData("tag", "hash")]
    [InlineData(null, "hash")]
    [InlineData("new-tag", null)]
    public async Task AddUserAsync_ExistingUser_KeepsBlurHashOnlyForSameImage(string? imageTag, string? expectedBlurHash)
    {
        var service = new StateStorageService(new InMemorySettingsService());
        var userId = Guid.NewGuid();
        var serverId = Guid.NewGuid();

        await service.AddUserAsync(new UserStateModel { Id = userId, ServerId = serverId, Name = "user", ImageTag = "tag", ImageBlurHash = "hash" });
        await service.AddUserAsync(new UserStateModel { Id = userId, ServerId = serverId, Name = "user", ImageTag = imageTag });

        var state = await service.GetStoredStateAsync();
        var stored = Assert.Single(state.Users);
        Assert.Equal(imageTag ?? "tag", stored.ImageTag);
        Assert.Equal(expectedBlurHash, stored.ImageBlurHash);
    }

    private sealed class InMemorySettingsService : ISettingsService
    {
        private readonly Dictionary<string, string> _values = new(StringComparer.Ordinal);

        public Task<string?> GetAsync(string key)
            => Task.FromResult(_values.TryGetValue(key, out var value) ? value : null);

        public void Remove(string key) => _values.Remove(key);

        public void RemoveAll() => _values.Clear();

        public Task SetAsync(string key, string value)
        {
            _values[key] = value;
            return Task.CompletedTask;
        }
    }
}
