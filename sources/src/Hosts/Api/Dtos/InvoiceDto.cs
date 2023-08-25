namespace Api.Dtos
{
    public class InvoiceDto
    {
        public int Id { get; set; }

        public decimal? Sum { get; set; }

        public string Status { get; set; }

        public string Hash { get; set; }

        public string PaymentLink { get; set; }
    }
}
