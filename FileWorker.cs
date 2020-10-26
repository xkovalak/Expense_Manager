using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Environment = System.Environment;

namespace Expense_Manager
{
    public static class FileWorker
    {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);



        public static async void CreateFile(string path)
        {
            await Semaphore.WaitAsync();
            if (!File.Exists(path))
            {
                await using (var _ = File.Create(path)) { }
            }
            Semaphore.Release();
        }

        public static async Task WriteToFileAsync(Expense expense, string path)
        {
            if (!File.Exists(path))
            {
                await using (var _ = File.Create(path)) { }
            }

            await Semaphore.WaitAsync();
            try
            {
                await using var writer = new StreamWriter(path, true);
                await writer.WriteLineAsync(expense.Date.ToString("dd.MM.yyyy") + " " + expense.Amount.ToString("F"));
            }
            finally
            {
                Semaphore.Release();
            }
        }

        public static async Task<Tuple<List<Expense>, double>> LoadFileAsync(string path)
        {
            using var reader = new StreamReader(path);
            var allLines = await reader.ReadToEndAsync();

            var lines = allLines.Split(Environment.NewLine).ToList();

            lines.RemoveRange(lines.Count - 1, 1);

            var expenses = new List<Expense>();
            double balance = 0;

            return await Task.Run(() =>
            {
                foreach (var line in lines.Select(l => l.Split(' ')))
                {
                    try
                    {
                        if (!DateTime.TryParseExact(line[0], "dd.MM.yyyy", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out var date) || !double.TryParse(line[1], out var amount))
                        {
                            return null;
                        }

                        balance += amount;
                        expenses.Add(new Expense(date, amount));
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return null;
                    }
                }

                return new Tuple<List<Expense>, double>(expenses, balance);
            });
        }

        public static async Task RewriteFile(List<Expense> expenses, string path)
        {
            var dataToWrite = new List<string>();
            await Task.Run(() =>
            {
                dataToWrite.AddRange(expenses.Select(expense => expense.Date.ToString("dd.MM.yyyy") + " " + expense.Amount.ToString("F")));
            });

            await Semaphore.WaitAsync();
            try
            {
                await File.WriteAllLinesAsync(path, dataToWrite);
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}