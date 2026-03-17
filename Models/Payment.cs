using System;

namespace Vibra_DesktopApp.Models
{
    public class Payment
    {
        public int? id { get; set; }
        public DateTime? created_at { get; set; }

        public int? status { get; set; } // 1 fail, 2 success (based on your Vue)

        public int? playlist_id { get; set; }
        public int? song_id { get; set; }

        public Album? playlist { get; set; }
        public Song? song { get; set; }
    }
}