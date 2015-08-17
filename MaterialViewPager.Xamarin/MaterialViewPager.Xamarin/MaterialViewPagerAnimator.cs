using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Java.Lang;
using MaterialViewPager.Library;
using Xamarin.NineOldAndroids.Views;

namespace MaterialViewPager
{
	public class MaterialViewPagerAnimator
	{

		private static readonly string TAG = typeof (MaterialViewPagerAnimator).Name;

    public static bool ENABLE_LOG = false;

    private Context context;

    //contains MaterialViewPager subviews references
    private readonly MaterialViewPagerHeader mHeader;

    //duration of translate header enter animation
    private static int ENTER_TOOLBAR_ANIMATION_DURATION = 600;

    //reference to the current MaterialViewPager
    protected MaterialViewPager materialViewPager;

    //toolbar layout elevation (if attr viewpager_enableToolbarElevation = true)
    public float elevation;

    //max scroll which will be dispatched for all scrollable
    public float scrollMax;

    // equals scrollMax in DP (saved to avoir convert to dp anytime I use it)
    public float scrollMaxDp;

    protected internal float lastYOffset = -1; //the current yOffset
    protected internal float lastPercent = 0; //the current Percent

    //contains the attributes given to MaterialViewPager from layout
    protected MaterialViewPagerSettings settings;

    //list of all registered scrollers
    protected List<View> scrollViewList = new List<View>();

    //save all yOffsets of scrollables
    protected Dictionary<Object, int> yOffsets = new Dictionary<Object, int>();

    //the last headerYOffset during scroll
    private float headerYOffset = float.MaxValue;

    //the tmp headerAnimator (not null if animating, else null)
    private Object headerAnimator;

    bool followScrollToolbarIsVisible = false;
    float firstScrollValue = float.MinValue;
    bool justToolbarAnimated = false;

    //intial distance between pager & toolbat
    float initialDistance = -1;

    public MaterialViewPagerAnimator(MaterialViewPager materialViewPager) {

        this.settings = materialViewPager.Settings;

        this.materialViewPager = materialViewPager;
        this.mHeader = materialViewPager.MaterialViewPagerHeader;
        this.context = mHeader.Context;

        // initialise the scrollMax to headerHeight, so until the first cell touch the top of the screen
        this.scrollMax = this.settings.headerHeight;
        //save in into dp once
        this.scrollMaxDp = Utils.dpToPx(this.scrollMax, context);

        //heightMaxScrollToolbar = context.getResources().getDimension(R.dimen.material_viewpager_padding_top);
        elevation = dpToPx(4, context);
    }

    /**
     * When notified for scroll, dispatch it to all registered scrollables
     *
     * @param source
     * @param yOffset
     */
    protected void dispatchScrollOffset(Object source, float yOffset) {
        if (scrollViewList != null) {
            for (Object scroll : scrollViewList) {

                //do not re-scroll the source
                if (scroll != null && scroll != source) {
                    setScrollOffset(scroll, yOffset);
                }
            }
        }
    }

    /**
     * When notified for scroll, dispatch it to all registered scrollables
     *
     * @param scroll
     * @param yOffset
     */
    private void setScrollOffset(Object scroll, float yOffset) {
        //do not re-scroll the source
        if (scroll != null && yOffset >= 0) {

            scrollTo(scroll, yOffset);

            //save the current yOffset of the scrollable on the yOffsets hashmap
            yOffsets.put(scroll, (int) yOffset);
        }
    }

