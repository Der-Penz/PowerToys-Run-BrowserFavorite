// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Wox.Infrastructure;
using Wox.Plugin.Logger;
using Path = System.IO.Path;

namespace Community.PowerToys.Run.Plugin.BraveFavorite.Helpers;

public class FireFoxBrowserSource : IBrowserSource
{
    public FireFoxBrowserSource()
    {
        DefaultExecutablePath = @"C:\Program Files\Mozilla Firefox\firefox.exe";
        BrowserExecutable = DefaultExecutablePath;
        FavoriteProvider = new FireFoxFavoriteProvider();
    }

    public string DefaultExecutablePath { get; }

    public IFavoriteProvider FavoriteProvider { get; }

    public string BrowserExecutable { get; set; }

    public void Open(string url, bool privateMode = false)
    {
        var directory = Path.GetDirectoryName(BrowserExecutable);

        if (directory is null)
        {
            Log.Warn("Browser executable directory could not be found.", typeof(ChromeBrowserSource));
            return;
        }

        var fileName = Path.GetFileName(BrowserExecutable);

        if (privateMode)
        {
            Helper.OpenInShell($@".\{fileName}", workingDir: directory, arguments: $"-private-window={url}");
        }
        else
        {
            Helper.OpenInShell($@".\{fileName}", workingDir: directory, arguments: url);
        }
    }
}
