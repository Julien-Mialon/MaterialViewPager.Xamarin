using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Java.Lang;

namespace ObservableScrollView
{
	public class ObservableRecyclerView : RecyclerView, IScrollable {

    private static int _recyclerViewLibraryVersion = 22;

    // Fields that should be saved onSaveInstanceState
    private int _mPrevFirstVisiblePosition;
    private int _mPrevFirstVisibleChildHeight = -1;
    private int _mPrevScrolledChildrenHeight;
    private int _mPrevScrollY;
    private int _mScrollY;
    private SparseIntArray _mChildrenHeights;

    // Fields that don't need to be saved onSaveInstanceState
    private IObservableScrollViewCallbacks _mCallbacks;
    private ObservableScrollState _mScrollState;
    private bool _mFirstScroll;
    private bool _mDragging;
    private bool _mIntercepted;
    private MotionEvent _mPrevMoveEvent;
    private ViewGroup _mTouchInterceptionViewGroup;

    public ObservableRecyclerView(Context context) : base(context) {
        Init();
    }

    public ObservableRecyclerView(Context context, IAttributeSet attrs) : base(context, attrs) {
        Init();
    }

    public ObservableRecyclerView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle) {
        Init();
    }


		protected override void OnRestoreInstanceState(IParcelable state) {
        ObservableListSavedState ss = (ObservableListSavedState) state;
        _mPrevFirstVisiblePosition = ss.PrevFirstVisiblePosition;
        _mPrevFirstVisibleChildHeight = ss.PrevFirstVisibleChildHeight;
        _mPrevScrolledChildrenHeight = ss.PrevScrolledChildrenHeight;
        _mPrevScrollY = ss.PrevScrollY;
        _mScrollY = ss.ScrollY;
        _mChildrenHeights = ss.ChildrenHeights;
        base.OnRestoreInstanceState(ss.SuperState);
    }


