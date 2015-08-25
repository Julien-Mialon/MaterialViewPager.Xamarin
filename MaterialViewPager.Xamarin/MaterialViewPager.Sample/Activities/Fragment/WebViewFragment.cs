using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Java.Lang;
using ObservableScrollView;
using String = System.String;

namespace MaterialViewPager.Sample.Activities.Fragment
{
	public class WebViewFragment : Android.Support.V4.App.Fragment {

    private ObservableWebView mWebView;

    public static WebViewFragment newInstance() {
        return new WebViewFragment();
    }

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resources.Layout.fragment_webview, container, false);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);

			 mWebView = view.FindViewById<ObservableWebView>(Resources.Id.webView);

        //must be called before loadUrl()
        MaterialViewPagerHelper.PreLoadInjectHeader(mWebView);

			mWebView.SetWebViewClient();

        //have to inject header when WebView page loaded
        mWebView.setWebViewClient(new WebViewClient() {
            @Override
            public void onPageFinished(WebView view, String url) {
                MaterialViewPagerHelper.injectHeader(mWebView, true);
            }
            @Override
            public boolean shouldOverrideUrlLoading(WebView view, String url) {
                view.loadUrl(url);
                return true;
            }
        });

        mWebView.loadUrl("http://mobile.francetvinfo.fr/");

        MaterialViewPagerHelper.registerWebView(getActivity(), mWebView, null);
		}

}
}