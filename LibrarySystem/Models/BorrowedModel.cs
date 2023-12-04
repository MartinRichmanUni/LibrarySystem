namespace LibrarySystem.Models
{
    public class Borrowed
    {
        public int BorrowedID { get; set; }

        public DateOnly BorrowedDate { get; set; }

        public DateOnly DueDate { get; set; }

        public DateOnly ReturnedDate { get; set; }

        public string MemberID { get; set; }

        public int BookID { get; set; }
        
    }
}