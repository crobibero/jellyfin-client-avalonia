using System.Text.Json.Serialization;
using Jellyfin.Mvvm.Models;
using Jellyfin.Mvvm.Preferences;

namespace Jellyfin.Mvvm;

/// <summary>
/// Json serializer context for Jellyfin.Mvvm.
/// </summary>
[JsonSourceGenerationOptions(UseStringEnumConverter = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(StateContainerModel))]
[JsonSerializable(typeof(PlaybackPreferences))]
[JsonSerializable(typeof(UserInterfacePreferences))]
[JsonSerializable(typeof(ServerDiscoveryResponse))]
public partial class MvvmJsonSerializerContext : JsonSerializerContext
{
}