    /**
     * Called when a scroller(RecyclerView/ListView,ScrollView,WebView) scrolled by the user
     *
     * @param source  the scroller
     * @param yOffset the scroller current yOffset
     */
    public bool onMaterialScrolled(Object source, float yOffset) {

        if(initialDistance == -1 || initialDistance == 0) {
            initialDistance = mHeader.MPagerSlidingTabStrip.getTop() - mHeader.Toolbar.getBottom();
        }

        //only if yOffset changed
        if (yOffset == lastYOffset)
            return false;

        float scrollTop = -yOffset;

        {
            //parallax scroll of the Background ImageView (the KenBurnsView)
            if (mHeader.HeaderBackground != null) {

                if (this.settings.parallaxHeaderFactor != 0)
                    ViewHelper.setTranslationY(mHeader.HeaderBackground, scrollTop / this.settings.parallaxHeaderFactor);

                if (ViewHelper.getY(mHeader.HeaderBackground) >= 0)
                    ViewHelper.setY(mHeader.HeaderBackground, 0);
            }


        }

        if (ENABLE_LOG)
            Log.d("yOffset", "" + yOffset);

        //dispatch the new offset to all registered scrollables
        dispatchScrollOffset(source, minMax(0, yOffset, scrollMaxDp));

        float percent = yOffset / scrollMax;

        //distance between pager & toolbar
        float newDistance = ViewHelper.getY(mHeader.MPagerSlidingTabStrip) - mHeader.Toolbar.getBottom();

        percent = 1 - newDistance/initialDistance;

        if(Float.isNaN(percent)) //fix for orientation change
            return false;

        percent = minMax(0, percent, 1);
        {

            if (!settings.toolbarTransparent) {
                // change color of toolbar & viewpager indicator &  statusBaground
                setColorPercent(percent);
            } else {
                if (justToolbarAnimated) {
                    if (toolbarJoinsTabs())
                        setColorPercent(1);
                    else if (lastPercent != percent) {
                        animateColorPercent(0, 200);
                    }
                }
            }

            lastPercent = percent; //save the percent

            if (mHeader.MPagerSlidingTabStrip != null) { //move the viewpager indicator
                //float newY = ViewHelper.getY(mHeader.mPagerSlidingTabStrip) + scrollTop;

                if (ENABLE_LOG)
                    Log.d(TAG, "" + scrollTop);


                //mHeader.mPagerSlidingTabStrip.setTranslationY(mHeader.getToolbar().getBottom()-mHeader.mPagerSlidingTabStrip.getY());
                if (scrollTop <= 0) {
                    ViewHelper.setTranslationY(mHeader.MPagerSlidingTabStrip, scrollTop);
                    ViewHelper.setTranslationY(mHeader.ToolbarLayoutBackground, scrollTop);

                    //when
                    if (ViewHelper.getY(mHeader.MPagerSlidingTabStrip) < mHeader.GetToolbar().getBottom()) {
                        float ty = mHeader.GetToolbar().getBottom() - mHeader.MPagerSlidingTabStrip.getTop();
                        ViewHelper.setTranslationY(mHeader.MPagerSlidingTabStrip, ty);
                        ViewHelper.setTranslationY(mHeader.ToolbarLayoutBackground, ty);
                    }
                }

            }


            if (mHeader.MLogo != null) { //move the header logo to toolbar

                if (this.settings.hideLogoWithFade) {
                    ViewHelper.setAlpha(mHeader.MLogo, 1 - percent);
                    ViewHelper.setTranslationY(mHeader.MLogo, (mHeader.FinalTitleY - mHeader.OriginalTitleY) * percent);
                } else {
                    ViewHelper.setTranslationY(mHeader.MLogo, (mHeader.FinalTitleY - mHeader.OriginalTitleY) * percent);
                    ViewHelper.setTranslationX(mHeader.MLogo, (mHeader.FinalTitleX - mHeader.OriginalTitleX) * percent);

                    float scale = (1 - percent) * (1 - mHeader.FinalScale) + mHeader.FinalScale;
                    setScale(scale, mHeader.MLogo);
                }
            }

            if (this.settings.hideToolbarAndTitle && mHeader.ToolbarLayout != null) {
                bool scrollUp = lastYOffset < yOffset;

                if (scrollUp) {
                    scrollUp(yOffset);
                } else {
                    scrollDown(yOffset);
                }
            }
        }

        if (headerAnimator != null && percent < 1) {
            if (headerAnimator instanceof ObjectAnimator)
                ((ObjectAnimator) headerAnimator).cancel();
            else if (headerAnimator instanceof android.animation.ObjectAnimator)
                ((android.animation.ObjectAnimator) headerAnimator).cancel();
            headerAnimator = null;
        }

        lastYOffset = yOffset;

        return true;
    }

    private void scrollUp(float yOffset) {
        if (ENABLE_LOG)
            Log.d(TAG, "scrollUp");

        followScrollToolbarLayout(yOffset);
    }

