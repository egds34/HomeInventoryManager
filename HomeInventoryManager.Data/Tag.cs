using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeInventoryManager.Data
{
    public class Tag
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public string user_id { get; set; } = string.Empty;
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
    }
}
