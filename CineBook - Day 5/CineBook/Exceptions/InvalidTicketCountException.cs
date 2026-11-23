namespace CineBook.Exceptions
{
    public class InvalidTicketCountException : Exception
    {
        public InvalidTicketCountException(int totalTickets, int seatCount)
            : base($"Jumlah tiket ({totalTickets}) tidak sama dengan jumlah kursi yang dipilih ({seatCount}).")
        {
        }
    }
}
