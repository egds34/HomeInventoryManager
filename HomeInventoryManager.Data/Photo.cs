using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeInventoryManager.Data
{
    public class Photo
    {
        public int id { get; set; } = 0; // unique id
        public string user_id { get; set; } = "DEFAULT"; // user id
        public string url { get; set; } = string.Empty; // image string
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public ICollection<Item> Items { get; set; } = new List<Item>(); //nav, idk search by image?? it may be useful..
    }
}
