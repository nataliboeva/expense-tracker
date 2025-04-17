using System;

namespace Expense_Tracker.Services
{
    public class CurrencyConverter
    {
        private static readonly Dictionary<string, decimal> EuroRates = new()
        {
            { "EUR", 1.0m },
            { "BGN", 1.96m },
            { "USD", 1.09m }  // This is approximate, you might want to update it
        };

        public static decimal ConvertToEuro(decimal amount, string fromCurrency)
        {
            // Handle case where "DOLLAR" is used instead of "USD"
            if (fromCurrency == "DOLLAR")
                fromCurrency = "USD";
                
            if (!EuroRates.ContainsKey(fromCurrency))
                throw new ArgumentException($"Unsupported currency: {fromCurrency}");

            if (fromCurrency == "EUR")
                return amount;

            return amount / EuroRates[fromCurrency];
        }

        public static decimal ConvertFromEuro(decimal amountInEuro, string toCurrency)
        {
            // Handle case where "DOLLAR" is used instead of "USD"
            if (toCurrency == "DOLLAR")
                toCurrency = "USD";
                
            if (!EuroRates.ContainsKey(toCurrency))
                throw new ArgumentException($"Unsupported currency: {toCurrency}");

            if (toCurrency == "EUR")
                return amountInEuro;

            return amountInEuro * EuroRates[toCurrency];
        }

        public static decimal Convert(decimal amount, string fromCurrency, string toCurrency)
        {
            // Handle case where "DOLLAR" is used instead of "USD"
            if (fromCurrency == "DOLLAR")
                fromCurrency = "USD";
                
            if (toCurrency == "DOLLAR")
                toCurrency = "USD";
                
            if (fromCurrency == toCurrency)
                return amount;

            var amountInEuro = ConvertToEuro(amount, fromCurrency);
            return ConvertFromEuro(amountInEuro, toCurrency);
        }
    }
} 