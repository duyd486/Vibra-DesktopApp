using System.Collections.Generic;

namespace Vibra_DesktopApp.Models
{
    public class SearchResult
    {
        public List<Album> albums { get; set; } = [];
        public List<Song> songs { get; set; } = [];
        public List<User> artists { get; set; } = [];
    }
}
