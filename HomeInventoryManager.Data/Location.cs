using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeInventoryManager.Data
{
    public class Location
    {
        public int id { get; set; } = 0; // unique id
        public string user_id { get; set; } = "DEFAULT"; // user id
        public string name { get; set; } = string.Empty; // name of the location
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
