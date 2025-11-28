using System.Collections.Generic;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Themes.FullscreenTheme
{
    public class ThemeInfo : ITheme
    {
        public Theme Theme => new Theme
        {
            Name = "Fullscreen Theme",
            Version = "1.0.0",
            Resources = new List<Resource>()
            {
                new Stylesheet("/themes/Oqtane.Themes.FullscreenTheme/Theme.css")
            }
        };
    }
}
