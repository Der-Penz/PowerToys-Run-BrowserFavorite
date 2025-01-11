// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Text.Json;
using Community.PowerToys.Run.Plugin.BrowserFavorite.Models;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.BrowserFavorite.Helpers;

public abstract class ChromiumFavoriteProvider : IFavoriteProvider
{
    private readonly FileSystemWatcher _watcher;
    private readonly string _bookmarkPath;
    private FavoriteItem _root;

    public FavoriteItem Root => _root;

    protected ChromiumFavoriteProvider(string bookmarkPath)
    {
        _root = new FavoriteItem();
        _bookmarkPath = bookmarkPath;
        InitFavorites();

        _watcher = new FileSystemWatcher
        {
            Path = Path.GetDirectoryName(_bookmarkPath)!,
            Filter = Path.GetFileName(_bookmarkPath),
            NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite,
        };

        _watcher.Changed += (s, e) => InitFavorites();
        _watcher.EnableRaisingEvents = true;
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }

    private void InitFavorites()
    {
        if (!Path.Exists(_bookmarkPath))
        {
            Log.Warn($"Failed to find bookmarks file {_bookmarkPath}", typeof(ChromiumFavoriteProvider));
            return;
        }

        using var fs = new FileStream(_bookmarkPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var sr = new StreamReader(fs);
        string json = sr.ReadToEnd();

        JsonDocument parsed;
        try
        {
            parsed = JsonDocument.Parse(json);
        }
        catch (Exception ex)
        {
            Log.Exception("Failed to parse bookmarks file, it might be edited in this moment", ex, typeof(ChromiumFavoriteProvider));
            return;
        }

        parsed.RootElement.TryGetProperty("roots", out var rootElement);
        if (rootElement.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        var newRoot = new FavoriteItem();
        rootElement.TryGetProperty("bookmark_bar", out var bookmarkBarElement);
        if (bookmarkBarElement.ValueKind == JsonValueKind.Object)
        {
            ProcessFavorites(bookmarkBarElement, newRoot, string.Empty, true);
        }

        rootElement.TryGetProperty("other", out var otherElement);
        if (otherElement.ValueKind == JsonValueKind.Object)
        {
            ProcessFavorites(otherElement, newRoot, string.Empty, newRoot.Childrens.Count == 0);
        }

        _root = newRoot;
    }

    private void ProcessFavorites(JsonElement element, FavoriteItem parent, string path, bool root)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("children", out var children))
        {
            var name = element.GetProperty("name").GetString();
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (!root)
                {
                    path += $"{(string.IsNullOrWhiteSpace(path) ? string.Empty : "/")}{name}";
                }

                var folder = new FavoriteItem(name, null, path, FavoriteType.Folder);

                if (root)
                {
                    folder = parent;
                }
                else
                {
                    parent.AddChildren(folder);
                }

                if (children.ValueKind == JsonValueKind.Array)
                {
                    foreach (var child in children.EnumerateArray())
                    {
                        ProcessFavorites(child, folder, path, false);
                    }
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("url", out var url))
        {
            var name = element.GetProperty("name").GetString();
            try
            {
                path += $"{(string.IsNullOrWhiteSpace(path) ? string.Empty : "/")}{name}";
                var favorite = new FavoriteItem(name ?? string.Empty, new Uri(url.GetString()!), path, FavoriteType.Url);
                parent.AddChildren(favorite);
            }
            catch (Exception ex)
            {
                Log.Exception("Failed to create Favourite item", ex, typeof(FavoriteItem));
            }
        }
    }
}
