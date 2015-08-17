using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Xamarin.NineOldAndroids.Views;

namespace MaterialViewPager
{
	public static class Utils
	{
		/**
		 * convert dp to px
		 */
		public static float DpToPx(float dp, Context context)
		{
			return dp * context.Resources.DisplayMetrics.Density;
		}

		/**
		 * convert px to dp
		 */
		public static float PxToDp(float px, Context context)
		{
			return px / context.Resources.DisplayMetrics.Density;
		}

		/*
		 * Create a color from [$color].RGB and then add an alpha with 255*[$percent]
		 */
		public static int ColorWithAlpha(int color, float percent)
		{
			int r = Color.GetRedComponent(color);
			int g = Color.GetGreenComponent(color);
			int b = Color.GetBlueComponent(color);
			int alpha = (int)Math.Round(percent * 255);

			return Color.Argb(alpha, r, g, b);
		}

		public static float MinMax(float min, float value, float max)
		{
			return Math.Max(min, Math.Min(value, max));
		}

		public static void SetScale(float scale, params View[] views)
		{
			views.Where(x => x != null).Apply(x =>
			{
				ViewHelper.SetScaleX(x, scale);
				ViewHelper.SetScaleY(x, scale);
			});
		}

		public static void SetElevation(float elevation, params View[] views)
		{
			views.Where(x => x != null).Apply(x => ViewCompat.SetElevation(x, elevation));
		}

		public static void SetBackgroundColor(int color, params View[] views)
		{
			Color colorObject = new Color(color);
			views.Where(x => x != null).Apply(x => x.SetBackgroundColor(colorObject));
		}

		public static bool CanScroll(View view)
		{
			ScrollView scrollView = view as ScrollView;
			if (scrollView != null)
			{
				View child = scrollView.GetChildAt(0);
				if (child == null)
				{
					return false;
				}

				return scrollView.Height < child.Height + scrollView.PaddingTop + scrollView.PaddingBottom;
			}
			RecyclerView recyclerView = view as RecyclerView;
			if (recyclerView != null)
			{
				return recyclerView.ComputeVerticalScrollOffset() != 0;
			}
			return true;
		}

		public static void ScrollTo(object scroll, float yOffset)
		{
			// If RecyclerView
			RecyclerView recyclerView = scroll as RecyclerView;
			if (recyclerView != null)
			{
				//RecyclerView.scrollTo : UnsupportedOperationException
				//Moved to the RecyclerView.LayoutManager.scrollToPositionWithOffset
				//Have to be is RecyclerView.LayoutManager to work (so work with RecyclerView.GridLayoutManager)
				RecyclerView.LayoutManager layoutManager = recyclerView.GetLayoutManager();

				LinearLayoutManager linearLayoutManager = layoutManager as LinearLayoutManager;
				if (linearLayoutManager != null)
				{
					linearLayoutManager.ScrollToPositionWithOffset(0, (int)-yOffset);
				}
				else
				{
					StaggeredGridLayoutManager staggeredGridLayoutManager = layoutManager as StaggeredGridLayoutManager;
					if (staggeredGridLayoutManager != null)
					{
						staggeredGridLayoutManager.ScrollToPositionWithOffset(0, (int) -yOffset);
					}
				}
				return;
			}

			// If ScrollView
			ScrollView scrollView = scroll as ScrollView;
			if (scrollView != null)
			{
				scrollView.ScrollTo(0, (int)yOffset);
				return;
			}

			// If ListView
			ListView listView = scroll as ListView;
			if (listView != null)
			{
				listView.ScrollTo(0, (int)yOffset);
				return;
			}

			// If WebView
			WebView webView = scroll as WebView;
			if (webView != null)
			{
				webView.ScrollTo(0, (int)yOffset);
			}
		}

		public static View GetTheVisibileView(List<View> viewList)
		{
			Rect scrollBounds = new Rect();

			return viewList.FirstOrDefault(x =>
			{
				x.GetHitRect(scrollBounds);
				return x.GetLocalVisibleRect(scrollBounds);
			});
		}
	}
}