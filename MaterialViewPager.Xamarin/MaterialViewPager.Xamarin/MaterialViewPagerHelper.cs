using System;
using System.Collections.Concurrent;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Webkit;
using ObservableScrollView;

namespace MaterialViewPager
{
	public class MaterialViewPagerHelper
	{
		private static ConcurrentDictionary<Object, MaterialViewPagerAnimator> _hashMap = new ConcurrentDictionary<object, MaterialViewPagerAnimator>();

    /**
     * Register an MaterialViewPagerAnimator attached to an activity into the ConcurrentHashMap
     *
     * @param context  the context
     * @param animator the current MaterialViewPagerAnimator
     */
    public static void Register(Context context, MaterialViewPagerAnimator animator) {
		
        _hashMap.TryAdd(context, animator);
    }

    public static void Unregister(Context context) {
	    if (context != null)
	    {
		    MaterialViewPagerAnimator r;
		    _hashMap.TryRemove(context, out r);
	    }
    }

    /**
     * Register a RecyclerView to the current MaterialViewPagerAnimator
     * Listen to RecyclerView.OnScrollListener so give to $[onScrollListener] your RecyclerView.OnScrollListener if you already use one
     * For loadmore or anything else
     *
     * @param activity         current context
     * @param recyclerView     the scrollable
     * @param onScrollListener use it if you want to get a callback of the RecyclerView
     */
    public static void RegisterRecyclerView(Activity activity, RecyclerView recyclerView, RecyclerView.OnScrollListener onScrollListener) {
        if (activity != null && _hashMap.ContainsKey(activity)) {
            MaterialViewPagerAnimator animator;
            if (_hashMap.TryGetValue(activity, out animator) && animator != null) {
                animator.RegisterRecyclerView(recyclerView, onScrollListener);
            }
        }
    }

    /**
     * Register a WebView to the current MaterialViewPagerAnimator
     * Listen to ObservableScrollViewCallbacks so give to $[observableScrollViewCallbacks] your ObservableScrollViewCallbacks if you already use one
     * For loadmore or anything else
     *
     * @param activity                      current context
     * @param webView                       the scrollable
     * @param observableScrollViewCallbacks use it if you want to get a callback of the RecyclerView
     */
    public static void RegisterWebView(Activity activity, ObservableWebView webView, IObservableScrollViewCallbacks observableScrollViewCallbacks) {
        if (activity != null && _hashMap.ContainsKey(activity)) {
            MaterialViewPagerAnimator animator;
            if (_hashMap.TryGetValue(activity, out animator) && animator != null) {
                animator.RegisterWebView(webView, observableScrollViewCallbacks);
            }
        }
    }

    /**
     * Register a ScrollView to the current MaterialViewPagerAnimator
     * Listen to ObservableScrollViewCallbacks so give to $[observableScrollViewCallbacks] your ObservableScrollViewCallbacks if you already use one
     * For loadmore or anything else
     *
     * @param activity                      current context
     * @param mScrollView                   the scrollable
     * @param observableScrollViewCallbacks use it if you want to get a callback of the RecyclerView
     */
    public static void RegisterScrollView(Activity activity, ObservableScrollView.ObservableScrollView mScrollView, IObservableScrollViewCallbacks observableScrollViewCallbacks) {
        if (activity != null && _hashMap.ContainsKey(activity)) {
            MaterialViewPagerAnimator animator;
            if (_hashMap.TryGetValue(activity, out animator) && animator != null) {
                animator.RegisterScrollView(mScrollView, observableScrollViewCallbacks);
            }
        }
    }

    /**
     * Retrieve the current MaterialViewPagerAnimator used in this context (Activity)
     *
     * @param context the context
     * @return current MaterialViewPagerAnimator
     */
    public static MaterialViewPagerAnimator GetAnimator(Context context)
    {
	    MaterialViewPagerAnimator animator;
	    return _hashMap.TryGetValue(context, out animator) ? animator : null;
    }

    private static void WebViewLoadJs(WebView webView, string js){
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat) {
            webView.EvaluateJavascript(js, null);
        }else{
            webView.LoadUrl("javascript: " + js);
        }
    }

    /**
     * Have to be called from WebView.WebViewClient.onPageFinished
     * ex : mWebView.setWebViewClient(new WebViewClient() { onPageFinished(WebView view, String url) { [HERE] }});
     * Inject a header to a webview : add a margin-top="**dpx"
     * Had to have a transparent background with a placeholder on top
     * So inject js for placeholder and setLayerType(WebView.LAYER_TYPE_SOFTWARE, null); for transparency
     * TODO : inject JavaScript for Pre-Lolipop with loadUrl("js:...")
     *
     * @param webView
     * @param withAnimation if true, disapear with a fadein
     */
    public static void InjectHeader(WebView webView, bool withAnimation) {
        if (webView != null) {

            MaterialViewPagerAnimator animator = GetAnimator(webView.Context);
            if (animator != null) {

                WebSettings webSettings = webView.Settings;
#pragma warning disable 618
                webSettings.SetRenderPriority(WebSettings.RenderPriority.High);
#pragma warning restore 618
                webSettings.CacheMode = CacheModes.NoCache;
                webSettings.JavaScriptEnabled = true;
                webSettings.DomStorageEnabled = true;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb) {
                    //transparent background
                    webView.SetLayerType(LayerType.Software, null);
                }

                { //inject margin top
                    int marginTop = animator.GetHeaderHeight() + 10;
                    String js = string.Format("document.body.style.marginTop= \"{0}px\"", marginTop);
                    WebViewLoadJs(webView, js);
                }

                {
                    string js = "document.body.style.backround-color= white";
                    WebViewLoadJs(webView,js);
                }

                if (withAnimation)
                    webView.PostDelayed(() => {
                            webView.Visibility = ViewStates.Visible;
                            ObjectAnimator.OfFloat(webView, "alpha", 0, 1).Start();
                        }, 400);
            }
        }
    }

    /**
     * Prepare the webview, set Invisible and transparent background
     * Must call injectHeader next
     */
    public static void PreLoadInjectHeader(WebView mWebView) {
        mWebView.SetBackgroundColor(Color.Transparent);
        mWebView.Visibility = ViewStates.Invisible;
    }
	}
}