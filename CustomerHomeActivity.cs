using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ShortCut_Android_v4
{
    [Activity(Label = "CustomerHomeActivity")]
    public class CustomerHomeActivity : Activity
    {
        string[] items;
        ListView mainList;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            items = new string[] {
            "Alex",
            "Andrei",
            "Ioana",
            "Marcel",
            "Dede",
            "Justin" };
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.customer_homescreen);
            mainList = (ListView)FindViewById<ListView>(Resource.Id.mainlistview);
            mainList.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, items);
            
            mainList.ItemClick += (s, e) => {
                var t = items[e.Position];
                StartActivity(new Intent(Application.Context, typeof(CalendarActivity)));

            };

        }
    }
}