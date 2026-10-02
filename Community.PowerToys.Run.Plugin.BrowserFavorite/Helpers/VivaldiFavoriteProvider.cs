// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Community.PowerToys.Run.Plugin.BrowserFavorite.Helpers;

public class VivaldiFavoriteProvider : ChromiumFavoriteProvider
{
    private static readonly string Path =
        Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\Vivaldi\User Data\Default\Bookmarks");

    public VivaldiFavoriteProvider()
        : base(Path)
    {
    }
}
