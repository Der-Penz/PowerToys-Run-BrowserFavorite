// Copyright (c) Davide Giacometti. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Community.PowerToys.Run.Plugin.BrowserFavorite.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Community.PowerToys.Run.Plugin.BrowserFavorite.Tests
{
    [TestClass]
    public class BrowserSourceTest
    {
        [TestMethod]
        public void Should_Include_Comet_In_Browser_Source_Enum()
        {
            // Act
            var browserSources = Enum.GetValues(typeof(BrowserSource));

            // Assert
            Assert.IsTrue(Array.Exists((BrowserSource[])browserSources, source => source == BrowserSource.Comet));
        }

        [TestMethod]
        public void Should_Instantiate_Comet_Browser_Source_From_Enum()
        {
            // Arrange
            var browserSourceType = BrowserSource.Comet;

            // Act & Assert - Should not throw exception
            IBrowserSource browserSource = browserSourceType switch
            {
                BrowserSource.Brave => new BraveBrowserSource(),
                BrowserSource.Chrome => new ChromeBrowserSource(),
                BrowserSource.Comet => new CometBrowserSource(),
                BrowserSource.Edge => new EdgeBrowserSource(),
                BrowserSource.FireFox => new FireFoxBrowserSource(),
                BrowserSource.WaterFox => new WaterFoxBrowserSource(),
                _ => throw new ArgumentOutOfRangeException(nameof(browserSourceType), browserSourceType, null),
            };

            Assert.IsNotNull(browserSource);
            Assert.IsInstanceOfType(browserSource, typeof(CometBrowserSource));
        }
    }
}
