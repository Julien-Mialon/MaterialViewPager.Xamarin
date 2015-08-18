using System.Diagnostics;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Net;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Math = System.Math;

namespace KenBurnsView
{
	public class KenBurnsView : ImageView
	{

		/** Delay between a pair of frames at a 60 FPS frame rate. */
		private const long FRAME_DELAY = 1000 / 60;

		/** Matrix used to perform all the necessary transition transformations. */
		private readonly Matrix _mMatrix = new Matrix();

		/** The {@link TransitionGenerator} implementation used to perform the transitions between
		 *  rects. The default {@link TransitionGenerator} is {@link RandomTransitionGenerator}. */
		private ITransitionGenerator _mTransGen = new RandomTransitionGenerator();

		/** A {@link KenBurnsView.ITransitionListener} to be notified when
		 *  a transition starts or ends. */
		private ITransitionListener _mTransitionListener;

		/** The ongoing transition. */
		private Transition _mCurrentTrans;

		/** The rect that holds the bounds of this view. */
		private readonly RectF _mViewportRect = new RectF();
		/** The rect that holds the bounds of the current {@link Drawable}. */
		private RectF _mDrawableRect;

		/** The progress of the animation, in milliseconds. */
		private long _mElapsedTime;

		/** The time, in milliseconds, of the last animation frame.
		 * This is useful to increment {@link #mElapsedTime} regardless
		 * of the amount of time the animation has been paused. */
		private long _mLastFrameTime;

		/** Controls whether the the animation is running. */
		private bool _mPaused;

		/** Indicates whether the parent constructor was already called.
		 * This is needed to distinguish if the image is being set before
		 * or after the super class constructor returns. */
		private readonly bool _mInitialized;


		public KenBurnsView(Context context)
			: this(context, null)
		{
		}


		public KenBurnsView(Context context, IAttributeSet attrs)
			: this(context, attrs, 0)
		{

		}


		public KenBurnsView(Context context, IAttributeSet attrs, int defStyle)
			: base(context, attrs, defStyle)
		{
			_mInitialized = true;
			// Attention to the super call here!
			base.SetScaleType(ScaleType.Matrix);
		}



		public override void SetScaleType(ScaleType scaleType)
		{
			// It'll always be center-cropped by default.
		}

		public override ViewStates Visibility
		{
			get { return base.Visibility; }
			set
			{
				base.Visibility = value;
				switch (value)
				{
					case ViewStates.Visible:
						Resume();
						break;
					default:
						Pause();
						break;
				}
			}
		}



		public override void SetImageBitmap(Bitmap bm)
		{
			base.SetImageBitmap(bm);
			HandleImageChange();
		}



		public override void SetImageResource(int resId)
		{
			base.SetImageResource(resId);
			HandleImageChange();
		}



		public override void SetImageURI(Uri uri)
		{
			base.SetImageURI(uri);
			HandleImageChange();
		}



