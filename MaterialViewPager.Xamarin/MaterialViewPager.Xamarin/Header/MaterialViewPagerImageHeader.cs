using System;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Util;

namespace MaterialViewPager.Header
{
	public class MaterialViewPagerImageHeader : KenBurnsView.KenBurnsView
	{
		public MaterialViewPagerImageHeader(Context context)
			: base(context)
		{

		}

		public MaterialViewPagerImageHeader(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{

		}

		public MaterialViewPagerImageHeader(Context context, IAttributeSet attrs, int defStyle)
			: base(context, attrs, defStyle)
		{

		}

		public void SetImageUrl(String urlImage, int fadeDuration)
		{
			MaterialViewPagerImageHelper.SetImageUrl(this, urlImage, fadeDuration);
		}

		public void SetImageDrawable(Drawable drawable, int fadeDuration)
		{
			MaterialViewPagerImageHelper.SetImageDrawable(this, drawable, fadeDuration);
		}
	}
}