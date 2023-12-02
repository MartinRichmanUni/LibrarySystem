namespace LibrarySystem.Models
{
    public class UserBooks
    {
        public int BookID { get; set; }
        public string BookTitle { get; set; }

        public string Genre { get; set; }

        public string Author { get; set; }

        public DateTime BorrowedDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? ReturnedDate { get; set; }
        
    }
}