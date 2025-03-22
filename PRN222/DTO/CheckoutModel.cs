namespace PRN222.DTO
{
    public class CheckoutModel
    {
        public List<BookDto> BorrowBooks { get; set; }
        public int CardId { get; set; }
        public int borrowId { get; set; }
        public string ValidFrom { get; set; }
        public string ValidThru { get; set; }
    }

}
