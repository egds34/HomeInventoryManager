using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeInventoryManager.Data
{
    public class Receipt
    {
        public string id { get; set; } = string.Empty; //unique id
        public string location_purchased = string.Empty; // address/location of where the item was purchased
        public bool was_delivered = false; // true if the item was delivered
        public int Receipt_id { get; set; } = 0; // actual reciept id
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
