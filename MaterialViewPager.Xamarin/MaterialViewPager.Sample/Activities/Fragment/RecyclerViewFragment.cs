using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Java.Lang;
using MaterialViewPager.Adapter;
using Object = Java.Lang.Object;

namespace MaterialViewPager.Sample.Activities.Fragment
{
	public class RecyclerViewFragment : Android.Support.V4.App.Fragment {

    private RecyclerView mRecyclerView;
    private RecyclerView.Adapter mAdapter;

    private static int ITEM_COUNT = 100;

	private List<object> mContentItems = new List<object>();

    public static RecyclerViewFragment newInstance() {
        return new RecyclerViewFragment();
    }

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resources.Layout.fragment_recyclerview, container, false);
		}

    public override void OnViewCreated(View view, Bundle savedInstanceState) {
        base.OnViewCreated(view, savedInstanceState);
        mRecyclerView = view.FindViewById<RecyclerView>(Resources.Id.recyclerView);
        RecyclerView.LayoutManager layoutManager = new LinearLayoutManager(Activity);
        mRecyclerView.SetLayoutManager(layoutManager);
        mRecyclerView.HasFixedSize = true;

        mAdapter = new RecyclerViewMaterialAdapter(new TestRecyclerViewAdapter(mContentItems));
        mRecyclerView.SetAdapter(mAdapter);

        {
            for (int i = 0; i < ITEM_COUNT; ++i)
                mContentItems.Add(new object());
            mAdapter.NotifyDataSetChanged();
        }

        MaterialViewPagerHelper.RegisterRecyclerView(Activity, mRecyclerView, null);
    }
}
}