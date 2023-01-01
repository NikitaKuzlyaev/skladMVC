using System.Drawing.Printing;

namespace skladMVC.Models
{
    public class Catalog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public string Logo { get; set; }
    }
}
