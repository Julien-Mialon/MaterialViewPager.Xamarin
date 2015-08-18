using System;
using Android.Animation;
using Android.Graphics.Drawables;
using Android.Views.Animations;
using Android.Widget;
using Square.Picasso;
using Xamarin.NineOldAndroids.Views;

namespace MaterialViewPager.Header
{
	public class MaterialViewPagerImageHelper
	{

		/**
		 * change the image with a fade
		 * @param urlImage
		 * @param fadeDuration
		 *
		 * TODO : remove Picasso
		 */
		public static void SetImageUrl(ImageView imageView, String urlImage, int fadeDuration)
		{
			float alpha = ViewHelper.GetAlpha(imageView);
			ImageView viewToAnimate = imageView;

			//fade to alpha=0
			ObjectAnimator fadeOut = ObjectAnimator.OfFloat(viewToAnimate, "alpha", 0);
			fadeOut.SetDuration(fadeDuration);
			fadeOut.SetInterpolator(new DecelerateInterpolator());
			fadeOut.AnimationEnd += (sender, args) =>
			{
				//change the image when alpha=0
				Picasso.With(imageView.Context).Load(urlImage).CenterCrop().Fit().Into(viewToAnimate, () =>
				{
					//then fade to alpha=1
					ObjectAnimator fadeIn = ObjectAnimator.OfFloat(viewToAnimate, "alpha", alpha);
					fadeIn.SetDuration(fadeDuration);
					fadeIn.SetInterpolator(new AccelerateInterpolator());
					fadeIn.Start();
				}, () => { });
			};
			fadeOut.Start();
		}

		/**
		 * change the image with a fade
		 * @param drawable
		 * @param fadeDuration
		 */
		public static void SetImageDrawable(ImageView imageView, Drawable drawable, int fadeDuration)
		{
			float alpha = ViewHelper.GetAlpha(imageView);
			ImageView viewToAnimate = imageView;

			//fade to alpha=0
			ObjectAnimator fadeOut = ObjectAnimator.OfFloat(viewToAnimate, "alpha", 0);
			fadeOut.SetDuration(fadeDuration);
			fadeOut.SetInterpolator(new DecelerateInterpolator());
			fadeOut.AnimationEnd += (sender, args) =>
			{
				//change the image when alpha=0
				imageView.SetImageDrawable(drawable);

				//then fade to alpha=1
				ObjectAnimator fadeIn = ObjectAnimator.OfFloat(viewToAnimate, "alpha", alpha);
				fadeIn.SetDuration(fadeDuration);
				fadeIn.SetInterpolator(new AccelerateInterpolator());
				fadeIn.Start();
			};
			fadeOut.Start();
		}
	}
}