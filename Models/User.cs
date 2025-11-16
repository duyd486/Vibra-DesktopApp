using System;
using System.Collections.Generic;
using System.Text;

namespace Vibra_DesktopApp.Models
{
    public class User
    {
        public int? id { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public string? email { get; set; }
        public string? gender { get; set; }
        public DateTime? birth { get; set; }
        public DateTime? email_verified_at { get; set; }
        public string? avatar { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int? followers { get; set; }
        public string? token { get; set; }
        public string? avatar_path { get; set; }
    }
}
