namespace LibrarySystem.Models
{
    public class UserBooks
    {
        public int BookID { get; set; }
        public string BookTitle { get; set; }

        public string Genre { get; set; }

        public string Author { get; set; }

        public DateOnly BorrowedDate { get; set; }

        public DateOnly DueDate { get; set; }

        public DateOnly? ReturnedDate { get; set; }

        public string MemberID { get; set; }
        
    }
}