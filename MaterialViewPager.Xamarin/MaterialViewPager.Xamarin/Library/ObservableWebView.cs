using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Webkit;

namespace MaterialViewPager.Library
{
	public class ObservableWebView : WebView, IScrollable
	{

		// Fields that should be saved onSaveInstanceState
		private int _mPrevScrollY;
		private int _mScrollY;

		// Fields that don't need to be saved onSaveInstanceState
		private IObservableScrollViewCallbacks _mCallbacks;
		private ScrollState _mScrollState;
		private bool _mFirstScroll;
		private bool _mDragging;
		private bool _mIntercepted;
		private MotionEvent _mPrevMoveEvent;
		private ViewGroup _mTouchInterceptionViewGroup;

		public ObservableWebView(Context context)
			: base(context)
		{

		}

		public ObservableWebView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{

		}

		public ObservableWebView(Context context, IAttributeSet attrs, int defStyle)
			: base(context, attrs, defStyle)
		{

		}


		protected override void OnRestoreInstanceState(IParcelable state)
		{
			ObservableScrollSavedState ss = state as ObservableScrollSavedState;
			if (ss != null)
			{
				_mPrevScrollY = ss.PrevScrollY;
				_mScrollY = ss.ScrollY;
				base.OnRestoreInstanceState(ss.SuperState);
			}
		}


		protected override IParcelable OnSaveInstanceState()
		{
			IParcelable superState = base.OnSaveInstanceState();
			ObservableScrollSavedState ss = new ObservableScrollSavedState(superState)
			{
				PrevScrollY = _mPrevScrollY,
				ScrollY = _mScrollY
			};
			return ss;
		}


		protected override void OnScrollChanged(int l, int t, int oldl, int oldt)
		{
			base.OnScrollChanged(l, t, oldl, oldt);
			if (_mCallbacks != null)
			{
				_mScrollY = t;

				_mCallbacks.OnScrollChanged(t, _mFirstScroll, _mDragging);
				if (_mFirstScroll)
				{
					_mFirstScroll = false;
				}

				if (_mPrevScrollY < t)
				{
					_mScrollState = ScrollState.Up;
				}
				else if (t < _mPrevScrollY)
				{
					_mScrollState = ScrollState.Down;
				}
				else
				{
					_mScrollState = ScrollState.Stop;
				}
				_mPrevScrollY = t;
			}
		}


		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			if (_mCallbacks != null)
			{
				switch (ev.ActionMasked)
				{
					case MotionEventActions.Down:
						// Whether or not motion events are consumed by children,
						// flag initializations which are related to ACTION_DOWN events should be executed.
						// Because if the ACTION_DOWN is consumed by children and only ACTION_MOVEs are
						// passed to parent (this view), the flags will be invalid.
						// Also, applications might implement initialization codes to onDownMotionEvent,
						// so call it here.
						_mFirstScroll = _mDragging = true;
						_mCallbacks.OnDownMotionEvent();
						break;
				}
			}
			return base.OnInterceptTouchEvent(ev);
		}


		public override bool OnTouchEvent(MotionEvent ev)
		{
			if (_mCallbacks != null)
			{
				switch (ev.ActionMasked)
				{
					case MotionEventActions.Down:
						break;
					case MotionEventActions.Up:
					case MotionEventActions.Cancel:
						_mIntercepted = false;
						_mDragging = false;
						_mCallbacks.OnUpOrCancelMotionEvent(_mScrollState);
						break;
					case MotionEventActions.Move:
						if (_mPrevMoveEvent == null)
						{
							_mPrevMoveEvent = ev;
						}
						float diffY = ev.GetY() - _mPrevMoveEvent.GetY();
						_mPrevMoveEvent = MotionEvent.ObtainNoHistory(ev);
						if (GetCurrentScrollY() - diffY <= 0)
						{
							// Can't scroll anymore.

							if (_mIntercepted)
							{
								// Already dispatched ACTION_DOWN event to parents, so stop here.
								return false;
							}

							// Apps can set the interception target other than the direct parent.
							ViewGroup parent;
							if (_mTouchInterceptionViewGroup == null)
							{
								parent = (ViewGroup)Parent;
							}
							else
							{
								parent = _mTouchInterceptionViewGroup;
							}

							// Get offset to parents. If the parent is not the direct parent,
							// we should aggregate offsets from all of the parents.
							float offsetX = 0;
							float offsetY = 0;
							for (View v = this; v != null && v != parent; v = (View)v.Parent)
							{
								offsetX += v.Left - v.ScrollX;
								offsetY += v.Top - v.ScrollY;
							}
							MotionEvent eventNoHistory = MotionEvent.ObtainNoHistory(ev);
							eventNoHistory.OffsetLocation(offsetX, offsetY);

							if (parent.OnInterceptTouchEvent(eventNoHistory))
							{
								_mIntercepted = true;

								// If the parent wants to intercept ACTION_MOVE events,
								// we pass ACTION_DOWN event to the parent
								// as if these touch events just have began now.
								eventNoHistory.Action = MotionEventActions.Down;

								// Return this onTouchEvent() first and set ACTION_DOWN event for parent
								// to the queue, to keep events sequence.
								Post(() => parent.DispatchTouchEvent(eventNoHistory));
								return false;
							}
							// Even when this can't be scrolled anymore,
							// simply returning false here may cause subView's click,
							// so delegate it to base.
							return base.OnTouchEvent(ev);
						}
						break;
				}
			}
			return base.OnTouchEvent(ev);
		}


		public void SetScrollViewCallbacks(IObservableScrollViewCallbacks listener)
		{
			_mCallbacks = listener;
		}


		public void SetTouchInterceptionViewGroup(ViewGroup viewGroup)
		{
			_mTouchInterceptionViewGroup = viewGroup;
		}


		public void ScrollVerticallyTo(int y)
		{
			ScrollTo(0, y);
		}


		public int GetCurrentScrollY()
		{
			return _mScrollY;
		}
	}
}