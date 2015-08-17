using System;
using Android.Annotation;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;

namespace MaterialViewPager
{
	public class MaterialViewPagerHeaderView : View
	{
		public MaterialViewPagerHeaderView(Context context)
			: base(context)
		{

		}

		public MaterialViewPagerHeaderView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{

		}

		public MaterialViewPagerHeaderView(Context context, IAttributeSet attrs, int defStyleAttr)
			: base(context, attrs, defStyleAttr)
		{

		}


		[TargetApi(Value = (int)BuildVersionCodes.Lollipop)]
		public MaterialViewPagerHeaderView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
			: base(context, attrs, defStyleAttr, defStyleRes)
		{

		}

		private void SetMaterialHeight()
		{
			//get the MaterialViewPagerAnimator attached to this activity
			//to retrieve the declared header height
			//and set it as current view height (+10dp margin)

			MaterialViewPagerAnimator animator = MaterialViewPagerHelper.GetAnimator(Context);
			if (animator != null)
			{
				ViewGroup.LayoutParams param = LayoutParameters;
				param.Height = (int)Math.Round(Utils.DpToPx(animator.getHeaderHeight() + 10, Context));
				LayoutParameters = param;
			}
		}

		protected override void OnFinishInflate()
		{
			base.OnFinishInflate();
			if (!IsInEditMode)
			{
				ViewTreeObserver.PreDraw += ViewTreeObserverOnPreDraw;
			}
		}

		private void ViewTreeObserverOnPreDraw(object sender, ViewTreeObserver.PreDrawEventArgs preDrawEventArgs)
		{
			SetMaterialHeight();
			ViewTreeObserver.PreDraw -= ViewTreeObserverOnPreDraw;
		}
	}
}