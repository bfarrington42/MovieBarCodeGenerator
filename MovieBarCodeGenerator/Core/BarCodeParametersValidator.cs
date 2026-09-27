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

public class BarCodeParametersValidator
{
    public BarCodeParameters GetValidatedParameters(
        string rawInputPath,
        string rawBaseOutputPath,
        string rawBarWidth,
        string rawImageWidth,
        string rawImageHeight,
        bool useInputHeightForOutput,
        Func<IReadOnlyCollection<string>, bool> shouldOverwriteOutputPaths,
        IEnumerable<IBarGenerator> barGenerators,
        string fileNamePostfix = null)
    {
        var inputPath = rawInputPath.Trim(new[] { '"' });

        bool IsDirectoryPath(string path)
            => Directory.Exists(path)
                || path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                || path.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal);

        (string Path, bool AlreadyExists) ValidateOutputPath(string initialPath)
        {
            string path = initialPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                path = $"{GetSafeFileNameWithoutExtension(inputPath)}.png";
            }

            if (!Path.HasExtension(path))
            {
                path += ".png";
            }

            if (path.Any(x => Path.GetInvalidPathChars().Contains(x)))
            {
                throw new ParameterValidationException("The output path is invalid.");
            }

            return (path, File.Exists(path));
        }

        var rawTrimmedOutput = rawBaseOutputPath?.Trim(new[] { '"' });
        string baseOutputCandidate;
        if (string.IsNullOrWhiteSpace(rawTrimmedOutput))
        {
            // Default: next to the input file (falls back to the current
            // working directory when the input has no local directory,
            // e.g. urls or bare file names).
            var defaultFileName = $"{GetSafeFileNameWithoutExtension(inputPath)}.png";
            string inputDir = null;
            try
            {
                inputDir = Path.GetDirectoryName(inputPath);
            }
            catch
            {
                inputDir = null;
            }
            baseOutputCandidate = !string.IsNullOrEmpty(inputDir) && Directory.Exists(inputDir)
                ? Path.Combine(inputDir, defaultFileName)
                : defaultFileName;
        }
        else if (IsDirectoryPath(rawTrimmedOutput))
        {
            // Batch mode: one output per input, named after the input file.
            baseOutputCandidate = Path.Combine(rawTrimmedOutput, $"{GetSafeFileNameWithoutExtension(inputPath)}.png");
        }
        else
        {
            baseOutputCandidate = rawTrimmedOutput;
        }

        var baseOutputPath = ValidateOutputPath(baseOutputCandidate).Path;

        var postfix = fileNamePostfix ?? "";
        if (postfix.Any(x => Path.GetInvalidFileNameChars().Contains(x)))
        {
            throw new ParameterValidationException("The filename postfix is invalid.");
        }

        var generatorList = barGenerators.ToList();
        var outputPaths = from generator in generatorList
                          let modeSuffix = generatorList.Count > 1 ? generator.FileNameSuffix : ""
                          let name = $"{GetSafeFileNameWithoutExtension(baseOutputPath)}{modeSuffix}{postfix}{Path.GetExtension(baseOutputPath)}"
                          let path = Path.Combine(Path.GetDirectoryName(baseOutputPath), name)
                          select new { generator, ValidatedPath = ValidateOutputPath(path) };

        if (outputPaths.Any(x => x.ValidatedPath.AlreadyExists)
            && shouldOverwriteOutputPaths(outputPaths.Where(x => x.ValidatedPath.AlreadyExists).Select(x => x.ValidatedPath.Path).ToList()) == false)
        {
            throw new OperationCanceledException("At least one output file already exists.");
        }

        if (!int.TryParse(rawBarWidth, out var barWidth) || barWidth <= 0)
        {
            throw new ParameterValidationException("Invalid bar width.");
        }

        if (!int.TryParse(rawImageWidth, out var imageWidth) || imageWidth <= 0)
        {
            throw new ParameterValidationException("Invalid output width.");
        }

        int? imageHeight = null;
        if (!useInputHeightForOutput)
        {
            if (int.TryParse(rawImageHeight, out var nonNullableImageHeight) && nonNullableImageHeight > 0)
            {
                imageHeight = nonNullableImageHeight;
            }
            else
            {
                throw new ParameterValidationException("Invalid output height.");
            }
        }

        return new BarCodeParameters
        {
            InputPath = inputPath,
            GeneratorOutputPaths = outputPaths.ToDictionary(x => x.generator, x => x.ValidatedPath.Path),
            BarWidth = barWidth,
            Width = imageWidth,
            Height = imageHeight,
        };
    }

    private string GetSafeFileNameWithoutExtension(string input)
    {
        try
        {
            return Path.GetFileNameWithoutExtension(input);
        }
        catch
        {
            // TODO: this implementation could largely be improved...
            return "output";
        }
    }
}

public class ParameterValidationException : Exception
{
    public ParameterValidationException(string message) : base(message) { }
}
