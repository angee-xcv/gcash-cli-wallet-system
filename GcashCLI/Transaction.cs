
using System;

namespace GcashCLI
{

    public enum TransactionType
    {
        Deposit,
        Transfer,
        Send,
        Receive,
        CashIn,
        CashOut
    }

    public class Transaction
    {
        public string ReferenceNumber { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal BalanceAfter { get; set; }

        public Transaction(TransactionType type, decimal amount, string description, decimal balanceAfter)
        {
            ReferenceNumber = GenerateReferenceNumber();
            Type = type;
            Amount = amount;
            Description = description;
            Timestamp = DateTime.Now;
            BalanceAfter = balanceAfter;
        }

        private string GenerateReferenceNumber()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            string randomPart = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            return $"GC-{datePart}-{randomPart}";
        }

        public override string ToString()
        {
            string sign = (Type == TransactionType.Deposit ||
                           Type == TransactionType.Receive ||
                           Type == TransactionType.CashIn) ? "+" : "-";

            return $"[{Timestamp:MM/dd/yyyy hh:mm tt}] {Type,-10} {sign}₱{Amount:N2,-12} | Ref#: {ReferenceNumber} | {Description} | Bal: ₱{BalanceAfter:N2}";
        }
    }
}
