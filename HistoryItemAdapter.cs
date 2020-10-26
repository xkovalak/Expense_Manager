using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;

namespace Expense_Manager
{
    public class HistoryItemAdapter : BaseAdapter<Expense>
    {
        private readonly List<Expense> _items;
        private readonly Activity _context;

        public HistoryItemAdapter(Activity context, List<Expense> items)
        {
            _context = context;
            _items = items;
        }

        public override Expense this[int position] => _items[position];

        public override int Count => _items.Count;

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var row = convertView ?? LayoutInflater.From(_context).Inflate(Resource.Layout.custom_row, null);

            row.FindViewById<TextView>(Resource.Id.textView1).Text = _items[position].Date.ToString("dd.MM.yyyy");
            row.FindViewById<TextView>(Resource.Id.textView2).Text = _items[position].Amount.ToString("F");

            return row;
        }

        public void RemoveAt(int position)
        {
            _items.RemoveAt(position);
        }
    }
}