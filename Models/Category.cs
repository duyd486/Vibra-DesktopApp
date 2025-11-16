using System;
using System.Collections.Generic;
using System.Text;

namespace Vibra_DesktopApp.Models
{
    public class Category
    {
        public int? id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public string? thumbnail { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string? thumbnail_path { get; set; }
    }
}
