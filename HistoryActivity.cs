using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;

namespace Expense_Manager
{
    [Activity(Label = "History")]
    public class HistoryActivity : Activity
    {
        private List<Expense> _expenses;

        private ListView _listView;

        private HistoryItemAdapter _adapter;

        private double _removedAmount;



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_history);

            _expenses = JsonConvert.DeserializeObject<List<Expense>>(Intent.GetStringExtra("Expenses"));
            _expenses = _expenses.OrderByDescending(exp => exp.Date).ToList();

            _listView = FindViewById<ListView>(Resource.Id.listView);

            _adapter = new HistoryItemAdapter(this, _expenses);
            _listView.Adapter = _adapter;
            _listView.FastScrollEnabled = true;
            _listView.ItemLongClick += ListView_ItemLongClick;
        }

        private void ListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            var dialog = new AlertDialog.Builder(this);
            dialog.SetTitle("Remove");
            dialog.SetMessage("Do you want to remove this item?");
            dialog.SetPositiveButton("Yes", async delegate
            {
                _removedAmount += _adapter[e.Position].Amount;
                _adapter.RemoveAt(e.Position);
                _adapter.NotifyDataSetChanged();
                await FileWorker.RewriteFile(_expenses, MainActivity.PathToFile);
            });
            dialog.SetNegativeButton("No", delegate
            {
                dialog.Dispose();
            });
            dialog.Show();
        }

        public override void OnBackPressed()
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.PutExtra("Removed amount", _removedAmount);
            intent.PutExtra("Expenses", JsonConvert.SerializeObject(_expenses));
            SetResult(Result.Ok, intent);
            Finish();
        }
    }
}