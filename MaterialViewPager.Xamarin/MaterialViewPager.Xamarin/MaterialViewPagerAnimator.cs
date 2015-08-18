using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Java.Lang;
using ObservableScrollView;
using Xamarin.NineOldAndroids.Animations;
using Xamarin.NineOldAndroids.Views;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace MaterialViewPager
{
	public class MaterialViewPagerAnimator
	{

		private static readonly string _tag = typeof(MaterialViewPagerAnimator).Name;

		public static bool EnableLog = false;

		//contains MaterialViewPager subviews references
		private readonly MaterialViewPagerHeader _mHeader;

		//duration of translate header enter animation
		private static readonly int _enterToolbarAnimationDuration = 600;

		//reference to the current MaterialViewPager
		protected MaterialViewPager MaterialViewPager;

		//toolbar layout elevation (if attr viewpager_enableToolbarElevation = true)
		public float Elevation;

		//max scroll which will be dispatched for all scrollable
		public float ScrollMax;

		// equals scrollMax in DP (saved to avoir convert to dp anytime I use it)
		public float ScrollMaxDp;

		protected internal float LastYOffset = -1; //the current yOffset
		protected internal float LastPercent; //the current Percent

		//contains the attributes given to MaterialViewPager from layout
		protected MaterialViewPagerSettings Settings;

		//list of all registered scrollers
		protected List<View> ScrollViewList = new List<View>();

		//save all yOffsets of scrollables
		protected internal Dictionary<Object, int> YOffsets = new Dictionary<Object, int>();

		//the tmp headerAnimator (not null if animating, else null)
		private Object _headerAnimator;

		internal bool FollowScrollToolbarIsVisible;
		internal float FirstScrollValue = float.MinValue;
		internal bool JustToolbarAnimated;

		//intial distance between pager & toolbat
		float _initialDistance = -1;

		public MaterialViewPagerAnimator(MaterialViewPager materialViewPager)
		{
			Settings = materialViewPager.Settings;

			MaterialViewPager = materialViewPager;
			_mHeader = materialViewPager.MaterialViewPagerHeader;
			Context context = _mHeader.Context;

			// initialise the scrollMax to headerHeight, so until the first cell touch the top of the screen
			ScrollMax = Settings.HeaderHeight;
			//save in into dp once
			ScrollMaxDp = Utils.DpToPx(ScrollMax, context);

			//heightMaxScrollToolbar = context.getResources().getDimension(R.dimen.material_viewpager_padding_top);
			Elevation = Utils.DpToPx(4, context);
		}

		/**
		 * When notified for scroll, dispatch it to all registered scrollables
		 *
		 * @param source
		 * @param yOffset
		 */
		protected void DispatchScrollOffset(Object source, float yOffset)
		{
			if (ScrollViewList != null)
			{
				foreach (View scroll in ScrollViewList)
				{

					//do not re-scroll the source
					if (scroll != null && scroll != source)
					{
						SetScrollOffset(scroll, yOffset);
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
		private void SetScrollOffset(Object scroll, float yOffset)
		{
			//do not re-scroll the source
			if (scroll != null && yOffset >= 0)
			{

				Utils.ScrollTo(scroll, yOffset);

				//save the current yOffset of the scrollable on the yOffsets hashmap
				YOffsets.AddOrUpdate(scroll, (int)yOffset);
			}
		}

		/**
		 * Called when a scroller(RecyclerView/ListView,ScrollView,WebView) scrolled by the user
		 *
		 * @param source  the scroller
		 * @param yOffset the scroller current yOffset
		 */
		public bool OnMaterialScrolled(Object source, float yOffset)
		{

			if (_initialDistance == -1 || _initialDistance == 0)
			{
				_initialDistance = _mHeader.MPagerSlidingTabStrip.Top - _mHeader.Toolbar.Bottom;
			}

			//only if yOffset changed
			if (yOffset == LastYOffset)
				return false;

			float scrollTop = -yOffset;

			{
				//parallax scroll of the Background ImageView (the KenBurnsView)
				if (_mHeader.HeaderBackground != null)
				{

					if (Settings.ParallaxHeaderFactor != 0)
						ViewHelper.SetTranslationY(_mHeader.HeaderBackground, scrollTop / Settings.ParallaxHeaderFactor);

					if (ViewHelper.GetY(_mHeader.HeaderBackground) >= 0)
						ViewHelper.SetY(_mHeader.HeaderBackground, 0);
				}


			}

			if (EnableLog)
				Log.Debug("yOffset", "" + yOffset);

			//dispatch the new offset to all registered scrollables
			DispatchScrollOffset(source, Utils.MinMax(0, yOffset, ScrollMaxDp));

			//distance between pager & toolbar
			float newDistance = ViewHelper.GetY(_mHeader.MPagerSlidingTabStrip) - _mHeader.Toolbar.Bottom;

			float percent = 1 - newDistance / _initialDistance;

			if (float.IsNaN(percent)) //fix for orientation change
				return false;

			percent = Utils.MinMax(0, percent, 1);
			{

				if (!Settings.ToolbarTransparent)
				{
					// change color of toolbar & viewpager indicator &  statusBaground
					SetColorPercent(percent);
				}
				else
				{
					if (JustToolbarAnimated)
					{
						if (ToolbarJoinsTabs())
							SetColorPercent(1);
						else if (LastPercent != percent)
						{
							AnimateColorPercent(0, 200);
						}
					}
				}

				LastPercent = percent; //save the percent

				if (_mHeader.MPagerSlidingTabStrip != null)
				{ //move the viewpager indicator
					//float newY = ViewHelper.getY(mHeader.mPagerSlidingTabStrip) + scrollTop;

					if (EnableLog)
						Log.Debug(_tag, "" + scrollTop);


					//mHeader.mPagerSlidingTabStrip.setTranslationY(mHeader.getToolbar().getBottom()-mHeader.mPagerSlidingTabStrip.getY());
					if (scrollTop <= 0)
					{
						ViewHelper.SetTranslationY(_mHeader.MPagerSlidingTabStrip, scrollTop);
						ViewHelper.SetTranslationY(_mHeader.ToolbarLayoutBackground, scrollTop);

						//when
						if (ViewHelper.GetY(_mHeader.MPagerSlidingTabStrip) < _mHeader.GetToolbar().Bottom)
						{
							float ty = _mHeader.GetToolbar().Bottom - _mHeader.MPagerSlidingTabStrip.Top;
							ViewHelper.SetTranslationY(_mHeader.MPagerSlidingTabStrip, ty);
							ViewHelper.SetTranslationY(_mHeader.ToolbarLayoutBackground, ty);
						}
					}

				}


				if (_mHeader.MLogo != null)
				{ //move the header logo to toolbar

					if (Settings.HideLogoWithFade)
					{
						ViewHelper.SetAlpha(_mHeader.MLogo, 1 - percent);
						ViewHelper.SetTranslationY(_mHeader.MLogo, (_mHeader.FinalTitleY - _mHeader.OriginalTitleY) * percent);
					}
					else
					{
						ViewHelper.SetTranslationY(_mHeader.MLogo, (_mHeader.FinalTitleY - _mHeader.OriginalTitleY) * percent);
						ViewHelper.SetTranslationX(_mHeader.MLogo, (_mHeader.FinalTitleX - _mHeader.OriginalTitleX) * percent);

						float scale = (1 - percent) * (1 - _mHeader.FinalScale) + _mHeader.FinalScale;
						Utils.SetScale(scale, _mHeader.MLogo);
					}
				}

				if (Settings.HideToolbarAndTitle && _mHeader.ToolbarLayout != null)
				{
					bool scrollUp = LastYOffset < yOffset;

					if (scrollUp)
					{
						ScrollUp(yOffset);
					}
					else
					{
						ScrollDown(yOffset);
					}
				}
			}

			if (_headerAnimator != null && percent < 1)
			{
				ObjectAnimator objectAnimator = _headerAnimator as ObjectAnimator;
				if (objectAnimator != null)
					objectAnimator.Cancel();
				else
				{
					Android.Animation.ObjectAnimator androidAnimator = _headerAnimator as Android.Animation.ObjectAnimator;
					if (androidAnimator != null)
						androidAnimator.Cancel();
				}
				_headerAnimator = null;
			}

			LastYOffset = yOffset;

			return true;
		}

		private void ScrollUp(float yOffset)
		{
			if (EnableLog)
				Log.Debug(_tag, "scrollUp");

			FollowScrollToolbarLayout(yOffset);
		}

		private void ScrollDown(float yOffset)
		{
			if (EnableLog)
				Log.Debug(_tag, "scrollDown");
			if (yOffset > _mHeader.ToolbarLayout.Height * 1.5f)
			{
				AnimateEnterToolbarLayout();
			}
			else
			{
				if (_headerAnimator != null)
				{
					FollowScrollToolbarIsVisible = true;
				}
				else
				{
					FollowScrollToolbarLayout(yOffset);
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
		public void SetColor(int color, int duration)
		{
			ValueAnimator colorAnim = ObjectAnimator.OfInt(_mHeader.HeaderBackground, "backgroundColor", Settings.Color, color);
			colorAnim.SetEvaluator(new ArgbEvaluator());
			colorAnim.SetDuration(duration);
			colorAnim.Update += (sender, args) =>
			{
				int animatedValue = (int)colorAnim.AnimatedValue;
				int colorAlpha = Utils.ColorWithAlpha(animatedValue, LastPercent);
				_mHeader.HeaderBackground.SetBackgroundColor(new Color(colorAlpha));
				_mHeader.StatusBackground.SetBackgroundColor(new Color(colorAlpha));
				_mHeader.Toolbar.SetBackgroundColor(new Color(colorAlpha));
				_mHeader.ToolbarLayoutBackground.SetBackgroundColor(new Color(colorAlpha));
				_mHeader.MPagerSlidingTabStrip.SetBackgroundColor(new Color(colorAlpha));

				//set the new color as MaterialViewPager's color
				Settings.Color = animatedValue;
			};

			colorAnim.Start();
		}

		public void AnimateColorPercent(float percent, int duration)
		{
			ValueAnimator valueAnimator = ValueAnimator.OfFloat(LastPercent, percent);
			valueAnimator.Update += (sender, args) =>
			{
				SetColorPercent((float)valueAnimator.AnimatedValue);
			};
			valueAnimator.SetDuration(duration);
			valueAnimator.Start();
		}

		public void SetColorPercent(float percent)
		{
			// change color of
			// toolbar & viewpager indicator &  statusBaground

			Utils.SetBackgroundColor(
					Utils.ColorWithAlpha(Settings.Color, percent),
					_mHeader.StatusBackground
			);

			Utils.SetBackgroundColor(
				Utils.ColorWithAlpha(Settings.Color, percent >= 1 ? percent : 0),
				_mHeader.Toolbar,
				_mHeader.ToolbarLayoutBackground,
				_mHeader.MPagerSlidingTabStrip
				);

			if (Settings.EnableToolbarElevation && ToolbarJoinsTabs())
				Utils.SetElevation(
						(percent == 1) ? Elevation : 0,
						_mHeader.Toolbar,
						_mHeader.ToolbarLayoutBackground,
						_mHeader.MPagerSlidingTabStrip,
						_mHeader.MLogo
				);
		}

		private bool ToolbarJoinsTabs()
		{
			return (_mHeader.Toolbar.Bottom == _mHeader.MPagerSlidingTabStrip.Top + ViewHelper.GetTranslationY(_mHeader.MPagerSlidingTabStrip));
		}

		/**
		 * move the toolbarlayout (containing toolbar & tabs)
		 * following the current scroll
		 */
		private void FollowScrollToolbarLayout(float yOffset)
		{
			if (_mHeader.Toolbar.Bottom == 0)
				return;

			if (ToolbarJoinsTabs())
			{
				if (FirstScrollValue == float.MinValue)
					FirstScrollValue = yOffset;

				float translationY = FirstScrollValue - yOffset;

				if (EnableLog)
					Log.Debug(_tag, "translationY " + translationY);

				ViewHelper.SetTranslationY(_mHeader.ToolbarLayout, translationY);
			}
			else
			{
				ViewHelper.SetTranslationY(_mHeader.ToolbarLayout, 0);
				JustToolbarAnimated = false;
			}

			FollowScrollToolbarIsVisible = (ViewHelper.GetY(_mHeader.ToolbarLayout) >= 0);
		}

		/**
		 * Animate enter toolbarlayout
		 *
		 * @param yOffset
		 */
		private void AnimateEnterToolbarLayout()
		{
			if (!FollowScrollToolbarIsVisible && _headerAnimator != null)
			{
				ObjectAnimator objectAnimator = _headerAnimator as ObjectAnimator;
				if (objectAnimator != null)
					objectAnimator.Cancel();
				else
				{
					Android.Animation.ObjectAnimator androidObjectAnimator = _headerAnimator as Android.Animation.ObjectAnimator;
					if (androidObjectAnimator != null)
						androidObjectAnimator.Cancel();
				}
				_headerAnimator = null;
			}

			if (_headerAnimator == null)
			{
				if (Build.VERSION.SdkInt > BuildVersionCodes.GingerbreadMr1)
				{
					_headerAnimator = Android.Animation.ObjectAnimator.OfFloat(_mHeader.ToolbarLayout, "translationY", 0).SetDuration(_enterToolbarAnimationDuration);
					Android.Animation.ObjectAnimator animator = (Android.Animation.ObjectAnimator)_headerAnimator;

					animator.AnimationEnd += (sender, args) =>
					{
						FollowScrollToolbarIsVisible = true;
						FirstScrollValue = float.MinValue;
						JustToolbarAnimated = true;
					};
					animator.Start();
				}
				else
				{
					_headerAnimator = ObjectAnimator.OfFloat(_mHeader.ToolbarLayout, "translationY", 0).SetDuration(_enterToolbarAnimationDuration);
					ObjectAnimator animator = (ObjectAnimator) _headerAnimator;
					animator.AddListener(new AnimatorListener(this));
					animator.Start();
				}
			}
		}

		public int GetHeaderHeight()
		{
			return Settings.HeaderHeight;
		}

		protected internal bool IsNewYOffset(int yOffset)
		{
			if (LastYOffset == -1)
				return true;
			return yOffset != LastYOffset;
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
		public void RegisterRecyclerView(RecyclerView recyclerView, RecyclerView.OnScrollListener onScrollListener)
		{
			if (recyclerView != null)
			{
				ScrollViewList.Add(recyclerView); //add to the scrollable list
				YOffsets.AddOrUpdate(recyclerView, recyclerView.ScrollY); //save the initial recyclerview's yOffset (0) into hashmap
				//only necessary for recyclerview

				//listen to scroll
				recyclerView.AddOnScrollListener(new RecyclerViewScrollListener(this, onScrollListener));

				recyclerView.Post(() => SetScrollOffset(recyclerView, LastYOffset));
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
		public void RegisterScrollView(ObservableScrollView.ObservableScrollView scrollView, IObservableScrollViewCallbacks observableScrollViewCallbacks)
		{
			if (scrollView != null)
			{
				ScrollViewList.Add(scrollView);  //add to the scrollable list
				if (scrollView.Parent != null && scrollView.Parent.Parent is ViewGroup)
					scrollView.SetTouchInterceptionViewGroup((ViewGroup)scrollView.Parent.Parent);
				scrollView.SetScrollViewCallbacks(new ObservableScrollViewCallbacks(this, scrollView, observableScrollViewCallbacks));
				scrollView.Post(() => SetScrollOffset(scrollView, LastYOffset));
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
		public void RegisterWebView(ObservableWebView webView, IObservableScrollViewCallbacks observableScrollViewCallbacks)
		{
			if (webView != null)
			{
				if (!ScrollViewList.Any())
					OnMaterialScrolled(webView, webView.GetCurrentScrollY());
				ScrollViewList.Add(webView);  //add to the scrollable list

				webView.SetScrollViewCallbacks(new ObservableWebViewCallbacks(this, webView, observableScrollViewCallbacks));
				SetScrollOffset(webView, -LastYOffset);
			}
		}

		//endregion

		public void RestoreScroll(float scroll, MaterialViewPagerSettings settings)
		{
			//try to scroll up, on a looper to wait until restored
			new Handler(Looper.MainLooper).PostDelayed(() =>
			{
				if (!OnMaterialScrolled(null, scroll))
				{
					RestoreScroll(scroll, settings);
				}
			}, 100);
		}

		public void OnViewPagerPageChanged()
		{
			ScrollDown(LastYOffset);

			View visibleView = Utils.GetTheVisibileView(ScrollViewList);
			if (!Utils.CanScroll(visibleView))
			{
				FollowScrollToolbarLayout(0);
				OnMaterialScrolled(visibleView, 0);
			}
		}
	}

	public class ObservableWebViewCallbacks : IObservableScrollViewCallbacks
	{
		private readonly MaterialViewPagerAnimator _animator;
		private readonly ObservableWebView _webView;
		private readonly IObservableScrollViewCallbacks _observableScrollViewCallbacks;

		public ObservableWebViewCallbacks(MaterialViewPagerAnimator materialViewPagerAnimator, ObservableWebView webView, IObservableScrollViewCallbacks observableScrollViewCallbacks)
		{
			_animator = materialViewPagerAnimator;
			_webView = webView;
			_observableScrollViewCallbacks = observableScrollViewCallbacks;
		}

		public void OnScrollChanged(int yOffset, bool b, bool b2)
		{
			if (_observableScrollViewCallbacks != null)
				_observableScrollViewCallbacks.OnScrollChanged(yOffset, b, b2);

			if (_animator.IsNewYOffset(yOffset))
				_animator.OnMaterialScrolled(_webView, yOffset);
		}


		public void OnDownMotionEvent()
		{
			if (_observableScrollViewCallbacks != null)
				_observableScrollViewCallbacks.OnDownMotionEvent();
		}


		public void OnUpOrCancelMotionEvent(ObservableScrollState scrollState)
		{
			if (_observableScrollViewCallbacks != null)
				_observableScrollViewCallbacks.OnUpOrCancelMotionEvent(scrollState);
		}
	}

	public class ObservableScrollViewCallbacks : IObservableScrollViewCallbacks
	{
		private readonly MaterialViewPagerAnimator _animator;
		private readonly IObservableScrollViewCallbacks _observableScrollViewCallbacks;
		private readonly ObservableScrollView.ObservableScrollView _scrollView;
		private bool _firstZeroPassed;

		public ObservableScrollViewCallbacks(MaterialViewPagerAnimator materialViewPagerAnimator, ObservableScrollView.ObservableScrollView scrollView, IObservableScrollViewCallbacks observableScrollViewCallbacks)
		{
			_animator = materialViewPagerAnimator;
			_scrollView = scrollView;
			_observableScrollViewCallbacks = observableScrollViewCallbacks;
		}



		public void OnScrollChanged(int yOffset, bool b, bool b2)
		{
			if (_observableScrollViewCallbacks != null)
				_observableScrollViewCallbacks.OnScrollChanged(yOffset, b, b2);

			//first time you get 0, don't share it to others scrolls
			if (yOffset == 0 && !_firstZeroPassed)
			{
				_firstZeroPassed = true;
				return;
			}

			//only if yOffset changed
			if (_animator.IsNewYOffset(yOffset))
				_animator.OnMaterialScrolled(_scrollView, yOffset);
		}


		public void OnDownMotionEvent()
		{
			if (_observableScrollViewCallbacks != null)
				_observableScrollViewCallbacks.OnDownMotionEvent();
		}


		public void OnUpOrCancelMotionEvent(ObservableScrollState scrollState)
		{
			if (_observableScrollViewCallbacks != null)
				_observableScrollViewCallbacks.OnUpOrCancelMotionEvent(scrollState);
		}


	}

	public class RecyclerViewScrollListener : RecyclerView.OnScrollListener
	{
		private readonly MaterialViewPagerAnimator _animator;
		private readonly RecyclerView.OnScrollListener _onScrollListener;
		private bool _firstZeroPassed;

		public RecyclerViewScrollListener(MaterialViewPagerAnimator materialViewPagerAnimator, RecyclerView.OnScrollListener onScrollListener)
		{
			_animator = materialViewPagerAnimator;
			_onScrollListener = onScrollListener;
		}



		public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
		{
			base.OnScrollStateChanged(recyclerView, newState);
			if (_onScrollListener != null)
				_onScrollListener.OnScrollStateChanged(recyclerView, newState);
		}


		public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
		{
			base.OnScrolled(recyclerView, dx, dy);

			if (_onScrollListener != null)
				_onScrollListener.OnScrolled(recyclerView, dx, dy);

			int yOffset = _animator.YOffsets[recyclerView];

			yOffset += dy;
			_animator.YOffsets.AddOrUpdate(recyclerView, yOffset); //save the new offset

			//first time you get 0, don't share it to others scrolls
			if (yOffset == 0 && !_firstZeroPassed)
			{
				_firstZeroPassed = true;
				return;
			}

			//only if yOffset changed
			if (_animator.IsNewYOffset(yOffset))
				_animator.OnMaterialScrolled(recyclerView, yOffset);
		}
	}

	internal class AnimatorListener : Object, Animator.IAnimatorListener
	{
		private readonly MaterialViewPagerAnimator _animator;
		public AnimatorListener(MaterialViewPagerAnimator materialViewPagerAnimator)
		{
			_animator = materialViewPagerAnimator;
		}

		public void OnAnimationCancel(Animator p0)
		{

		}

		public void OnAnimationEnd(Animator p0)
		{
			_animator.FollowScrollToolbarIsVisible = true;
			_animator.FirstScrollValue = float.MinValue;
			_animator.JustToolbarAnimated = true;
		}

		public void OnAnimationRepeat(Animator p0)
		{

		}

		public void OnAnimationStart(Animator p0)
		{

		}
	}
}