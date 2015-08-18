using Android.Annotation;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace ObservableScrollView
{
	public class TouchInterceptionFrameLayout : FrameLayout
	{
		/**
		 * Callbacks for TouchInterceptionFrameLayout.
		 */
		public interface ITouchInterceptionListener
		{
			/**
			 * Determines whether the layout should intercept this event.
			 *
			 * @param ev     motion event
			 * @param moving true if this event is ACTION_MOVE type
			 * @param diffX  difference between previous X and current X, if moving is true
			 * @param diffY  difference between previous Y and current Y, if moving is true
			 * @return true if the layout should intercept
			 */
			bool ShouldInterceptTouchEvent(MotionEvent ev, bool moving, float diffX, float diffY);

			/**
			 * Called if the down motion event is intercepted by this layout.
			 *
			 * @param ev motion event
			 */
			void OnDownMotionEvent(MotionEvent ev);

			/**
			 * Called if the move motion event is intercepted by this layout.
			 *
			 * @param ev    motion event
			 * @param diffX difference between previous X and current X
			 * @param diffY difference between previous Y and current Y
			 */
			void OnMoveMotionEvent(MotionEvent ev, float diffX, float diffY);

			/**
			 * Called if the up (or cancel) motion event is intercepted by this layout.
			 *
			 * @param ev motion event
			 */
			void OnUpOrCancelMotionEvent(MotionEvent ev);
		}

		private bool _mIntercepting;
		private bool _mDownMotionEventPended;
		private bool _mBeganFromDownMotionEvent;
		private bool _mChildrenEventsCanceled;
		private PointF _mInitialPoint;
		private MotionEvent _mPendingDownMotionEvent;
		private ITouchInterceptionListener _mTouchInterceptionListener;

		public TouchInterceptionFrameLayout(Context context)
			: base(context)
		{

		}

		public TouchInterceptionFrameLayout(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{

		}

		public TouchInterceptionFrameLayout(Context context, IAttributeSet attrs, int defStyleAttr)
			: base(context, attrs, defStyleAttr)
		{

		}

		[TargetApi(Value = (int)BuildVersionCodes.Lollipop)]
		public TouchInterceptionFrameLayout(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
			: base(context, attrs, defStyleAttr, defStyleRes)
		{

		}

		public void SetScrollInterceptionListener(ITouchInterceptionListener listener)
		{
			_mTouchInterceptionListener = listener;
		}


		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			if (_mTouchInterceptionListener == null)
			{
				return false;
			}

			// In here, we must initialize touch state variables
			// and ask if we should intercept this event.
			// Whether we should intercept or not is kept for the later event handling.
			switch (ev.ActionMasked)
			{
				case MotionEventActions.Down:
					_mInitialPoint = new PointF(ev.GetX(), ev.GetY());
					_mPendingDownMotionEvent = MotionEvent.ObtainNoHistory(ev);
					_mDownMotionEventPended = true;
					_mIntercepting = _mTouchInterceptionListener.ShouldInterceptTouchEvent(ev, false, 0, 0);
					_mBeganFromDownMotionEvent = _mIntercepting;
					_mChildrenEventsCanceled = false;
					return _mIntercepting;
				case MotionEventActions.Move:
					// ACTION_MOVE will be passed suddenly, so initialize to avoid exception.
					if (_mInitialPoint == null)
					{
						_mInitialPoint = new PointF(ev.GetX(), ev.GetY());
					}

					// diffX and diffY are the origin of the motion, and should be difference
					// from the position of the ACTION_DOWN event occurred.
					float diffX = ev.GetX() - _mInitialPoint.X;
					float diffY = ev.GetY() - _mInitialPoint.Y;
					_mIntercepting = _mTouchInterceptionListener.ShouldInterceptTouchEvent(ev, true, diffX, diffY);
					return _mIntercepting;
			}
			return false;
		}


		public override bool OnTouchEvent(MotionEvent ev)
		{
			if (_mTouchInterceptionListener != null)
			{
				switch (ev.ActionMasked)
				{
					case MotionEventActions.Down:
						if (_mIntercepting)
						{
							_mTouchInterceptionListener.OnDownMotionEvent(ev);
							DuplicateTouchEventForChildren(ev);
							return true;
						}
						break;
					case MotionEventActions.Move:
						// ACTION_MOVE will be passed suddenly, so initialize to avoid exception.
						if (_mInitialPoint == null)
						{
							_mInitialPoint = new PointF(ev.GetX(), ev.GetY());
						}

						// diffX and diffY are the origin of the motion, and should be difference
						// from the position of the ACTION_DOWN event occurred.
						float diffX = ev.GetX() - _mInitialPoint.X;
						float diffY = ev.GetY() - _mInitialPoint.Y;
						_mIntercepting = _mTouchInterceptionListener.ShouldInterceptTouchEvent(ev, true, diffX, diffY);
						if (_mIntercepting)
						{
							// If this layout didn't receive ACTION_DOWN motion event,
							// we should generate ACTION_DOWN event with current position.
							if (!_mBeganFromDownMotionEvent)
							{
								_mBeganFromDownMotionEvent = true;

								MotionEvent event2 = MotionEvent.ObtainNoHistory(_mPendingDownMotionEvent);
								event2.SetLocation(ev.GetX(), ev.GetY());
								_mTouchInterceptionListener.OnDownMotionEvent(event2);

								_mInitialPoint = new PointF(ev.GetX(), ev.GetY());
								diffX = diffY = 0;
							}

							// Children's touches should be canceled
							if (!_mChildrenEventsCanceled)
							{
								_mChildrenEventsCanceled = true;
								DuplicateTouchEventForChildren(ObtainMotionEvent(ev, MotionEventActions.Cancel));
							}

							_mTouchInterceptionListener.OnMoveMotionEvent(ev, diffX, diffY);

							// If next mIntercepting become false,
							// then we should generate fake ACTION_DOWN event.
							// Therefore we set pending flag to true as if this is a down motion event.
							_mDownMotionEventPended = true;

							// Whether or not this event is consumed by the listener,
							// assume it consumed because we declared to intercept the event.
							return true;
						}
						if (_mDownMotionEventPended)
						{
							_mDownMotionEventPended = false;
							MotionEvent event2 = MotionEvent.ObtainNoHistory(_mPendingDownMotionEvent);
							event2.SetLocation(ev.GetX(), ev.GetY());
							DuplicateTouchEventForChildren(ev, event2);
						}
						else
						{
							DuplicateTouchEventForChildren(ev);
						}

						// If next mIntercepting become true,
						// then we should generate fake ACTION_DOWN event.
						// Therefore we set beganFromDownMotionEvent flag to false
						// as if we haven't received a down motion event.
						_mBeganFromDownMotionEvent = false;

						// Reserve children's click cancellation here if they've already canceled
						_mChildrenEventsCanceled = false;
						break;
					case MotionEventActions.Up:
					case MotionEventActions.Cancel:
						_mBeganFromDownMotionEvent = false;
						if (_mIntercepting)
						{
							_mTouchInterceptionListener.OnUpOrCancelMotionEvent(ev);
						}

						// Children's touches should be canceled regardless of
						// whether or not this layout intercepted the consecutive motion events.
						if (!_mChildrenEventsCanceled)
						{
							_mChildrenEventsCanceled = true;
							if (_mDownMotionEventPended)
							{
								_mDownMotionEventPended = false;
								MotionEvent event2 = MotionEvent.ObtainNoHistory(_mPendingDownMotionEvent);
								event2.SetLocation(ev.GetX(), ev.GetY());
								DuplicateTouchEventForChildren(ev, event2);
							}
							else
							{
								DuplicateTouchEventForChildren(ev);
							}
						}
						return true;
				}
			}
			return base.OnTouchEvent(ev);
		}

		private MotionEvent ObtainMotionEvent(MotionEvent source, MotionEventActions action)
		{
			MotionEvent ev = MotionEvent.ObtainNoHistory(source);
			ev.Action = action;
			return ev;
		}

		/**
		 * Duplicate touch events to child views.
		 * We want to dispatch a down motion event and the move events to
		 * child views, but calling dispatchTouchEvent() causes StackOverflowError.
		 * Therefore we do it manually.
		 *
		 * @param ev            motion event to be passed to children
		 * @param pendingEvents pending events like ACTION_DOWN. This will be passed to the children before ev
		 */
		private void DuplicateTouchEventForChildren(MotionEvent ev, params MotionEvent[] pendingEvents)
		{
			if (ev == null)
			{
				return;
			}
			for (int i = ChildCount - 1; 0 <= i; i--)
			{
				View childView = GetChildAt(i);
				if (childView != null)
				{
					Rect childRect = new Rect();
					childView.GetHitRect(childRect);
					MotionEvent event2 = MotionEvent.ObtainNoHistory(ev);
					if (!childRect.Contains((int)event2.GetX(), (int)event2.GetY()))
					{
						continue;
					}
					float offsetX = -childView.Left;
					float offsetY = -childView.Top;
					bool consumed = false;
					if (pendingEvents != null)
					{
						foreach (MotionEvent pe in pendingEvents)
						{
							if (pe != null)
							{
								MotionEvent peAdjusted = MotionEvent.ObtainNoHistory(pe);
								peAdjusted.OffsetLocation(offsetX, offsetY);
								consumed |= childView.DispatchTouchEvent(peAdjusted);
							}
						}
					}
					event2.OffsetLocation(offsetX, offsetY);
					consumed |= childView.DispatchTouchEvent(event2);
					if (consumed)
					{
						break;
					}
				}
			}
		}
	}

}