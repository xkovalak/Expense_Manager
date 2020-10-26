using System;

namespace Expense_Manager
{
    public class Expense
    {
        public DateTime Date { get; set; }

        public double Amount { get; set; }

        public Expense(DateTime date, double amount)
        {
            Date = date;
            Amount = amount;
        }
    }
}