    private void scrollDown(float yOffset) {
        if (ENABLE_LOG)
            Log.d(TAG, "scrollDown");
        if (yOffset > mHeader.ToolbarLayout.getHeight() * 1.5f) {
            animateEnterToolbarLayout(yOffset);
        } else {
            if (headerAnimator != null) {
                followScrollToolbarIsVisible = true;
            } else {
                headerYOffset = Float.MAX_VALUE;
                followScrollToolbarLayout(yOffset);
            }
        }
    }

    /**
     * Change the color of the statusbackground, toolbar, toolbarlayout and pagertitlestrip
     * With a color transition animation
     *
     * @param color    the color
     * @param duration the transition color animation duration
     */
    public void setColor(int color, int duration) {
        ValueAnimator colorAnim = ObjectAnimator.ofInt(mHeader.HeaderBackground, "backgroundColor", settings.color, color);
        colorAnim.setEvaluator(new ArgbEvaluator());
        colorAnim.setDuration(duration);
        colorAnim.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() {
            
            public void onAnimationUpdate(ValueAnimator animation) {
                int animatedValue = (Integer) animation.getAnimatedValue();
                int colorAlpha = colorWithAlpha(animatedValue, lastPercent);
                mHeader.headerBackground.setBackgroundColor(colorAlpha);
                mHeader.statusBackground.setBackgroundColor(colorAlpha);
                mHeader.toolbar.setBackgroundColor(colorAlpha);
                mHeader.toolbarLayoutBackground.setBackgroundColor(colorAlpha);
                mHeader.mPagerSlidingTabStrip.setBackgroundColor(colorAlpha);

                //set the new color as MaterialViewPager's color
                settings.color = animatedValue;
            }
        });
        colorAnim.start();
    }

