//Copyright 2011-2021 Melvyn Laily
//https://zerowidthjoiner.net

//This file is part of MovieBarCodeGenerator.

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;

namespace MovieBarCodeGenerator.Core;

/// <summary>
/// Video file extensions recognized when an input directory is expanded
/// (CLI batch mode and GUI batch mode). Extensions include the leading dot
/// and comparison is case-insensitive.
/// </summary>
public static class SupportedVideoExtensions
{
    public static readonly string[] Default =
    {
        ".mp4", ".m4v", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm",
        ".mpg", ".mpeg", ".ts", ".m2ts", ".vob", ".ogv", ".3gp",
    };

    /// <summary>
    /// Parses a user supplied extension list (e.g. "mp4,mkv" or ".mp4;*.mkv").
    /// Returns null when <paramref name="raw"/> is null/whitespace.
    /// </summary>
    public static HashSet<string> ParseOrNull(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in raw.Split(new[] { ',', ';', '|', ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var cleaned = part.Trim().Trim('"');

            if (cleaned.StartsWith("*."))
            {
                cleaned = cleaned.Substring(1);
            }
            if (!cleaned.StartsWith(".", StringComparison.Ordinal))
            {
                cleaned = "." + cleaned;
            }
            if (cleaned.Length > 1)
            {
                result.Add(cleaned);
            }
        }

        return result;
    }

    public static bool IsSupported(string path, ISet<string> allowed)
    {
        if (allowed == null || string.IsNullOrEmpty(path))
        {
            return false;
        }

        try
        {
            return allowed.Contains(Path.GetExtension(path));
        }
        catch
        {
            return false;
        }
    }
}
