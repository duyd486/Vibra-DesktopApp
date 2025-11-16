using System;
using System.Collections.Generic;
using System.Text;

namespace Vibra_DesktopApp.Models
{
    public class Song
    {
        public int? id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public string? lyrics { get; set; }
        public string? thumbnail { get; set; }
        public int? total_played { get; set; }
        public int? status { get; set; }
        public int? price { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string? song_path { get; set; }
        public string? lyrics_path { get; set; }
        public string? thumbnail_path { get; set; }
        public List<string>? list_lyric { get; set; }
        public User? author { get; set; }
        public Album? playlist { get; set; }
        public Category? category { get; set; }
    }
}
