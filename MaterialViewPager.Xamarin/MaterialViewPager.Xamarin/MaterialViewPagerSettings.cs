using System.Linq;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Util;
using Java.Interop;
using Java.Lang;
using Exception = System.Exception;

namespace MaterialViewPager
{
	public class MaterialViewPagerSettings : Object, IParcelable
	{
		protected internal int HeaderLayoutId;
		protected internal int PagerTitleStripId;

		protected internal int LogoLayoutId;
		protected internal int LogoMarginTop;

		protected internal int HeaderAdditionalHeight;

		protected internal int HeaderHeight;
		protected internal int HeaderHeightPx;
		protected internal int Color;

		protected internal float HeaderAlpha;
		protected internal float ParallaxHeaderFactor;

		protected internal bool HideToolbarAndTitle;
		protected internal bool HideLogoWithFade;
		protected internal bool EnableToolbarElevation;
		protected internal bool DisplayToolbarWhenSwipe;
		protected internal bool ToolbarTransparent;
		protected internal bool AnimatedHeaderImage;
		protected internal bool DisableToolbar;

		/**
		 * Retrieve attributes from the MaterialViewPager
		 * @param context
		 * @param attrs
		 */

		public void HandleAttributes(Context context, IAttributeSet attrs)
		{
			try
			{
				TypedArray styledAttrs = context.ObtainStyledAttributes(attrs, Resource.Styleable.MaterialViewPager);
				
				{
					HeaderLayoutId = styledAttrs.GetResourceId(Resource.Styleable.MaterialViewPager_viewpager_header, -1);
				}
				{
					PagerTitleStripId = styledAttrs.GetResourceId(Resource.Styleable.MaterialViewPager_viewpager_pagerTitleStrip, -1);
					if (PagerTitleStripId == -1)
						PagerTitleStripId = Resource.Layout.material_view_pager_pagertitlestrip_standard;
				}
				{
					LogoLayoutId = styledAttrs.GetResourceId(Resource.Styleable.MaterialViewPager_viewpager_logo, -1);
					LogoMarginTop = styledAttrs.GetDimensionPixelSize(Resource.Styleable.MaterialViewPager_viewpager_logoMarginTop, 0);
				}
				{
					Color = styledAttrs.GetColor(Resource.Styleable.MaterialViewPager_viewpager_color, 0);
				}
				{
					HeaderHeightPx = styledAttrs.GetDimensionPixelOffset(Resource.Styleable.MaterialViewPager_viewpager_headerHeight, 200);
					HeaderHeight = Math.Round(Utils.PxToDp(HeaderHeightPx, context)); //convert to dp
				}
				{
					HeaderAdditionalHeight = styledAttrs.GetDimensionPixelOffset(Resource.Styleable.MaterialViewPager_viewpager_headerAdditionalHeight, 60);
				}
				{
					HeaderAlpha = styledAttrs.GetFloat(Resource.Styleable.MaterialViewPager_viewpager_headerAlpha, 0.5f);
				}
				{
					ParallaxHeaderFactor = styledAttrs.GetFloat(Resource.Styleable.MaterialViewPager_viewpager_parallaxHeaderFactor, 1.5f);
					ParallaxHeaderFactor = Math.Max(ParallaxHeaderFactor, 1); //min=1
				}
				{
					HideToolbarAndTitle = styledAttrs.GetBoolean(Resource.Styleable.MaterialViewPager_viewpager_hideToolbarAndTitle, false);
					HideLogoWithFade = styledAttrs.GetBoolean(Resource.Styleable.MaterialViewPager_viewpager_hideLogoWithFade, false);
				}
				{
					EnableToolbarElevation = styledAttrs.GetBoolean(Resource.Styleable.MaterialViewPager_viewpager_enableToolbarElevation, false);
				}
				{
					DisplayToolbarWhenSwipe = styledAttrs.GetBoolean(Resource.Styleable.MaterialViewPager_viewpager_displayToolbarWhenSwipe, false);
				}
				{
					ToolbarTransparent = styledAttrs.GetBoolean(Resource.Styleable.MaterialViewPager_viewpager_transparentToolbar, false);
				}
				{
					AnimatedHeaderImage = styledAttrs.GetBoolean(Resource.Styleable.MaterialViewPager_viewpager_animatedHeaderImage, true);
				}
				{
					DisableToolbar = styledAttrs.GetBoolean(Resource.Styleable.MaterialViewPager_viewpager_disableToolbar, false);
				}
				styledAttrs.Recycle();
			}
			catch (Exception)
			{
				// ignored
			}
		}

		//region parcelable

		public int DescribeContents()
		{
			return 0;
		}

		public void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
		{
			dest.WriteInt(HeaderLayoutId);
			dest.WriteInt(PagerTitleStripId);
			dest.WriteInt(LogoLayoutId);
			dest.WriteInt(LogoMarginTop);
			dest.WriteInt(HeaderAdditionalHeight);
			dest.WriteInt(HeaderHeight);
			dest.WriteInt(HeaderHeightPx);
			dest.WriteInt(Color);
			dest.WriteFloat(HeaderAlpha);
			dest.WriteFloat(ParallaxHeaderFactor);
			dest.WriteByte(HideToolbarAndTitle ? (sbyte)1 : (sbyte)0);
			dest.WriteByte(HideLogoWithFade ? (sbyte)1 : (sbyte)0);
			dest.WriteByte(EnableToolbarElevation ? (sbyte)1 : (sbyte)0);
		}

		public MaterialViewPagerSettings()
		{
		}

		private MaterialViewPagerSettings(Parcel input)
		{
			HeaderLayoutId = input.ReadInt();
			PagerTitleStripId = input.ReadInt();
			LogoLayoutId = input.ReadInt();
			LogoMarginTop = input.ReadInt();
			HeaderAdditionalHeight = input.ReadInt();
			HeaderHeight = input.ReadInt();
			HeaderHeightPx = input.ReadInt();
			Color = input.ReadInt();
			HeaderAlpha = input.ReadFloat();
			ParallaxHeaderFactor = input.ReadFloat();
			HideToolbarAndTitle = input.ReadByte() != 0;
			HideLogoWithFade = input.ReadByte() != 0;
			EnableToolbarElevation = input.ReadByte() != 0;
		}

		[ExportField("CREATOR")]
		// ReSharper disable once UnusedMember.Local
		static MaterialViewPagerSettingsCreator InititalizeCreator()
		{
			return new MaterialViewPagerSettingsCreator();
		}

		public class MaterialViewPagerSettingsCreator : Object, IParcelableCreator
		{
			public Object CreateFromParcel(Parcel source)
			{
				return new MaterialViewPagerSettings(source);
			}

			public Object[] NewArray(int size)
			{
				return (new MaterialViewPagerSettings[size]).Cast<Object>().ToArray();
			}
		}
	}

}