using System;
using System.Linq;
using Android.Annotation;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using com.refractored;
using Java.Interop;
using MaterialViewPager.Header;
using Xamarin.NineOldAndroids.Views;
using Object = Java.Lang.Object;

namespace MaterialViewPager
{
	public class MaterialViewPager : FrameLayout, ViewPager.IOnPageChangeListener
	{

		/**
		 * the layout containing the header
		 * default : add @layout/material_view_pager_default_header
		 * with viewpager_header you can set your own layout
		 */
		private ViewGroup _headerBackgroundContainer;

		/**
		 * the layout containing tabs
		 * default : add @layout/material_view_pager_pagertitlestrip_standard
		 * with viewpager_pagerTitleStrip you can set your own layout
		 */
		private ViewGroup _pagerTitleStripContainer;

		/**
		 * the layout containing logo
		 * default : empty
		 * with viewpager_logo you can set your own layout
		 */
		private ViewGroup _logoContainer;

		/**
		 * Contains all references to MatervialViewPager's header views
		 */
		protected internal MaterialViewPagerHeader MaterialViewPagerHeader;

		//the child toolbar
		protected Toolbar MToolbar;

		//the child viewpager
		protected ViewPager MViewPager;

		//a view used to add placeholder color below the header
		protected View HeaderBackground;

		//a view used to add fading color over the headerBackgroundContainer
		protected View ToolbarLayoutBackground;

		//Class containing the configuration of the MaterialViewPager
		protected internal MaterialViewPagerSettings Settings = new MaterialViewPagerSettings();

		protected IListener Listener;

		public MaterialViewPager(Context context)
			: base(context)
		{

		}

		public MaterialViewPager(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			Settings.HandleAttributes(context, attrs);
		}

		public MaterialViewPager(Context context, IAttributeSet attrs, int defStyleAttr)
			: base(context, attrs, defStyleAttr)
		{
			Settings.HandleAttributes(context, attrs);
		}

