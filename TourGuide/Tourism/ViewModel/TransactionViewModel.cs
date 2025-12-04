using Tourism.Models;

namespace Tourism.ViewModel
{
    public class TransactionViewModel
    {
        public CreditCard cc { get; set; } = new();
        public double balance { get; set; } = 0;
    }
}
