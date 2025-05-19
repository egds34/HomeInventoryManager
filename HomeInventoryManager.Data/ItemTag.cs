using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeInventoryManager.Data
{
    //sets up the many to many relationship.
    public class ItemTag
    {
        public int item_id { get; set; }
        public Item Item { get; set; } = null!;

        public int tag_id { get; set; }
        public Tag Tag { get; set; } = null!;
    }

    }
