using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Environment = System.Environment;
using Path = System.IO.Path;

namespace Expense_Manager
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        public static readonly string PathToFile =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "data.txt");

        private double _balance;

        private List<Expense> _expenses;

        private EditText _editTextAmount;
        private EditText _editTextDate;

        private TextView _txtBalance;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _editTextAmount = FindViewById<EditText>(Resource.Id.editTextAmount);
            _editTextDate = FindViewById<EditText>(Resource.Id.editTextDate);
            _txtBalance = FindViewById<TextView>(Resource.Id.balance);

            _editTextDate.Text = DateTime.Today.ToString("dd.MM.yyyy");

            FindViewById<Button>(Resource.Id.buttonHistory).Click += OnHistoryClick;
            FindViewById<Button>(Resource.Id.buttonIncome).Click += OnIncomeClick;
            FindViewById<Button>(Resource.Id.buttonExpense).Click += OnExpenseClick;

            FileWorker.CreateFile(PathToFile);

            var temp = await FileWorker.LoadFileAsync(PathToFile);
            if (temp == null)
            {
                _expenses = new List<Expense>();
                return;
            }
            _expenses = temp.Item1;
            _balance = temp.Item2;
            _txtBalance.Text = _balance.ToString("F");
            CheckBalanceColor();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            //
            //TO DO
            //
            var id = item.ItemId;

            switch (id)
            {
                case Resource.Id.load_file:
                {
                    Toast.MakeText(this, "Coming soon.", ToastLength.Long).Show();
                    return true;
                }
                case Resource.Id.write_to_file:
                {
                    Toast.MakeText(this, "Coming soon.", ToastLength.Long).Show();
                    return true;
                }
            }

            return base.OnOptionsItemSelected(item);
        }

        protected virtual async void OnExpenseClick(object sender, EventArgs e)
        {
            await NewExpense(true);
        }

        protected virtual async void OnIncomeClick(object sender, EventArgs e)
        {
            await NewExpense(false);
        }

        private async Task NewExpense(bool isExpense)
        {
            if (_editTextAmount.Text == "" || !DateTime.TryParseExact(_editTextDate.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                Toast.MakeText(this, "Date or Amount are invalid", ToastLength.Long).Show();
                return;
            }

            var amount = double.Parse(_editTextAmount.Text);

            if (isExpense)
            {
                amount = -amount;
            }

            _balance += amount;
            _txtBalance.Text = _balance.ToString("F");

            CheckBalanceColor();

            _expenses.Add(new Expense(date, amount));
            await FileWorker.WriteToFileAsync(new Expense(date, amount), PathToFile);

            _editTextAmount.Text = "";
        }

        protected virtual void OnHistoryClick(object sender, EventArgs e)
        {
            var historyIntent = new Intent(this, typeof(HistoryActivity));
            historyIntent.PutExtra("Expenses", JsonConvert.SerializeObject(_expenses));
            StartActivityForResult(historyIntent, 0);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode != Result.Ok)
            {
                return;
            }

            var removedAmount = data.GetDoubleExtra("Removed amount", 0);
            _expenses = JsonConvert.DeserializeObject<List<Expense>>(data.GetStringExtra("Expenses"));
            _balance -= removedAmount;
            _txtBalance.Text = _balance.ToString("F");
            CheckBalanceColor();
        }

        private void CheckBalanceColor()
        {
            if (Math.Abs(_balance) < 0.0001)
            {
                _txtBalance.SetTextColor(Color.WhiteSmoke);
            }
            else if (_balance > 0)
            {
                _txtBalance.SetTextColor(Color.Green);
            }
            else
            {
                _txtBalance.SetTextColor(Color.Red);
            }
        }
    }
}
