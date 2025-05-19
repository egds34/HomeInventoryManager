using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeInventoryManager.Data
{
    public class User
    {
        public int user_id { get; set; } = 0; // unique id
        public string user_name { get; set; } = string.Empty; // name of the user
        public string email { get; set; } = string.Empty;
        public byte[] password_hash { get; set; } = Array.Empty<byte>();
        public byte[] password_salt { get; set; } = Array.Empty<byte>();
        public string? refresh_token { get; set; }
        public DateTime? refresh_token_time { get; set; }
        public string first_name { get; set; } = string.Empty; // name of the user
        public string last_name { get; set; } = string.Empty; // name of the user
        public string role { get; set; } = "Basic";
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime last_login_time { get; set; } = DateTime.UtcNow;
        public DateTime last_login_attempt_time { get; set; } = DateTime.UtcNow;
        public int failed_login_attempts { get; set; } = 0;
        public DateTime lockout_until_time { get; set; }
        public int total_lockouts { get; set; } = 0;
    }
}
