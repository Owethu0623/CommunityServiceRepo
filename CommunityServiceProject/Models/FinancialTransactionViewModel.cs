using System;
using System.Collections.Generic;

namespace CommunityServiceProject.ViewModels
{
    public class FinancialTransactionRowViewModel
    {
        public string Reference { get; set; }

        public string Type { get; set; }

        public DateTime TransactionDate { get; set; }

        public string CitizenName { get; set; }

        public string InvoiceNumber { get; set; }

        public string Status { get; set; }

        public decimal Amount { get; set; }

        public string PaymentMethod { get; set; }

        public string Description { get; set; }
    }

    public class FinancialTransactionsViewModel
    {
        public string SearchTerm { get; set; }

        public string TransactionType { get; set; }

        public string StatusFilter { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public List<FinancialTransactionRowViewModel> Transactions { get; set; }

        public int TotalTransactions { get; set; }

        public decimal TotalPayments { get; set; }

        public decimal TotalRefunds { get; set; }

        public FinancialTransactionsViewModel()
        {
            Transactions =
                new List<FinancialTransactionRowViewModel>();
        }
    }
}