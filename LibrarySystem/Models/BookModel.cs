namespace LibrarySystem.Models
{
    public class Book
    {
        public int BookID { get; set; }
        
        public string BookTitle { get; set; }

        public string Genre { get; set; }

        public string Author { get; set; }

        public int StockAmount { get; set; }
    }
}