		public override void SetImageDrawable(Drawable drawable)
		{
			base.SetImageDrawable(drawable);
			HandleImageChange();
		}



		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);
			Restart();
		}



		protected override void OnDraw(Canvas canvas)
		{
			Drawable d = Drawable;
			if (!_mPaused && d != null)
			{
				if (_mDrawableRect.IsEmpty)
				{
					UpdateDrawableBounds();
				}
				else if (HasBounds())
				{
					if (_mCurrentTrans == null)
					{ // Starting the first transition.
						StartNewTransition();
					}

					Debug.Assert(_mCurrentTrans != null, "mCurrentTrans != null");
					if (_mCurrentTrans.GetDestinyRect() != null)
					{ // If null, it's supposed to stop.
						_mElapsedTime += JavaSystem.CurrentTimeMillis() - _mLastFrameTime;
						RectF currentRect = _mCurrentTrans.GetInterpolatedRect(_mElapsedTime);

						float widthScale = _mDrawableRect.Width() / currentRect.Width();
						float heightScale = _mDrawableRect.Height() / currentRect.Height();
						// Scale to make the current rect match the smallest drawable dimension.
						float currRectToDrwScale = Math.Min(widthScale, heightScale);
						// Scale to make the current rect match the viewport bounds.
						float currRectToVpScale = _mViewportRect.Width() / currentRect.Width();
						// Combines the two scales to fill the viewport with the current rect.
						float totalScale = currRectToDrwScale * currRectToVpScale;

						float translX = totalScale * (_mDrawableRect.CenterX() - currentRect.Left);
						float translY = totalScale * (_mDrawableRect.CenterY() - currentRect.Top);

						/* Performs matrix transformations to fit the content
						   of the current rect into the entire view. */
						_mMatrix.Reset();
						_mMatrix.PostTranslate(-_mDrawableRect.Width() / 2, -_mDrawableRect.Height() / 2);
						_mMatrix.PostScale(totalScale, totalScale);
						_mMatrix.PostTranslate(translX, translY);

						ImageMatrix = _mMatrix;

						// Current transition is over. It's time to start a new one.
						if (_mElapsedTime >= _mCurrentTrans.GetDuration())
						{
							FireTransitionEnd(_mCurrentTrans);
							StartNewTransition();
						}
					}
					else
					{ // Stopping? A stop event has to be fired.
						FireTransitionEnd(_mCurrentTrans);
					}
				}
				_mLastFrameTime = JavaSystem.CurrentTimeMillis();
				PostInvalidateDelayed(FRAME_DELAY);
			}
			base.OnDraw(canvas);
		}


		/**
		 * Generates and starts a transition.
		 */
		private void StartNewTransition()
		{
			if (!HasBounds())
			{
				return; // Can't start transition if the drawable has no bounds.
			}
			_mCurrentTrans = _mTransGen.GenerateNextTransition(_mDrawableRect, _mViewportRect);
			_mElapsedTime = 0;
			_mLastFrameTime = JavaSystem.CurrentTimeMillis();
			FireTransitionStart(_mCurrentTrans);
		}


		/**
		 * Creates a new transition and starts over.
		 */
		public void Restart()
		{
			int width = Width;
			int height = Height;

			if (width == 0 || height == 0)
			{
				return; // Can't call restart() when view area is zero.
			}

			UpdateViewport(width, height);
			UpdateDrawableBounds();

			StartNewTransition();
		}


		/**
		 * Checks whether this view has bounds.
		 * @return
		 */
		private bool HasBounds()
		{
			return !_mViewportRect.IsEmpty;
		}


		/**
		 * Fires a start event on {@link #mTransitionListener};
		 * @param transition the transition that just started.
		 */
		private void FireTransitionStart(Transition transition)
		{
			if (_mTransitionListener != null && transition != null)
			{
				_mTransitionListener.OnTransitionStart(transition);
			}
		}


		/**
		 * Fires an end event on {@link #mTransitionListener};
		 * @param transition the transition that just ended.
		 */
		private void FireTransitionEnd(Transition transition)
		{
			if (_mTransitionListener != null && transition != null)
			{
				_mTransitionListener.OnTransitionEnd(transition);
			}
		}


		/**
		 * Sets the {@link TransitionGenerator} to be used in animations.
		 * @param transgen the {@link TransitionGenerator} to be used in animations.
		 */
		public void SetTransitionGenerator(ITransitionGenerator transgen)
		{
			_mTransGen = transgen;
			StartNewTransition();
		}


		/**
		 * Updates the viewport rect. This must be called every time the size of this view changes.
		 * @param width the new viewport with.
		 * @param height the new viewport height.
		 */
		private void UpdateViewport(float width, float height)
		{
			_mViewportRect.Set(0, 0, width, height);
		}


		/**
		 * Updates the drawable bounds rect. This must be called every time the drawable
		 * associated to this view changes.
		 */
		private void UpdateDrawableBounds()
		{
			if (_mDrawableRect == null)
			{
				_mDrawableRect = new RectF();
			}
			Drawable d = Drawable;
			if (d != null && d.IntrinsicHeight > 0 && d.IntrinsicWidth > 0)
			{
				_mDrawableRect.Set(0, 0, d.IntrinsicWidth, d.IntrinsicHeight);
			}
		}


		/**
		 * This method is called every time the underlying image
		 * is changed.
		 */
		private void HandleImageChange()
		{
			UpdateDrawableBounds();
			/* Don't start a new transition if this event
			 was fired during the super constructor execution.
			 The view won't be ready at this time. Also,
			 don't start it if this view size is still unknown. */
			if (_mInitialized)
			{
				StartNewTransition();
			}
		}


		public void SetTransitionListener(ITransitionListener transitionListener)
		{
			_mTransitionListener = transitionListener;
		}


		/**
		 * Pauses the Ken Burns Effect animation.
		 */
		public void Pause()
		{
			_mPaused = true;
		}


		/**
		 * Resumes the Ken Burns Effect animation.
		 */
		public void Resume()
		{
			_mPaused = false;
			// This will make the animation to continue from where it stopped.
			_mLastFrameTime = JavaSystem.CurrentTimeMillis();
			Invalidate();
		}
	}
}