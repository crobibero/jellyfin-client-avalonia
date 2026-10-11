namespace Jellyfin.Mvvm;

/// <summary>
/// The json options.
/// </summary>
public static class JsonOptions
{
    /// <summary>
    /// Gets the json serializer context.
    /// </summary>
    public static MvvmJsonSerializerContext Context => MvvmJsonSerializerContext.Default;
}
