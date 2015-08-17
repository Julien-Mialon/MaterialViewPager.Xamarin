using Android.Content;
using Android.Views;
using Android.Widget;
using Xamarin.NineOldAndroids.Views;

namespace MaterialViewPager
{
	public class MaterialViewPagerHeader
	{

		protected Context Context;

		protected View ToolbarLayout;
		protected Toolbar Toolbar;
		protected View MPagerSlidingTabStrip;

		protected View ToolbarLayoutBackground;
		protected View HeaderBackground;
		protected View StatusBackground;
		protected View MLogo;

		//positions used to animate views during scroll

		public float FinalTabsY;

		public float FinalTitleY;
		public float FinalTitleHeight;
		public float FinalTitleX;

		public float OriginalTitleY;
		public float OriginalTitleHeight;
		public float OriginalTitleX;
		public float FinalScale;

		private MaterialViewPagerHeader(Toolbar toolbar)
		{
			Toolbar = toolbar;
			Context = toolbar.Context;
			ToolbarLayout = (View)toolbar.Parent;
		}

		public static MaterialViewPagerHeader WithToolbar(Toolbar toolbar)
		{
			return new MaterialViewPagerHeader(toolbar);
		}

		public Context GetContext()
		{
			return Context;
		}

		public MaterialViewPagerHeader WithPagerSlidingTabStrip(View pagerSlidingTabStrip)
		{
			MPagerSlidingTabStrip = pagerSlidingTabStrip;

			MPagerSlidingTabStrip.ViewTreeObserver.PreDraw += PSTSViewTreeObserverOnPreDraw;

			return this;
		}

		private void PSTSViewTreeObserverOnPreDraw(object sender, ViewTreeObserver.PreDrawEventArgs preDrawEventArgs)
		{
			FinalTabsY = Utils.DpToPx(-2, Context);
			MPagerSlidingTabStrip.ViewTreeObserver.PreDraw -= PSTSViewTreeObserverOnPreDraw;
		}

		public MaterialViewPagerHeader WithHeaderBackground(View headerBackground)
		{
			HeaderBackground = headerBackground;
			return this;
		}

		public MaterialViewPagerHeader WithStatusBackground(View statusBackground)
		{
			StatusBackground = statusBackground;
			return this;
		}

		public MaterialViewPagerHeader WithToolbarLayoutBackground(View toolbarLayoutBackground)
		{
			ToolbarLayoutBackground = toolbarLayoutBackground;
			return this;
		}

		public int GetStatusBarHeight(Context context)
		{
			int result = 0;
			int resourceId = context.Resources.GetIdentifier("status_bar_height", "dimen", "android");
			if (resourceId > 0)
			{
				result = context.Resources.GetDimensionPixelSize(resourceId);
			}
			return result;
		}

		public MaterialViewPagerHeader WithLogo(View logo)
		{
			MLogo = logo;

			//when logo get a height, initialise initial & final logo positions
			ToolbarLayout.ViewTreeObserver.PreDraw += ToolbarViewTreeObserverOnPreDraw;

			return this;
		}

		private void ToolbarViewTreeObserverOnPreDraw(object sender, ViewTreeObserver.PreDrawEventArgs preDrawEventArgs)
		{
			//rotation fix, if not set, originalTitleY = Na
			ViewHelper.SetTranslationY(MLogo, 0);
			ViewHelper.SetTranslationX(MLogo, 0);

			OriginalTitleY = ViewHelper.GetY(MLogo);
			OriginalTitleX = ViewHelper.GetX(MLogo);

			OriginalTitleHeight = MLogo.Height;
			FinalTitleHeight = Utils.DpToPx(21, Context);

			//the final scale of the logo
			FinalScale = FinalTitleHeight / OriginalTitleHeight;

			FinalTitleY = (Toolbar.PaddingTop + Toolbar.Height) / 2f - FinalTitleHeight / 2 - (1 - FinalScale) * FinalTitleHeight;

			//(mLogo.getWidth()/2) *(1-finalScale) is the margin left added by the scale() on the logo
			//when logo scaledown, the content stay in center, so we have to anually remove the left padding
			FinalTitleX = Utils.DpToPx(52f, Context) - (MLogo.Width / 2f) * (1 - FinalScale);

			ToolbarLayout.ViewTreeObserver.PreDraw -= ToolbarViewTreeObserverOnPreDraw;
		}

		public Toolbar GetToolbar()
		{
			return Toolbar;
		}

		public View GetHeaderBackground()
		{
			return HeaderBackground;
		}

		public View GetStatusBackground()
		{
			return StatusBackground;
		}

		public View GetLogo()
		{
			return MLogo;
		}

	}
}