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

namespace WindowsTooling.AppxManifest.FileReader;

public interface IAppxFileReader : IDisposable
{
    Stream GetFile(string filePath);

    bool FileExists(string filePath);

    bool DirectoryExists(string filePath);

    IAsyncEnumerable<string> EnumerateDirectories(string? rootRelativePath = null, CancellationToken cancellationToken = default);

    IAsyncEnumerable<AppxFileInfo> EnumerateFiles(string rootRelativePath, string wildcard, SearchOption searchOption = SearchOption.TopDirectoryOnly, CancellationToken cancellationToken = default);

    IAsyncEnumerable<AppxFileInfo> EnumerateFiles(string? rootRelativePath = null, CancellationToken cancellationToken = default);

    Stream? GetResource(string resourceFilePath);
}