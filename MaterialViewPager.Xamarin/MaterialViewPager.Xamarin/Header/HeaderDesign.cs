using System;
using Android.Graphics.Drawables;

namespace MaterialViewPager.Header
{
	public class HeaderDesign
	{
		public int Color { get; protected set; }
		public int ColorRes { get; protected set; }
		public String ImageUrl { get; protected set; }
		public Drawable Drawable { get; protected set; }

		protected HeaderDesign()
		{

		}

		public static HeaderDesign FromColorAndUrl(int color, String imageUrl)
		{
			return new HeaderDesign
			{
				Color = color,
				ImageUrl = imageUrl
			};
		}

		public static HeaderDesign FromColorResAndUrl(int colorRes, String imageUrl)
		{
			return new HeaderDesign
			{
				ColorRes = colorRes,
				ImageUrl = imageUrl
			};
		}

		public static HeaderDesign FromColorAndDrawable(int color, Drawable drawable)
		{
			return new HeaderDesign
			{
				Drawable = drawable,
				Color = color
			};
		}

		public static HeaderDesign FromColorResAndDrawable(int colorRes, Drawable drawable)
		{
			return new HeaderDesign
			{
				ColorRes = colorRes,
				Drawable = drawable
			};
		}
	}
}