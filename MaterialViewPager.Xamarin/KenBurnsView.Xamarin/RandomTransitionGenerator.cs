using System;
using Android.Graphics;
using Android.Views.Animations;
using Java.Lang;

namespace KenBurnsView
{
	public class RandomTransitionGenerator : ITransitionGenerator
	{

		/** Default value for the transition duration in milliseconds. */
		public static int DefaultTransitionDuration = 10000;

		/** Minimum rect dimension factor, according to the maximum one. */
		private const float MIN_RECT_FACTOR = 0.75f;

		/** Random object used to generate arbitrary rects. */
		private readonly Random _mRandom = new Random(DateTime.Now.Millisecond);

		/** The duration, in milliseconds, of each transition. */
		private long _mTransitionDuration;

		/** The {@link IInterpolator} to be used to create transitions. */
		private IInterpolator _mTransitionIInterpolator;

		/** The last generated transition. */
		private Transition _mLastGenTrans;

		/** The bounds of the drawable when the last transition was generated. */
		private RectF _mLastDrawableBounds;


		public RandomTransitionGenerator()
			: this(DefaultTransitionDuration, new AccelerateDecelerateInterpolator())
		{
		}


		public RandomTransitionGenerator(long transitionDuration, IInterpolator transitionIInterpolator)
		{
			SetTransitionDuration(transitionDuration);
			SetTransitionIInterpolator(transitionIInterpolator);
		}

		public Transition GenerateNextTransition(RectF drawableBounds, RectF viewport)
		{
			bool firstTransition = _mLastGenTrans == null;
			bool drawableBoundsChanged = true;
			bool viewportRatioChanged = true;

			RectF srcRect = null;
			RectF dstRect = null;

			if (!firstTransition)
			{
				dstRect = _mLastGenTrans.GetDestinyRect();
				drawableBoundsChanged = !drawableBounds.Equals(_mLastDrawableBounds);
				viewportRatioChanged = !MathUtils.HaveSameAspectRatio(dstRect, viewport);
			}

			if (dstRect == null || drawableBoundsChanged || viewportRatioChanged)
			{
				srcRect = GenerateRandomRect(drawableBounds, viewport);
			}
			else
			{
				/* Sets the destiny rect of the last transition as the source one
				 if the current drawable has the same dimensions as the one of
				 the last transition. */
				srcRect = dstRect;
			}
			dstRect = GenerateRandomRect(drawableBounds, viewport);

			_mLastGenTrans = new Transition(srcRect, dstRect, _mTransitionDuration,
					_mTransitionIInterpolator);
			_mLastDrawableBounds = drawableBounds;

			return _mLastGenTrans;
		}


		/**
		 * Generates a random rect that can be fully contained within {@code drawableBounds} and
		 * has the same aspect ratio of {@code viewportRect}. The dimensions of this random rect
		 * won't be higher than the largest rect with the same aspect ratio of {@code viewportRect}
		 * that {@code drawableBounds} can contain. They also won't be lower than the dimensions
		 * of this upper rect limit weighted by {@code MIN_RECT_FACTOR}.
		 * @param drawableBounds the bounds of the drawable that will be zoomed and panned.
		 * @param viewportRect the bounds of the view that the drawable will be shown.
		 * @return an arbitrary generated rect with the same aspect ratio of {@code viewportRect}
		 * that will be contained within {@code drawableBounds}.
		 */
		private RectF GenerateRandomRect(RectF drawableBounds, RectF viewportRect)
		{
			float drawableRatio = MathUtils.GetRectRatio(drawableBounds);
			float viewportRectRatio = MathUtils.GetRectRatio(viewportRect);
			RectF maxCrop;

			if (drawableRatio > viewportRectRatio)
			{
				float r = (drawableBounds.Height() / viewportRect.Height()) * viewportRect.Width();
				float b = drawableBounds.Height();
				maxCrop = new RectF(0, 0, r, b);
			}
			else
			{
				float r = drawableBounds.Width();
				float b = (drawableBounds.Width() / viewportRect.Width()) * viewportRect.Height();
				maxCrop = new RectF(0, 0, r, b);
			}

			float randomFloat = MathUtils.Truncate((float)_mRandom.NextDouble(), 2);
			float factor = MIN_RECT_FACTOR + ((1 - MIN_RECT_FACTOR) * randomFloat);

			float width = factor * maxCrop.Width();
			float height = factor * maxCrop.Height();
			int widthDiff = (int)(drawableBounds.Width() - width);
			int heightDiff = (int)(drawableBounds.Height() - height);
			int left = widthDiff > 0 ? _mRandom.Next(widthDiff) : 0;
			int top = heightDiff > 0 ? _mRandom.Next(heightDiff) : 0;
			return new RectF(left, top, left + width, top + height);
		}


		/**
		 * Sets the duration, in milliseconds, for each transition generated.
		 * @param transitionDuration the transition duration.
		 */
		public void SetTransitionDuration(long transitionDuration)
		{
			_mTransitionDuration = transitionDuration;
		}


		/**
		 * Sets the {@link IInterpolator} for each transition generated.
		 * @param IInterpolator the transition IInterpolator.
		 */
		public void SetTransitionIInterpolator(IInterpolator interpolator)
		{
			_mTransitionIInterpolator = interpolator;
		}
	}
}