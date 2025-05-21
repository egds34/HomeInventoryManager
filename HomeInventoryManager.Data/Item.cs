namespace HomeInventoryManager.Data
{
    public class Item
    {
        public long id { get; set; } = 0;
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public int user_id { get; set; } = 0;
        public string barcode { get; set; } = string.Empty;
        public int category_id { get; set; } = 0;
        public int location_id { get; set; } = 0; // location of the item
        public int photo_id { get; set; } = 0; // photo of the item
        public int receipt_id { get; set; } = 0; // receipt of the item
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
    }
}
