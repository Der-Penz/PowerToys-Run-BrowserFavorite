// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Community.PowerToys.Run.Plugin.BrowserFavorite.Models;
using Microsoft.Data.Sqlite;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.BrowserFavorite.Helpers;

public class FireFoxFavoriteProvider : IFavoriteProvider
{
    private const string FolderType = "2";
    private const string RootGuid = "root________";

    private readonly FileSystemWatcher _watcher;
    private FavoriteItem _root;

    public FavoriteItem Root => _root;

    public FireFoxFavoriteProvider()
    {
        var bookMarkPath = GetBookmarkPath();
        _root = new FavoriteItem();
        InitFavorites(bookMarkPath);

        _watcher = new FileSystemWatcher
        {
            Path = Path.GetDirectoryName(bookMarkPath)!,
            Filter = Path.GetFileName(bookMarkPath),
            NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite,
        };

        _watcher.Changed += (_, _) => InitFavorites(bookMarkPath);
        _watcher.EnableRaisingEvents = true;
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }

    private static string GetBookmarkPath()
    {
        string profilesPath = Environment.ExpandEnvironmentVariables(@"%APPDATA%\Mozilla\Firefox\Profiles");

        if (Directory.Exists(profilesPath))
        {
            var defaultReleaseFolder = Directory.GetDirectories(profilesPath, "*default-release*")
                .FirstOrDefault();

            if (defaultReleaseFolder != null)
            {
                return Path.Combine(defaultReleaseFolder, "places.sqlite");
            }
        }

        throw new DirectoryNotFoundException("No default-release folder found in Firefox profiles.");
    }

    private void InitFavorites(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return;
        }

        using var db = new SqliteConnection($"Filename={filePath}");
        db.Open();

        var statement = @"
                        SELECT moz_bookmarks.title, moz_bookmarks.id, moz_bookmarks.type, moz_bookmarks.parent, moz_bookmarks.guid, moz_places.url 
                        FROM moz_bookmarks
                        LEFT JOIN moz_places ON moz_bookmarks.fk = moz_places.id order by type desc";
        var command = db.CreateCommand();
        command.CommandText = statement;
        var reader = command.ExecuteReader();
        BookMark? rootBookmark = null;
        Dictionary<int, BookMark> favorites = new();
        while (reader.Read())
        {
            var name = !reader.IsDBNull(0) ? reader.GetString(0) : string.Empty;
            var id = reader.GetString(1);
            var type = reader.GetString(2);
            var parent = int.Parse(reader.GetString(3));
            var guid = reader.GetString(4);
            var url = !reader.IsDBNull(5) ? reader.GetString(5) : string.Empty;

            var isFolder = type.Equals(FolderType);

            var bookmark = new BookMark
            {
                Name = name,
                Type = isFolder ? FavoriteType.Folder : FavoriteType.Url,
                Url = url,
                Children = new List<BookMark>(),
            };

            if (guid.Equals(RootGuid))
            {
                rootBookmark = bookmark;
            }

            if (parent != 0 && favorites.TryGetValue(parent, out var parentBookmark))
            {
                parentBookmark.Children.Add(bookmark);
            }

            favorites.Add(int.Parse(id), bookmark);
        }

        reader.Close();
        if (rootBookmark == null)
        {
            throw new NullReferenceException("Failed to find root Element in the Table");
        }

        var newRoot = new FavoriteItem();
        ProcessFavorites(rootBookmark, newRoot, string.Empty, true);

        _root = newRoot;
    }

    private void ProcessFavorites(BookMark element, FavoriteItem parent, string path, bool root)
    {
        if (element.Type == FavoriteType.Folder)
        {
            // special case toolbar
            if (element.Name.Equals("toolbar"))
            {
                foreach (var child in element.Children)
                {
                    ProcessFavorites(child, parent, path, false);
                }

                return;
            }

            if (!root)
            {
                path +=
                    $"{(string.IsNullOrWhiteSpace(path) ? string.Empty : "/")}{(string.IsNullOrWhiteSpace(element.Name) ? " " : element.Name)}";
            }

            var folder = new FavoriteItem(element.Name, null, path, FavoriteType.Folder);
            if (root)
            {
                folder = parent;
            }
            else
            {
                parent.AddChildren(folder);
            }

            foreach (var child in element.Children)
            {
                ProcessFavorites(child, folder, path, false);
            }
        }
        else
        {
            try
            {
                path += $"{(string.IsNullOrWhiteSpace(path) ? string.Empty : "/")}{element.Name}";
                var favorite = new FavoriteItem(element.Name, new Uri(element.Url), path, FavoriteType.Url);
                parent.AddChildren(favorite);
            }
            catch (Exception ex)
            {
                Log.Exception("Failed to create Favourite item", ex, typeof(FavoriteItem));
            }
        }
    }

    private class BookMark
    {
        public required string Name { get; init; }

        public FavoriteType Type { get; init; }

        public required string Url { get; init; }

        public required List<BookMark> Children { get; init; }
    }
}
