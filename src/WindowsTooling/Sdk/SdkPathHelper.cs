﻿// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

namespace WindowsTooling.Sdk;

public static class SdkPathHelper
{
    public static string GetSdkPath(string localName, string? baseDirectory = null)
    {
        string baseDir = baseDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "redistr", "sdk");
        string path = Path.Combine(baseDir, nint.Size == 4 ? "x86" : "x64", localName);
        if (!File.Exists(path))
        {
            path = Path.Combine(baseDir, localName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(string.Format("Could not locate SDK part {0}.", path), path);
            }
        }

        return path;
    }
}