		[TargetApi(Value = (int)BuildVersionCodes.Lollipop)]
		public MaterialViewPager(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
			: base(context, attrs, defStyleAttr, defStyleRes)
		{
			Settings.HandleAttributes(context, attrs);
		}

		protected override void OnDetachedFromWindow()
		{
			MaterialViewPagerHelper.Unregister(Context);
			Listener = null;
			base.OnDetachedFromWindow();
		}


		protected override void OnFinishInflate()
		{
			base.OnFinishInflate();

			//add @layout/material_view_pager_layout as child, containing all the MaterialViewPager views
			AddView(LayoutInflater.From(Context).Inflate(Resource.Layout.material_view_pager_layout, this, false));

			_headerBackgroundContainer = FindViewById<ViewGroup>(Resource.Id.headerBackgroundContainer);
			_pagerTitleStripContainer = FindViewById<ViewGroup>(Resource.Id.pagerTitleStripContainer);
			_logoContainer = FindViewById<ViewGroup>(Resource.Id.logoContainer);

			MToolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
			if (Settings.DisableToolbar)
				MToolbar.Visibility = ViewStates.Invisible;
			MViewPager = FindViewById<ViewPager>(Resource.Id.viewPager);

			MViewPager.AddOnPageChangeListener(this);

			//inflate subviews defined in attributes

			{
				int headerId = Settings.HeaderLayoutId;
				if (headerId == -1)
				{
					headerId = Settings.AnimatedHeaderImage ? Resource.Layout.material_view_pager_moving_header : Resource.Layout.material_view_pager_imageview_header;
				}
				_headerBackgroundContainer.AddView(LayoutInflater.From(Context).Inflate(headerId, _headerBackgroundContainer, false));
			}

			if (IsInEditMode)
			{ //preview titlestrip
				//add fake tabs on edit mode
				Settings.PagerTitleStripId = Resource.Layout.tools_material_view_pager_pagertitlestrip;
			}
			if (Settings.PagerTitleStripId != -1)
			{
				_pagerTitleStripContainer.AddView(LayoutInflater.From(Context).Inflate(Settings.PagerTitleStripId, _pagerTitleStripContainer, false));
			}

			if (Settings.LogoLayoutId != -1)
			{
				_logoContainer.AddView(LayoutInflater.From(Context).Inflate(Settings.LogoLayoutId, _logoContainer, false));
				if (Settings.LogoMarginTop != 0)
				{
					RelativeLayout.LayoutParams layoutParams = (RelativeLayout.LayoutParams)_logoContainer.LayoutParameters;
					layoutParams.SetMargins(0, Settings.LogoMarginTop, 0, 0);
					_logoContainer.LayoutParameters = layoutParams;
				}
			}

			HeaderBackground = FindViewById(Resource.Id.headerBackground);
			ToolbarLayoutBackground = FindViewById(Resource.Id.toolbar_layout_background);

			InitialiseHeights();

			//construct the materialViewPagerHeader with subviews
			if (!IsInEditMode)
			{
				MaterialViewPagerHeader = MaterialViewPagerHeader
						.WithToolbar(MToolbar)
						.WithToolbarLayoutBackground(ToolbarLayoutBackground)
						.WithPagerSlidingTabStrip(_pagerTitleStripContainer)
						.WithHeaderBackground(HeaderBackground)
						.WithStatusBackground(FindViewById(Resource.Id.statusBackground))
						.WithLogo(_logoContainer);

				//and construct the MaterialViewPagerAnimator
				//attach it to the activity to enable MaterialViewPagerHeaderView.setMaterialHeight();
				MaterialViewPagerHelper.Register(Context, new MaterialViewPagerAnimator(this));
			}
			else
			{

				//if in edit mode, add fake cardsviews
				View sample = LayoutInflater.From(Context).Inflate(Resource.Layout.tools_list_items, _pagerTitleStripContainer, false);

				LayoutParams param = (LayoutParams)sample.LayoutParameters;
				int marginTop = (int)Math.Round(Utils.DpToPx(Settings.HeaderHeight + 10, Context));
				param.SetMargins(0, marginTop, 0, 0);
				LayoutParameters = param;

				AddView(sample);
			}
		}

		private void InitialiseHeights()
		{
			if (HeaderBackground != null)
			{
				HeaderBackground.SetBackgroundColor(new Color(Settings.Color));

				ViewGroup.LayoutParams layoutParams = HeaderBackground.LayoutParameters;
				layoutParams.Height = (int)Utils.DpToPx(Settings.HeaderHeight + Settings.HeaderAdditionalHeight, Context);
				HeaderBackground.LayoutParameters = layoutParams;
			}
			if (_pagerTitleStripContainer != null)
			{
				RelativeLayout.LayoutParams layoutParams = (RelativeLayout.LayoutParams)_pagerTitleStripContainer.LayoutParameters;
				int marginTop = (int)Utils.DpToPx(Settings.HeaderHeight - 40, Context);
				layoutParams.SetMargins(0, marginTop, 0, 0);
				_pagerTitleStripContainer.LayoutParameters = (layoutParams);
			}
			if (ToolbarLayoutBackground != null)
			{
				ViewGroup.LayoutParams layoutParams = ToolbarLayoutBackground.LayoutParameters;
				layoutParams.Height = (int)Utils.DpToPx(Settings.HeaderHeight, Context);
				ToolbarLayoutBackground.LayoutParameters = (layoutParams);
			}
		}

		/**
		 * Retrieve the displayed viewpager, don't forget to use
		 * getPagerTitleStrip().setAdapter(materialviewpager.getViewPager())
		 * after set an adapter
		 *
		 * @return the displayed viewpager
		 */
		public ViewPager GetViewPager()
		{
			return MViewPager;
		}

		/**
		 * Retrieve the displayed tabs
		 *
		 * @return the displayed tabs
		 */
		public PagerSlidingTabStrip GetPagerTitleStrip()
		{
			return (PagerSlidingTabStrip)_pagerTitleStripContainer.FindViewById(Resource.Id.materialviewpager_pagerTitleStrip);
		}

		/**
		 * Retrieve the displayed toolbar
		 */
		public void SetToolbar(Toolbar toolbar)
		{
			MToolbar = toolbar;
		}

		/**
		 * Retrieve the displayed toolbar
		 *
		 * @return the displayed toolbar
		 */
		public Toolbar GetToolbar()
		{
			return MToolbar;
		}

		/**
		 * change the header displayed image with a fade
		 * may remove Picasso
		 */
		public void SetImageUrl(String imageUrl, int fadeDuration)
		{
			if (imageUrl != null)
			{
				ImageView headerBackgroundImage = FindViewById<ImageView>(Resource.Id.materialviewpager_imageHeader);
				//if using MaterialViewPagerImageHeader
				if (headerBackgroundImage != null)
				{
					ViewHelper.SetAlpha(headerBackgroundImage, Settings.HeaderAlpha);
					MaterialViewPagerImageHelper.SetImageUrl(headerBackgroundImage, imageUrl, fadeDuration);
				}
			}
		}

		/**
		 * change the header displayed image with a fade
		 * may remove Picasso
		 */
		public void SetImageDrawable(Drawable drawable, int fadeDuration)
		{
			if (drawable != null)
			{
				ImageView headerBackgroundImage = (ImageView)FindViewById(Resource.Id.materialviewpager_imageHeader);
				//if using MaterialViewPagerImageHeader
				if (headerBackgroundImage != null)
				{
					ViewHelper.SetAlpha(headerBackgroundImage, Settings.HeaderAlpha);
					MaterialViewPagerImageHelper.SetImageDrawable(headerBackgroundImage, drawable, fadeDuration);
				}
			}
		}

		/**
		 * Change the header color
		 */
		public void SetColor(int color, int fadeDuration)
		{
			MaterialViewPagerHelper.GetAnimator(Context).SetColor(color, fadeDuration * 2);
		}


		protected override IParcelable OnSaveInstanceState()
		{
			IParcelable superState = base.OnSaveInstanceState();

			SavedState ss = new SavedState(superState)
			{
				Settings = Settings,
				YOffset = MaterialViewPagerHelper.GetAnimator(Context).LastYOffset
			};
			//end

			return ss;
		}


		protected override void OnRestoreInstanceState(IParcelable state)
		{
			SavedState ss = (SavedState)state;
			base.OnRestoreInstanceState(ss.SuperState);

			Settings = ss.Settings;
			if (HeaderBackground != null)
				HeaderBackground.SetBackgroundColor(new Color(Settings.Color));

			MaterialViewPagerAnimator animator = MaterialViewPagerHelper.GetAnimator(Context);

			//-1*ss.yOffset restore to 0
			animator.RestoreScroll(-1 * ss.YOffset, ss.Settings);
			MaterialViewPagerHelper.Register(Context, animator);
		}

		public ViewGroup GetHeaderBackgroundContainer()
		{
			return _headerBackgroundContainer;
		}

		//region ViewPagerOnPageListener

		int _lastPosition = -1;


		public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
		{
			if (positionOffset >= 0.5)
			{
				OnPageSelected(position + 1);
			}
			else if (positionOffset <= -0.5)
			{
				OnPageSelected(position - 1);
			}
			else
			{
				OnPageSelected(position);
			}
		}

		public void NotifyHeaderChanged()
		{
			int position = _lastPosition;
			_lastPosition = -1;
			OnPageSelected(position);
		}


		public void OnPageSelected(int position)
		{
			if (position == _lastPosition || Listener == null)
				return;

			HeaderDesign headerDesign = Listener.GetHeaderDesign(position);
			if (headerDesign == null)
				return;

			int fadeDuration = 400;
			int color = headerDesign.Color;
			if (headerDesign.ColorRes != 0)
			{
				color = Context.Resources.GetColor(headerDesign.ColorRes);
			}

			if (headerDesign.Drawable != null)
			{
				SetImageDrawable(headerDesign.Drawable, fadeDuration);
			}
			else
			{
				SetImageUrl(headerDesign.ImageUrl, fadeDuration);
			}

			SetColor(color, fadeDuration);

			_lastPosition = position;
		}


		public void OnPageScrollStateChanged(int state)
		{
			if (Settings.DisplayToolbarWhenSwipe)
			{
				MaterialViewPagerHelper.GetAnimator(Context).OnViewPagerPageChanged();
			}
		}

		//endregion

		public void SetMaterialViewPagerListener(IListener listener)
		{
			Listener = listener;
		}

		public interface IListener
		{
			HeaderDesign GetHeaderDesign(int page);
		}

		public class SavedState : BaseSavedState
		{
			public MaterialViewPagerSettings Settings;
			public float YOffset;

			public SavedState(IParcelable superState)
				: base(superState)
			{

			}

			private SavedState(Parcel source)
				: base(source)
			{

				Settings = source.ReadParcelable(null) as MaterialViewPagerSettings;
				YOffset = source.ReadFloat();
			}


			public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
			{
				base.WriteToParcel(dest, flags);
				dest.WriteParcelable(Settings, flags);
				dest.WriteFloat(YOffset);
			}

			[ExportField("CREATOR")]
			// ReSharper disable once UnusedMember.Local
			static SavedStateCreator InititalizeCreator()
			{
				return new SavedStateCreator();
			}

			public class SavedStateCreator : Object, IParcelableCreator
			{
				public Object CreateFromParcel(Parcel source)
				{
					return new SavedState(source);
				}

				public Object[] NewArray(int size)
				{
					return (new SavedState[size]).Cast<Object>().ToArray();
				}
			}
		}
	}
}