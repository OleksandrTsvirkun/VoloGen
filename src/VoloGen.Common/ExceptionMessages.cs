using System.Resources;

namespace VoloGen.Common;

/// <summary>
/// Provides localized exception message templates backed by embedded resources.
/// Messages are resolved at generator compile-time and embedded into generated code.
/// </summary>
public static class ExceptionMessages
{
    /// <summary>
    /// Resource manager that loads strings from the <c>VoloGen.Common.Diagnostics</c> embedded resource.
    /// </summary>
    private static readonly ResourceManager _resourceManager =
        new("VoloGen.Common.Diagnostics", typeof(ExceptionMessages).Assembly);

    /// <summary>
    /// Fallback message used when the localized resource string cannot be loaded.
    /// </summary>
    private const string InvalidFormatFallback = "Input string was not in a correct format for type '{0}'.";

    /// <summary>
    /// Returns a localized "invalid format" message with the specified type name substituted.
    /// </summary>
    /// <param name="typeName">The display name of the type that failed to parse.</param>
    /// <returns>A formatted error message string suitable for use in <see cref="System.FormatException"/>.</returns>
    public static string GetInvalidFormatMessage(string typeName)
    {
        string format = _resourceManager.GetString("ExceptionMessage_InvalidFormat") ?? InvalidFormatFallback;
        return string.Format(format, typeName);
    }
}

