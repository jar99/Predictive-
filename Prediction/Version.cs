using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Prediction;

public readonly struct Version : ICloneable
{
    public int Major { get; init; }

    public int Minor { get; init; }
    public int Build { get; init; }
    public int Revision { get; init; }

    public string Tag { get; init; }


    public short MajorRevision => (short)(Revision >> 16);

    public short MinorRevision => (short)(Revision & 0xFFFF);


    public Version(int major, int minor, int build, int revision, string? tag = null)
    {
        if (major < 0)
            throw new ArgumentOutOfRangeException(nameof(major), "Argument Out Of Range Version");

        if (minor < 0)
            throw new ArgumentOutOfRangeException(nameof(minor), "Argument Out Of Range Version");

        if (build < 0)
            throw new ArgumentOutOfRangeException(nameof(build), "Argument Out Of Range Version");

        if (revision < 0)
            throw new ArgumentOutOfRangeException(nameof(revision), "Argument Out Of Range Version");

        Major = major;
        Minor = minor;
        Build = build;
        Revision = revision;
        Tag = tag ?? string.Empty;
    }

    public Version(int major, int minor, int build, string? tag = null)
    {
        if (major < 0)
            throw new ArgumentOutOfRangeException(nameof(major), "Argument Out Of Range Version");

        if (minor < 0)
            throw new ArgumentOutOfRangeException(nameof(minor), "Argument Out Of Range Version");

        if (build < 0)
            throw new ArgumentOutOfRangeException(nameof(build), "Argument Out Of Range Version");

        Major = major;
        Minor = minor;
        Build = build;
        Revision = -1;
        Tag = tag ?? string.Empty;
    }

    public Version(int major, int minor, string? tag = null)
    {
        if (major < 0)
            throw new ArgumentOutOfRangeException(nameof(major), "Argument Out Of Range Version");

        if (minor < 0)
            throw new ArgumentOutOfRangeException(nameof(minor), "Argument Out Of Range Version");

        Major = major;
        Minor = minor;
        Build = -1;
        Revision = -1;
        Tag = tag ?? string.Empty;
    }

    public Version(string version)
    {
        var v = Parse(version);
        Major = v.Major;
        Minor = v.Minor;
        Build = v.Build;
        Revision = v.Revision;
        Tag = v.Tag;
    }

    public Version()
    {
        Major = 0;
        Minor = 0;
        Build = -1;
        Revision = -1;
        Tag = string.Empty;
    }

    public Version(Version version)
    {
        Major = version.Major;
        Minor = version.Minor;
        Build = version.Build;
        Revision = version.Revision;
        Tag = version.Tag;
    }


    public object Clone()
    {
        return new Version(this);
    }


    public override int GetHashCode()
    {
        var accumulator = 0;
        accumulator |= (Major & 0x0000000F) << 28;
        accumulator |= (Minor & 0x000000FF) << 20;
        accumulator |= (Build & 0x000000FF) << 12;
        accumulator |= (Revision & 0x00000FFF);
        return accumulator;
    }

    public override string ToString() =>
        ToString(DefaultFormatFieldCount);

    public string ToString(int fieldCount)
    {
        Span<char> dest = stackalloc char[16 + 3 + Tag.Length];
        var success = TryFormat(dest, fieldCount, out var charsWritten);
        Debug.Assert(success);
        return dest[..charsWritten].ToString();
    }


    public bool TryFormat(Span<char> destination, out int charsWritten) =>
        TryFormat(destination, DefaultFormatFieldCount, out charsWritten);

    public bool TryFormat(Span<char> destination, int fieldCount, out int charsWritten)
    {
        switch ((uint)fieldCount)
        {
            case > 4:
                ThrowArgumentException("4");
                break;

            case >= 3 when Build == -1:
                ThrowArgumentException("2");
                break;

            case 4 when Revision == -1:
                ThrowArgumentException("3");
                break;

                static void ThrowArgumentException(string failureUpperBound) =>
                    throw new ArgumentException($"ArgumentOutOfRange Bounds Lower Upper {failureUpperBound}");
        }

        var totalCharsWritten = 0;

        for (var i = 0; i < fieldCount; i++)
        {
            if (i != 0)
            {
                if (destination.IsEmpty)
                {
                    charsWritten = 0;
                    return false;
                }

                destination[0] = '.';
                destination = destination[1..];
                totalCharsWritten++;
            }

            var value = i switch
            {
                0 => Major,
                1 => Minor,
                2 => Build,
                _ => Revision
            };

            if (!((uint)value).TryFormat(destination, out var valueCharsWritten))
            {
                charsWritten = 0;
                return false;
            }

            totalCharsWritten += valueCharsWritten;
            destination = destination[valueCharsWritten..];
        }

        if (!string.IsNullOrWhiteSpace(Tag))
        {
            if (destination.IsEmpty)
            {
                charsWritten = 0;
                return false;
            }

            destination[0] = '-';
            destination = destination[1..];
            totalCharsWritten++;
            if (!Tag.TryCopyTo(destination))
            {
                charsWritten = 0;
                return false;
            }

            totalCharsWritten += Tag.Length;
        }

        charsWritten = totalCharsWritten;
        return true;
    }

    private int DefaultFormatFieldCount =>
        Build == -1 ? 2 :
        Revision == -1 ? 3 :
        4;

    public static Version Parse(string input)
    {
        if (input is null) throw new ArgumentNullException(nameof(input));
        return ParseVersion(input.AsSpan(), throwOnFailure: true) ?? default;
    }

    public static Version Parse(ReadOnlySpan<char> input) =>
        ParseVersion(input, throwOnFailure: true) ?? default;


    public static bool TryParse([NotNullWhen(true)] string? input, [NotNullWhen(true)] out Version? result)
    {
        switch (input)
        {
            case null:
                result = null;
                return false;
            default:
                return (result = ParseVersion(input.AsSpan(), throwOnFailure: false)) != null;
        }
    }

    public static bool TryParse(ReadOnlySpan<char> input, [NotNullWhen(true)] out Version? result) =>
        (result = ParseVersion(input, throwOnFailure: false)) != null;

    private static Version? ParseVersion(ReadOnlySpan<char> input, bool throwOnFailure)
    {
        var majorEnd = input.IndexOf('.');
        if (majorEnd < 0)
        {
            if (throwOnFailure) throw new ArgumentException("Arg Version String", nameof(input));
            return null;
        }

        var buildEnd = -1;
        var minorEnd = input[(majorEnd + 1)..].IndexOf('.');
        if (minorEnd != -1)
        {
            minorEnd += (majorEnd + 1);
            buildEnd = input[(minorEnd + 1)..].IndexOf('.');
            if (buildEnd != -1)
            {
                buildEnd += (minorEnd + 1);
                if (input[(buildEnd + 1)..].Contains('.'))
                {
                    if (throwOnFailure) throw new ArgumentException("Arg Version String", nameof(input));
                    return null;
                }
            }
        }


        var tagStart = input[(minorEnd + 1)..].IndexOf('-');
        var tag = string.Empty;
        if (tagStart < 0) tagStart = input.Length;
        else
        {
            tagStart += (minorEnd + 1);
            tag = input[(tagStart + 1)..].ToString();
        }

        int minor, build, revision;

        if (!TryParseComponent(input[..majorEnd], nameof(input), throwOnFailure, out int major))
        {
            return null;
        }


        if (minorEnd != -1)
        {
            // If there's more than a major and minor, parse the minor, too.
            if (!TryParseComponent(input.Slice(majorEnd + 1, minorEnd - majorEnd - 1), nameof(input), throwOnFailure,
                    out minor))
            {
                return null;
            }

            if (buildEnd != -1)
            {
                // major.minor.build.revision
                return
                    TryParseComponent(input.Slice(minorEnd + 1, buildEnd - minorEnd - 1), nameof(build), throwOnFailure,
                        out build) &&
                    TryParseComponent(input[(buildEnd + 1)..tagStart], nameof(revision), throwOnFailure, out revision)
                        ? new Version(major, minor, build, revision, tag: tag)
                        : null;
            }

            // major.minor.build
            return TryParseComponent(input[(minorEnd + 1)..tagStart], nameof(build), throwOnFailure, out build)
                ? new Version(major, minor, build, tag: tag)
                : null;
        }

        // major.minor
        return TryParseComponent(input[(majorEnd + 1)..tagStart], nameof(input), throwOnFailure, out minor)
            ? new Version(major, minor, tag: tag)
            : null;
    }

    private static bool TryParseComponent(ReadOnlySpan<char> component, string componentName, bool throwOnFailure,
        out int parsedComponent)
    {
        if (throwOnFailure)
        {
            if ((parsedComponent = int.Parse(component, NumberStyles.Integer, CultureInfo.InvariantCulture)) < 0)
            {
                throw new ArgumentOutOfRangeException(componentName, "Argument Out Of Range Version");
            }

            return true;
        }

        return int.TryParse(component, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedComponent) &&
               parsedComponent >= 0;
    }
}

public class VersionJsonConverter : JsonConverter<Version>
{
    public override Version Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
        Version.Parse(reader.GetString()!);

    public override void Write(
        Utf8JsonWriter writer,
        Version version,
        JsonSerializerOptions options) =>
        writer.WriteStringValue(version.ToString());
}