		protected override IParcelable OnSaveInstanceState() {
        IParcelable superState = base.OnSaveInstanceState();
			ObservableListSavedState ss = new ObservableListSavedState(superState)
			{
				PrevFirstVisiblePosition = _mPrevFirstVisiblePosition,
				PrevFirstVisibleChildHeight = _mPrevFirstVisibleChildHeight,
				PrevScrolledChildrenHeight = _mPrevScrolledChildrenHeight,
				PrevScrollY = _mPrevScrollY,
				ScrollY = _mScrollY,
				ChildrenHeights = _mChildrenHeights
			};
			return ss;
    }

    
    protected override void OnScrollChanged(int l, int t, int oldl, int oldt) {
        base.OnScrollChanged(l, t, oldl, oldt);
        if (_mCallbacks != null) {
            if (ChildCount > 0) {
                int firstVisiblePosition = GetChildAdapterPosition(GetChildAt(0));
                int lastVisiblePosition = GetChildAdapterPosition(GetChildAt(ChildCount - 1));
                for (int i = firstVisiblePosition, j = 0; i <= lastVisiblePosition; i++, j++) {
                    int childHeight = 0;
                    View child = GetChildAt(j);
                    if (child != null) {
                        if (_mChildrenHeights.IndexOfKey(i) < 0 || (child.Height != _mChildrenHeights.Get(i))) {
                            childHeight = child.Height;
                        }
                    }
                    _mChildrenHeights.Put(i, childHeight);
                }

                View firstVisibleChild = GetChildAt(0);
                if (firstVisibleChild != null) {
                    if (_mPrevFirstVisiblePosition < firstVisiblePosition) {
                        // scroll down
                        int skippedChildrenHeight = 0;
                        if (firstVisiblePosition - _mPrevFirstVisiblePosition != 1) {
                            for (int i = firstVisiblePosition - 1; i > _mPrevFirstVisiblePosition; i--) {
                                if (0 < _mChildrenHeights.IndexOfKey(i)) {
                                    skippedChildrenHeight += _mChildrenHeights.Get(i);
                                } else {
                                    // Approximate each item's height to the first visible child.
                                    // It may be incorrect, but without this, scrollY will be broken
                                    // when scrolling from the bottom.
                                    skippedChildrenHeight += firstVisibleChild.Height;
                                }
                            }
                        }
                        _mPrevScrolledChildrenHeight += _mPrevFirstVisibleChildHeight + skippedChildrenHeight;
                        _mPrevFirstVisibleChildHeight = firstVisibleChild.Height;
                    } else if (firstVisiblePosition < _mPrevFirstVisiblePosition) {
                        // scroll up
                        int skippedChildrenHeight = 0;
                        if (_mPrevFirstVisiblePosition - firstVisiblePosition != 1) {
                            for (int i = _mPrevFirstVisiblePosition - 1; i > firstVisiblePosition; i--) {
                                if (0 < _mChildrenHeights.IndexOfKey(i)) {
                                    skippedChildrenHeight += _mChildrenHeights.Get(i);
                                } else {
                                    // Approximate each item's height to the first visible child.
                                    // It may be incorrect, but without this, scrollY will be broken
                                    // when scrolling from the bottom.
                                    skippedChildrenHeight += firstVisibleChild.Height;
                                }
                            }
                        }
                        _mPrevScrolledChildrenHeight -= firstVisibleChild.Height + skippedChildrenHeight;
                        _mPrevFirstVisibleChildHeight = firstVisibleChild.Height;
                    } else if (firstVisiblePosition == 0) {
                        _mPrevFirstVisibleChildHeight = firstVisibleChild.Height;
                        _mPrevScrolledChildrenHeight = 0;
                    }
                    if (_mPrevFirstVisibleChildHeight < 0) {
                        _mPrevFirstVisibleChildHeight = 0;
                    }
                    _mScrollY = _mPrevScrolledChildrenHeight - firstVisibleChild.Top;
                    _mPrevFirstVisiblePosition = firstVisiblePosition;

                    _mCallbacks.OnScrollChanged(_mScrollY, _mFirstScroll, _mDragging);
                    if (_mFirstScroll) {
                        _mFirstScroll = false;
                    }

                    if (_mPrevScrollY < _mScrollY) {
                        //down
                        _mScrollState = ObservableScrollState.Up;
                    } else if (_mScrollY < _mPrevScrollY) {
                        //up
                        _mScrollState = ObservableScrollState.Down;
                    } else {
                        _mScrollState = ObservableScrollState.Stop;
                    }
                    _mPrevScrollY = _mScrollY;
                }
            }
        }
    }

    
    public override bool OnInterceptTouchEvent(MotionEvent ev) {
        if (_mCallbacks != null) {
            switch (ev.ActionMasked) {
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

    
    public override bool OnTouchEvent(MotionEvent ev) {
        if (_mCallbacks != null) {
            switch (ev.ActionMasked) {
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                    _mIntercepted = false;
                    _mDragging = false;
                    _mCallbacks.OnUpOrCancelMotionEvent(_mScrollState);
                    break;
                case MotionEventActions.Move:
                    if (_mPrevMoveEvent == null) {
                        _mPrevMoveEvent = ev;
                    }
                    float diffY = ev.GetY() - _mPrevMoveEvent.GetY();
                    _mPrevMoveEvent = MotionEvent.ObtainNoHistory(ev);
                    if (GetCurrentScrollY() - diffY <= 0) {
                        // Can't scroll anymore.

                        if (_mIntercepted) {
                            // Already dispatched ACTION_DOWN event to parents, so stop here.
                            return false;
                        }

                        // Apps can set the interception target other than the direct parent.
                        ViewGroup parent;
                        if (_mTouchInterceptionViewGroup == null) {
                            parent = (ViewGroup) Parent;
                        } else {
                            parent = _mTouchInterceptionViewGroup;
                        }

                        // Get offset to parents. If the parent is not the direct parent,
                        // we should aggregate offsets from all of the parents.
                        float offsetX = 0;
                        float offsetY = 0;
                        for (View v = this; v != null && v != parent; v = (View) v.Parent) {
                            offsetX += v.Left - v.ScrollX;
                            offsetY += v.Top - v.ScrollY;
                        }
                        MotionEvent event2 = MotionEvent.ObtainNoHistory(ev);
                        event2.OffsetLocation(offsetX, offsetY);

                        if (parent.OnInterceptTouchEvent(event2)) {
                            _mIntercepted = true;

                            // If the parent wants to intercept ACTION_MOVE events,
                            // we pass ACTION_DOWN event to the parent
                            // as if these touch events just have began now.
                            event2.Action = MotionEventActions.Down;

                            // Return this onTouchEvent() first and set ACTION_DOWN event for parent
                            // to the queue, to keep events sequence.
                            Post(() => parent.DispatchTouchEvent(event2));
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

    
    public void SetScrollViewCallbacks(IObservableScrollViewCallbacks listener) {
        _mCallbacks = listener;
    }

    
    public void SetTouchInterceptionViewGroup(ViewGroup viewGroup) {
        _mTouchInterceptionViewGroup = viewGroup;
    }

    
    public void ScrollVerticallyTo(int y) {
        View firstVisibleChild = GetChildAt(0);
        if (firstVisibleChild != null) {
            int baseHeight = firstVisibleChild.Height;
            int position = y / baseHeight;
            ScrollVerticallyToPosition(position);
        }
    }

    /**
     * <p>Same as {@linkplain #scrollToPosition(int)} but it scrolls to the position not only make
     * the position visible.</p>
     * <p>It depends on {@code LayoutManager} how {@linkplain #scrollToPosition(int)} works,
     * and currently we know that {@linkplain LinearLayoutManager#scrollToPosition(int)} just
     * make the position visible.</p>
     * <p>In LinearLayoutManager, scrollToPositionWithOffset() is provided for scrolling to the position.
     * This method checks which LayoutManager is set,
     * and handles which method should be called for scrolling.</p>
     * <p>Other know classes (StaggeredGridLayoutManager and GridLayoutManager) are not tested.</p>
     *
     * @param position position to scroll
     */
    public void ScrollVerticallyToPosition(int position) {
        LayoutManager lm = GetLayoutManager();

	    LinearLayoutManager manager = lm as LinearLayoutManager;
	    if (manager != null) {
            manager.ScrollToPositionWithOffset(position, 0);
        } else {
            ScrollToPosition(position);
        }
    }

    
    public int GetCurrentScrollY() {
        return _mScrollY;
    }

    public override int GetChildAdapterPosition(View child) {
        if (22 <= _recyclerViewLibraryVersion) {
            return base.GetChildAdapterPosition(child);
        }
#pragma warning disable 618
        return GetChildPosition(child);
#pragma warning restore 618
    }

    private void Init() {
        _mChildrenHeights = new SparseIntArray();
        CheckLibraryVersion();
    }

    private void CheckLibraryVersion() {
        try {
            base.GetChildAdapterPosition(null);
        } catch (NoSuchMethodError) {
            _recyclerViewLibraryVersion = 21;
        }
    }

}
}