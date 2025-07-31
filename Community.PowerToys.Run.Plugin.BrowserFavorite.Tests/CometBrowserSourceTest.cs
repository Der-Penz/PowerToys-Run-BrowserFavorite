// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Community.PowerToys.Run.Plugin.BrowserFavorite.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Community.PowerToys.Run.Plugin.BrowserFavorite.Tests
{
    [TestClass]
    public class CometBrowserSourceTest
    {
        [TestMethod]
        public void Should_Initialize_Comet_Browser_Source()
        {
            // Arrange & Act
            var browserSource = new CometBrowserSource();

            // Assert
            Assert.IsNotNull(browserSource);
            Assert.IsNotNull(browserSource.FavoriteProvider);
            Assert.AreEqual(@"C:\Program Files\Comet\comet.exe", browserSource.DefaultExecutablePath);
            Assert.AreEqual(browserSource.DefaultExecutablePath, browserSource.BrowserExecutable);
        }

        [TestMethod]
        public void Should_Have_Comet_Favorite_Provider()
        {
            // Arrange & Act
            var browserSource = new CometBrowserSource();

            // Assert
            Assert.IsInstanceOfType(browserSource.FavoriteProvider, typeof(CometFavoriteProvider));
        }

        [TestMethod]
        public void Should_Allow_Custom_Browser_Executable_Path()
        {
            // Arrange
            var browserSource = new CometBrowserSource();
            var customPath = @"C:\Custom\Path\comet.exe";

            // Act
            browserSource.BrowserExecutable = customPath;

            // Assert
            Assert.AreEqual(customPath, browserSource.BrowserExecutable);
            Assert.AreNotEqual(browserSource.DefaultExecutablePath, browserSource.BrowserExecutable);
        }
    }
}
