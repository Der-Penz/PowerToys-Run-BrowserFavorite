// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Community.PowerToys.Run.Plugin.BrowserFavorite.Models;
using Microsoft.Data.Sqlite;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.BrowserFavorite.Helpers;

public class FireFoxFavoriteProvider : IFavoriteProvider
{
    private static readonly string FolderType = "2";
    private static readonly string RootGUID = "root________";
    private static readonly string BookmarkPath =
        Environment.ExpandEnvironmentVariables(
            @"%APPDATA%\Mozilla\Firefox\Profiles\c4pnpa26.default-release\places.sqlite");

    private readonly FileSystemWatcher _watcher;
    private FavoriteItem _root;

    public FavoriteItem Root => _root;

    public FireFoxFavoriteProvider()
    {
        Log.Info("Here", typeof(FireFoxFavoriteProvider));
        _root = new FavoriteItem();
        InitFavorites();

        _watcher = new FileSystemWatcher
        {
            Path = Path.GetDirectoryName(BookmarkPath)!,
            Filter = Path.GetFileName(BookmarkPath),
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
        Log.Info("Before", typeof(FireFoxFavoriteProvider));
        if (!File.Exists(BookmarkPath))
        {
            Log.Info(BookmarkPath, typeof(FireFoxFavoriteProvider));
            return;
        }

        Log.Info("After", typeof(FireFoxFavoriteProvider));

        using var db = new SqliteConnection($"Filename={BookmarkPath}");
        db.Open();

        Log.Info("Open", typeof(FireFoxFavoriteProvider));
        var statement = @"
                        SELECT moz_bookmarks.title, moz_bookmarks.id, moz_bookmarks.type, moz_bookmarks.parent, moz_bookmarks.guid, moz_places.url 
                        FROM moz_bookmarks
                        LEFT JOIN moz_places ON moz_bookmarks.fk = moz_places.id order by type desc";
        var command = db.CreateCommand();
        command.CommandText = statement;
        var reader = command.ExecuteReader();
        BookMark? root = null;
        Dictionary<int, (BookMark, int)> favorites = new();
        while (reader.Read())
        {
            var name = reader.GetString(0);
            var id = reader.GetString(1);
            var type = reader.GetString(2);
            var parent = reader.GetString(3);
            var guid = reader.GetString(4);
            var url = !reader.IsDBNull(5) ? reader.GetString(5) : string.Empty;

            var isFolder = type.Equals(FolderType);

            var bookmark = new BookMark
            {
                Id = int.Parse(id),
                Name = name,
                Type = isFolder ? FavoriteType.Folder : FavoriteType.Url,
                Url = url,
                ParentId = int.Parse(parent),
                Children = new List<BookMark>(),
            };

            if (guid.Equals(RootGUID))
            {
                root = bookmark;
            }

            if (string.IsNullOrEmpty(url))
            {
                continue;
            }

            Log.Info($"Found bookmark {name} ({type}) guid {guid} at {url}", typeof(FireFoxFavoriteProvider));
            favorites.Add(int.Parse(id), (bookmark, int.Parse(parent)));
        }

        if (root == null)
        {
            throw new NullReferenceException("Failed to find root Element in the Table");
        }

        foreach (var (key, (bookMark, parentKey)) in favorites)
        {
            if (!favorites.TryGetValue(parentKey, out var favorite))
            {
                continue;
            }

            var (parent, _) = favorite;
            parent.Children.Add(bookMark);
        }

        _root = new FavoriteItem();

        // build the actual FavoriteItems
        foreach (var (key, (bookMark, parentKey)) in favorites)
        {
            var item = new FavoriteItem(bookMark.Name, new Uri(bookMark.Url), string.Empty, bookMark.Type);
            _root.AddChildren(item);
        }

        reader.Close();
    }

    private class BookMark
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public FavoriteType Type { get; set; }

        public required string Url { get; set; }

        public int ParentId { get; set; }

        public required List<BookMark> Children { get; set; }

        public FavoriteItem ToFavoriteItem(BookMark root)
        {
            var path = string.Empty;

            return new FavoriteItem(Name, new Uri(Url), path, FavoriteType.Folder);
        }
    }
}
