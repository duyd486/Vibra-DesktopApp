using System;
using System.Collections.Generic;
using System.Text;

namespace Vibra_DesktopApp.Models
{
    public class Album
    {
        public int? id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public int? author_id { get; set; }
        public string? thumbnail { get; set; }
        public int? type { get; set; }
        public int? total_song { get; set; }
        public int? price { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string? thumbnail_path { get; set; }
        public User? author { get; set; }
    }
}
