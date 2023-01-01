namespace skladMVC.Models
{
    public class Item
    {
        public int Id { get; set; } // id
        public string Name { get; set; }
        public string Flag { get; set; }
        public int MaterialId { get; set; } // id материала
        public float Cost { get; set; }
        // высота, ширина
        // высота <= ширина
        public int Height { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
        public int CatalogId { get; set; }
        public int Quality { get; set; } // сорт: 0, 1, 2, 3
        public int Quantity { get; set; }
        public string Logo { get; set; }
        public string Description { get; set; }
    }
}
