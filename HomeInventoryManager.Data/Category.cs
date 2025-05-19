using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeInventoryManager.Data
{
    public class Category
    {
        public string name { get; set; } = string.Empty;
        public int id { get; set; } = 0;
        public string user_id { get; set; } = "DEFAULT"; // user id
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public ICollection<Item> items { get; set; } = new List<Item>(); //nav
    }
}