    public void animateColorPercent(float percent, int duration) {
        ValueAnimator valueAnimator = ValueAnimator.ofFloat(lastPercent, percent);
        valueAnimator.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() {
            
            public void onAnimationUpdate(ValueAnimator animation) {
                setColorPercent((float) animation.getAnimatedValue());
            }
        });
        valueAnimator.setDuration(duration);
        valueAnimator.start();
    }

    public void setColorPercent(float percent) {
        // change color of
        // toolbar & viewpager indicator &  statusBaground

        setBackgroundColor(
                colorWithAlpha(this.settings.color, percent),
                mHeader.StatusBackground
        );

        if (percent >= 1) {
            setBackgroundColor(
                    colorWithAlpha(this.settings.color, percent),
                    mHeader.Toolbar,
                    mHeader.ToolbarLayoutBackground,
                    mHeader.MPagerSlidingTabStrip
            );
        } else {
            setBackgroundColor(
                    colorWithAlpha(this.settings.color, 0),
                    mHeader.Toolbar,
                    mHeader.ToolbarLayoutBackground,
                    mHeader.MPagerSlidingTabStrip
            );
        }

        if (this.settings.enableToolbarElevation && toolbarJoinsTabs())
            setElevation(
                    (percent == 1) ? elevation : 0,
                    mHeader.Toolbar,
                    mHeader.ToolbarLayoutBackground,
                    mHeader.MPagerSlidingTabStrip,
                    mHeader.MLogo
            );
    }

    private bool toolbarJoinsTabs() {
        return (mHeader.Toolbar.getBottom() == mHeader.MPagerSlidingTabStrip.getTop() + ViewHelper.getTranslationY(mHeader.MPagerSlidingTabStrip));
    }

    /**
     * move the toolbarlayout (containing toolbar & tabs)
     * following the current scroll
     */
    private void followScrollToolbarLayout(float yOffset) {
        if (mHeader.Toolbar.getBottom() == 0)
            return;

        if (toolbarJoinsTabs()) {
            if (firstScrollValue == Float.MIN_VALUE)
                firstScrollValue = yOffset;

            float translationY = firstScrollValue - yOffset;

            if (ENABLE_LOG)
                Log.d(TAG, "translationY " + translationY);

            ViewHelper.setTranslationY(mHeader.ToolbarLayout, translationY);
        } else {
            ViewHelper.setTranslationY(mHeader.ToolbarLayout, 0);
            justToolbarAnimated = false;
        }

        followScrollToolbarIsVisible = (ViewHelper.getY(mHeader.ToolbarLayout) >= 0);
    }

    /**
     * Animate enter toolbarlayout
     *
     * @param yOffset
     */
    private void animateEnterToolbarLayout(float yOffset) {
        if (!followScrollToolbarIsVisible && headerAnimator != null) {
            if (headerAnimator instanceof ObjectAnimator)
                ((ObjectAnimator) headerAnimator).cancel();
            else if (headerAnimator instanceof android.animation.ObjectAnimator)
                ((android.animation.ObjectAnimator) headerAnimator).cancel();
            headerAnimator = null;
        }

        if (headerAnimator == null) {
            if (android.os.Build.VERSION.SDK_INT > Build.VERSION_CODES.GINGERBREAD_MR1) {
                headerAnimator = android.animation.ObjectAnimator.ofFloat(mHeader.ToolbarLayout, "translationY", 0).setDuration(ENTER_TOOLBAR_ANIMATION_DURATION);
                ((android.animation.ObjectAnimator) headerAnimator).addListener(new android.animation.AnimatorListenerAdapter() {
                    
                    public void onAnimationEnd(android.animation.Animator animation) {
                        super.onAnimationEnd(animation);
                        followScrollToolbarIsVisible = true;
                        firstScrollValue = Float.MIN_VALUE;
                        justToolbarAnimated = true;
                    }
                });
                ((android.animation.ObjectAnimator) headerAnimator).start();
            } else {
                headerAnimator = ObjectAnimator.ofFloat(mHeader.ToolbarLayout, "translationY", 0).setDuration(ENTER_TOOLBAR_ANIMATION_DURATION);
                ((ObjectAnimator) headerAnimator).addListener(new AnimatorListenerAdapter() {
                    
                    public void onAnimationEnd(Animator animation) {
                        super.onAnimationEnd(animation);
                        followScrollToolbarIsVisible = true;
                        firstScrollValue = Float.MIN_VALUE;
                        justToolbarAnimated = true;
                    }
                });
                ((ObjectAnimator) headerAnimator).start();
            }
            headerYOffset = yOffset;
        }
    }

    public int getHeaderHeight() {
        return this.settings.headerHeight;
    }

    protected bool isNewYOffset(int yOffset) {
        if (lastYOffset == -1)
            return true;
        else
            return yOffset != lastYOffset;
    }

    //region register scrollables

    /**
     * Register a RecyclerView to the current MaterialViewPagerAnimator
     * Listen to RecyclerView.OnScrollListener so give to $[onScrollListener] your RecyclerView.OnScrollListener if you already use one
     * For loadmore or anything else
     *
     * @param recyclerView     the scrollable
     * @param onScrollListener use it if you want to get a callback of the RecyclerView
     */
    public void registerRecyclerView(RecyclerView recyclerView, RecyclerView.OnScrollListener onScrollListener) {
        if (recyclerView != null) {
            scrollViewList.add(recyclerView); //add to the scrollable list
            yOffsets.put(recyclerView, recyclerView.getScrollY()); //save the initial recyclerview's yOffset (0) into hashmap
            //only necessary for recyclerview

            //listen to scroll
            recyclerView.addOnScrollListener(new RecyclerView.OnScrollListener() {

                bool firstZeroPassed;

                
                public void onScrollStateChanged(RecyclerView recyclerView, int newState) {
                    super.onScrollStateChanged(recyclerView, newState);
                    if (onScrollListener != null)
                        onScrollListener.onScrollStateChanged(recyclerView, newState);
                }

                
                public void onScrolled(RecyclerView recyclerView, int dx, int dy) {
                    super.onScrolled(recyclerView, dx, dy);

                    if (onScrollListener != null)
                        onScrollListener.onScrolled(recyclerView, dx, dy);

                    int yOffset = yOffsets.get(recyclerView);

                    yOffset += dy;
                    yOffsets.put(recyclerView, yOffset); //save the new offset

                    //first time you get 0, don't share it to others scrolls
                    if (yOffset == 0 && !firstZeroPassed) {
                        firstZeroPassed = true;
                        return;
                    }

                    //only if yOffset changed
                    if (isNewYOffset(yOffset))
                        onMaterialScrolled(recyclerView, yOffset);
                }
            });

            recyclerView.post(new Runnable() {
                
                public void run() {
                    setScrollOffset(recyclerView, lastYOffset);
                }
            });
        }
    }

    /**
     * Register a ScrollView to the current MaterialViewPagerAnimator
     * Listen to ObservableScrollViewCallbacks so give to $[observableScrollViewCallbacks] your ObservableScrollViewCallbacks if you already use one
     * For loadmore or anything else
     *
     * @param scrollView                    the scrollable
     * @param observableScrollViewCallbacks use it if you want to get a callback of the RecyclerView
     */
    public void registerScrollView(ObservableScrollView scrollView, ObservableScrollViewCallbacks observableScrollViewCallbacks) {
        if (scrollView != null) {
            scrollViewList.add(scrollView);  //add to the scrollable list
            if (scrollView.getParent() != null && scrollView.getParent().getParent() != null && scrollView.getParent().getParent() instanceof ViewGroup)
                scrollView.setTouchInterceptionViewGroup((ViewGroup) scrollView.getParent().getParent());
            scrollView.setScrollViewCallbacks(new ObservableScrollViewCallbacks() {

                bool firstZeroPassed;

                
                public void onScrollChanged(int yOffset, bool b, bool b2) {
                    if (observableScrollViewCallbacks != null)
                        observableScrollViewCallbacks.onScrollChanged(yOffset, b, b2);

                    //first time you get 0, don't share it to others scrolls
                    if (yOffset == 0 && !firstZeroPassed) {
                        firstZeroPassed = true;
                        return;
                    }

                    //only if yOffset changed
                    if (isNewYOffset(yOffset))
                        onMaterialScrolled(scrollView, yOffset);
                }

                
                public void onDownMotionEvent() {
                    if (observableScrollViewCallbacks != null)
                        observableScrollViewCallbacks.onDownMotionEvent();
                }

                
                public void onUpOrCancelMotionEvent(ScrollState scrollState) {
                    if (observableScrollViewCallbacks != null)
                        observableScrollViewCallbacks.onUpOrCancelMotionEvent(scrollState);
                }
            });

            scrollView.post(new Runnable() {
                
                public void run() {
                    setScrollOffset(scrollView, lastYOffset);
                }
            });
        }
    }

    /**
     * Register a WebView to the current MaterialViewPagerAnimator
     * Listen to ObservableScrollViewCallbacks so give to $[observableScrollViewCallbacks] your ObservableScrollViewCallbacks if you already use one
     * For loadmore or anything else
     *
     * @param webView                       the scrollable
     * @param observableScrollViewCallbacks use it if you want to get a callback of the RecyclerView
     */
    public void registerWebView(ObservableWebView webView, IObservableScrollViewCallbacks observableScrollViewCallbacks) {
        if (webView != null) {
            if (scrollViewList.isEmpty())
                onMaterialScrolled(webView, webView.getCurrentScrollY());
            scrollViewList.add(webView);  //add to the scrollable list
            webView.setScrollViewCallbacks(new ObservableScrollViewCallbacks() {
                
                public void onScrollChanged(int yOffset, bool b, bool b2) {
                    if (observableScrollViewCallbacks != null)
                        observableScrollViewCallbacks.onScrollChanged(yOffset, b, b2);

                    if (isNewYOffset(yOffset))
                        onMaterialScrolled(webView, yOffset);
                }

                
                public void onDownMotionEvent() {
                    if (observableScrollViewCallbacks != null)
                        observableScrollViewCallbacks.onDownMotionEvent();
                }

                
                public void onUpOrCancelMotionEvent(ScrollState scrollState) {
                    if (observableScrollViewCallbacks != null)
                        observableScrollViewCallbacks.onUpOrCancelMotionEvent(scrollState);
                }
            });

            this.setScrollOffset(webView, -lastYOffset);
        }
    }

    //endregion

    public void restoreScroll(float scroll, MaterialViewPagerSettings settings) {
        //try to scroll up, on a looper to wait until restored
        new Handler(Looper.getMainLooper()).postDelayed(new Runnable() {
            
            public void run() {
                if(!onMaterialScrolled(null, scroll)){
                    restoreScroll(scroll,settings);
                }
            }
        },100);

    }

    public void onViewPagerPageChanged() {
        scrollDown(lastYOffset);

        View visibleView = getTheVisibileView(scrollViewList);
        if (!canScroll(visibleView)) {
            followScrollToolbarLayout(0);
            onMaterialScrolled(visibleView, 0);
        }
    }
}

}