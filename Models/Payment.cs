using System;
using System.Globalization;

namespace Vibra_DesktopApp.Models
{
    public class Payment
    {
        public int? id { get; set; }
        public DateTime? created_at { get; set; }

        public string status { get; set; } = "1"; // 1 fail, 2 success (based on your Vue)

        public int? playlist_id { get; set; }
        public int? song_id { get; set; }

        public Album? playlist { get; set; }
        public Song? song { get; set; }

        public int? amount => playlist_id is not null ? playlist?.price : song?.price;

        public string? amount_text
        {
            get
            {
                if (amount is null)
                    return null;

                // Format like 12.000 vnd (vi-VN)
                var vi = CultureInfo.GetCultureInfo("vi-VN");
                return string.Format(vi, "{0:N0} vnd", amount);
            }
        }

        public string? item_name => playlist_id is not null ? playlist?.name : song?.name;

        public string package_name => playlist_id is not null ? "Album" : "Bài hát";

        public bool is_success => status == "2";
        public bool is_failed => status == "1";

        public string status_text => status switch
        {
            "2" => "Thành công",
            "1" => "Thất bại",
            _ => "Không xác định",
        };
    }
}