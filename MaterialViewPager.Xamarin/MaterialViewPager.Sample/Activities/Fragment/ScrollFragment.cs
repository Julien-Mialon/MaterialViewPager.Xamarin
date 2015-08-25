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

namespace MaterialViewPager.Sample.Activities.Fragment
{
	public class ScrollFragment : Android.Support.V4.App.Fragment {

    private ObservableScrollView.ObservableScrollView mScrollView;

    public static ScrollFragment newInstance() {
        return new ScrollFragment();
    }

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resources.Layout.fragment_scroll, container, false);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			mScrollView = view.FindViewById<ObservableScrollView.ObservableScrollView>(Resources.Id.scrollView);

        MaterialViewPagerHelper.RegisterScrollView(Activity, mScrollView, null);
		}

    }
}

}