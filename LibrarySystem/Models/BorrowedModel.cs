namespace LibrarySystem.Models
{
    public class Borrowed
    {
        public int BorrowedID { get; set; }

        public DateTime BorrowedDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime ReturnedDate { get; set; }

        public string MemberID { get; set; }

        public int BookID { get; set; }
        
    }
}