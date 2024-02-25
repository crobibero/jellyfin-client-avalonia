using System.Text.Json.Serialization;
using Jellyfin.Mvvm.Models;

namespace Jellyfin.Mvvm;

/// <summary>
/// Json serializer context for Jellyfin.Mvvm.
/// </summary>
[JsonSerializable(typeof(StateContainerModel))]
public partial class MvvmJsonSerializerContext : JsonSerializerContext
{
}
