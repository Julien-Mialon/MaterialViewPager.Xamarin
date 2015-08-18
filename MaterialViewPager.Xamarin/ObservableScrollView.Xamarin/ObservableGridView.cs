using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Math = System.Math;
using Object = Java.Lang.Object;

namespace ObservableScrollView
{
	public class ObservableGridView : GridView, IScrollable
	{
		class FixedViewInfo
		{
			public View View;
			public ViewGroup ViewContainer;
			public Object Data;
			public bool IsSelectable;
		}

		private class FullWidthFixedViewLayout : FrameLayout
		{
			private readonly ObservableGridView _gridView;

			public FullWidthFixedViewLayout(ObservableGridView gridView, Context context)
				: base(context)
			{
				_gridView = gridView;
			}


			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				int targetWidth = _gridView.MeasuredWidth - _gridView.PaddingLeft - _gridView.PaddingRight;
				widthMeasureSpec = MeasureSpec.MakeMeasureSpec(targetWidth, MeasureSpec.GetMode(widthMeasureSpec));
				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
			}
		}

		private class HeaderViewGridAdapter : Object, IWrapperListAdapter, IFilterable
		{
			private readonly DataSetObservable _dataSetObservable = new DataSetObservable();
			private readonly IListAdapter _adapter;
			
			// This List is assumed to NOT be null.
			readonly List<FixedViewInfo> _headerViewInfos;
			readonly List<FixedViewInfo> _footerViewInfos;
			private int _numColumns = 1;
			private int _rowHeight = -1;
			bool _areAllFixedViewsSelectable;
			private readonly bool _isFilterable;
			private readonly bool _cachePlaceHoldView = true;
			// From Recycle Bin or calling getView, this a question...
			private readonly bool _cacheFirstHeaderView = false;

			public HeaderViewGridAdapter(List<FixedViewInfo> headerViewInfos, List<FixedViewInfo> footViewInfos, IListAdapter adapter)
			{
				_adapter = adapter;
				_isFilterable = adapter is IFilterable;
				
				_headerViewInfos = headerViewInfos ?? new List<FixedViewInfo>();
				_footerViewInfos = footViewInfos ?? new List<FixedViewInfo>();

				_areAllFixedViewsSelectable = AreAllListInfosSelectable(_headerViewInfos)
						&& AreAllListInfosSelectable(_footerViewInfos);
			}

			public void SetNumColumns(int numColumns)
			{
				if (numColumns < 1)
				{
					return;
				}
				if (_numColumns != numColumns)
				{
					_numColumns = numColumns;
					NotifyDataSetChanged();
				}
			}

			// ReSharper disable once UnusedMember.Local
			public void SetRowHeight(int height)
			{
				_rowHeight = height;
			}

			private int GetHeadersCount()
			{
				return _headerViewInfos.Count;
			}

			private int GetFootersCount()
			{
				return _footerViewInfos.Count;
			}

			private bool AreAllListInfosSelectable(List<FixedViewInfo> infos)
			{
				if (infos != null)
				{
					return infos.All(info => info.IsSelectable);
				}
				return true;
			}

			public bool RemoveHeader(View v)
			{
				for (int i = 0; i < _headerViewInfos.Count; i++)
				{
					FixedViewInfo info = _headerViewInfos[i];
					if (info.View == v)
					{
						_headerViewInfos.RemoveAt(i);
						_areAllFixedViewsSelectable =
								AreAllListInfosSelectable(_headerViewInfos) && AreAllListInfosSelectable(_footerViewInfos);
						_dataSetObservable.NotifyChanged();
						return true;
					}
				}
				return false;
			}

			public bool RemoveFooter(View v)
			{
				for (int i = 0; i < _footerViewInfos.Count; i++)
				{
					FixedViewInfo info = _footerViewInfos[i];
					if (info.View == v)
					{
						_footerViewInfos.RemoveAt(i);
						_areAllFixedViewsSelectable =
								AreAllListInfosSelectable(_headerViewInfos) && AreAllListInfosSelectable(_footerViewInfos);
						_dataSetObservable.NotifyChanged();
						return true;
					}
				}
				return false;
			}

			public bool AreAllItemsEnabled()
			{
				return _adapter == null || _areAllFixedViewsSelectable && _adapter.AreAllItemsEnabled();
			}

			private int GetAdapterAndPlaceHolderCount()
			{
				return (int)(Math.Ceiling(1f * _adapter.Count / _numColumns) * _numColumns);
			}

			public bool IsEnabled(int position)
			{
				// Header (negative positions will throw an IndexOutOfBoundsException)
				int numHeadersAndPlaceholders = GetHeadersCount() * _numColumns;
				if (position < numHeadersAndPlaceholders)
				{
					return position % _numColumns == 0
							&& _headerViewInfos[position / _numColumns].IsSelectable;
				}

				// Adapter
				int adjPosition = position - numHeadersAndPlaceholders;
				int adapterCount = 0;
				if (_adapter != null)
				{
					adapterCount = GetAdapterAndPlaceHolderCount();
					if (adjPosition < adapterCount)
					{
						return adjPosition < _adapter.Count && _adapter.IsEnabled(adjPosition);
					}
				}

				// Footer (off-limits positions will throw an IndexOutOfBoundsException)
				int footerPosition = adjPosition - adapterCount;
				return footerPosition % _numColumns == 0
						&& _footerViewInfos[footerPosition / _numColumns].IsSelectable;
			}

			public Object GetItem(int position)
			{
				// Header (negative positions will throw an ArrayIndexOutOfBoundsException)
				int numHeadersAndPlaceholders = GetHeadersCount() * _numColumns;
				if (position < numHeadersAndPlaceholders)
				{
					if (position % _numColumns == 0)
					{
						return _headerViewInfos[position / _numColumns].Data;
					}
					return null;
				}

				// Adapter
				int adjPosition = position - numHeadersAndPlaceholders;
				int adapterCount = 0;
				if (_adapter != null)
				{
					adapterCount = GetAdapterAndPlaceHolderCount();
					if (adjPosition < adapterCount)
					{
						if (adjPosition < _adapter.Count)
						{
							return _adapter.GetItem(adjPosition);
						}
						return null;
					}
				}

				// Footer (off-limits positions will throw an IndexOutOfBoundsException)
				int footerPosition = adjPosition - adapterCount;
				if (footerPosition % _numColumns == 0)
				{
					return _footerViewInfos[footerPosition].Data;
				}
				return null;
			}

			public long GetItemId(int position)
			{
				int numHeadersAndPlaceholders = GetHeadersCount() * _numColumns;
				if (_adapter != null && position >= numHeadersAndPlaceholders)
				{
					int adjPosition = position - numHeadersAndPlaceholders;
					int adapterCount = _adapter.Count;
					if (adjPosition < adapterCount)
					{
						return _adapter.GetItemId(adjPosition);
					}
				}
				return -1;
			}

			public View GetView(int position, View convertView, ViewGroup parent)
			{

				// Header (negative positions will throw an ArrayIndexOutOfBoundsException)
				int numHeadersAndPlaceholders = GetHeadersCount() * _numColumns;
				if (position < numHeadersAndPlaceholders)
				{
					View headerViewContainer = _headerViewInfos[position / _numColumns].ViewContainer;
					if (position % _numColumns == 0)
					{
						return headerViewContainer;
					}
					if (convertView == null)
					{
						convertView = new View(parent.Context);
					}
					// We need to do this because GridView uses the height of the last item
					// in a row to determine the height for the entire row.
					convertView.Visibility = ViewStates.Invisible;
					convertView.SetMinimumHeight(headerViewContainer.Height);
					return convertView;
				}
				// Adapter
				int adjPosition = position - numHeadersAndPlaceholders;
				int adapterCount = 0;
				if (_adapter != null)
				{
					adapterCount = GetAdapterAndPlaceHolderCount();
					if (adjPosition < adapterCount)
					{
						if (adjPosition < _adapter.Count)
						{
							return _adapter.GetView(adjPosition, convertView, parent);
						}
						if (convertView == null)
						{
							convertView = new View(parent.Context);
						}
						convertView.Visibility = ViewStates.Invisible;
						convertView.SetMinimumHeight(_rowHeight);
						return convertView;
					}
				}
				// Footer
				int footerPosition = adjPosition - adapterCount;
				if (footerPosition < Count)
				{
					View footViewContainer = _footerViewInfos[footerPosition / _numColumns].ViewContainer;
					if (position % _numColumns == 0)
					{
						return footViewContainer;
					}
					if (convertView == null)
					{
						convertView = new View(parent.Context);
					}
					// We need to do this because GridView uses the height of the last item
					// in a row to determine the height for the entire row.
					convertView.Visibility = ViewStates.Invisible;
					convertView.SetMinimumHeight(footViewContainer.Height);
					return convertView;
				}
				throw new ArrayIndexOutOfBoundsException(position);
			}

			public int GetItemViewType(int position)
			{

				int numHeadersAndPlaceholders = GetHeadersCount() * _numColumns;
				int adapterViewTypeStart = _adapter == null ? 0 : _adapter.ViewTypeCount - 1;
				int type = ItemViewTypeHeaderOrFooter;
				if (_cachePlaceHoldView)
				{
					// Header
					if (position < numHeadersAndPlaceholders)
					{
						if (position == 0)
						{
							if (_cacheFirstHeaderView)
							{
								type = adapterViewTypeStart + _headerViewInfos.Count + _footerViewInfos.Count + 1 + 1;
							}
						}
						if (position % _numColumns != 0)
						{
							type = adapterViewTypeStart + (position / _numColumns + 1);
						}
					}
				}

				// Adapter
				int adjPosition = position - numHeadersAndPlaceholders;
				int adapterCount = 0;
				if (_adapter != null)
				{
					adapterCount = GetAdapterAndPlaceHolderCount();
					if (adjPosition >= 0 && adjPosition < adapterCount)
					{
						if (adjPosition < _adapter.Count)
						{
							type = _adapter.GetItemViewType(adjPosition);
						}
						else
						{
							if (_cachePlaceHoldView)
							{
								type = adapterViewTypeStart + _headerViewInfos.Count + 1;
							}
						}
					}
				}

				if (_cachePlaceHoldView)
				{
					// Footer
					int footerPosition = adjPosition - adapterCount;
					if (footerPosition >= 0 && footerPosition < Count && (footerPosition % _numColumns) != 0)
					{
						type = adapterViewTypeStart + _headerViewInfos.Count + 1 + (footerPosition / _numColumns + 1);
					}
				}

				return type;
			}

			public void RegisterDataSetObserver(DataSetObserver observer)
			{
				_dataSetObservable.RegisterObserver(observer);
				if (_adapter != null)
				{
					_adapter.RegisterDataSetObserver(observer);
				}
			}

			public void UnregisterDataSetObserver(DataSetObserver observer)
			{
				_dataSetObservable.UnregisterObserver(observer);
				if (_adapter != null)
				{
					_adapter.UnregisterDataSetObserver(observer);
				}
			}

			public void NotifyDataSetChanged()
			{
				_dataSetObservable.NotifyChanged();
			}

			public int Count
			{
				get
				{
					int count = (GetFootersCount() + GetHeadersCount()) * _numColumns;
					if (_adapter != null)
					{
						count += GetAdapterAndPlaceHolderCount();
					}
					return count;
				}
			}

			public bool HasStableIds { get { return _adapter != null && _adapter.HasStableIds; } }
			public bool IsEmpty { get { return (_adapter == null || _adapter.IsEmpty); } }

			public int ViewTypeCount
			{
				get
				{
					int count = _adapter == null ? 1 : _adapter.ViewTypeCount;
					if (_cachePlaceHoldView)
					{
						int offset = _headerViewInfos.Count + 1 + _footerViewInfos.Count;
						if (_cacheFirstHeaderView)
						{
							offset += 1;
						}
						count += offset;
					}

					return count;
				}
			}

			public IListAdapter WrappedAdapter { get { return _adapter; } }
			public Filter Filter
			{
				get
				{
					return _isFilterable ? ((IFilterable)_adapter).Filter : null;
				}
			}
		}

		private const int AUTO_FIT = -1;

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
		private List<FixedViewInfo> _mHeaderViewInfos;
		private List<FixedViewInfo> _mFooterViewInfos;

		private IOnScrollListener _mOriginalScrollListener;

		public ObservableGridView(Context context)
			: base(context)
		{
			Init();
		}

		public ObservableGridView(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			Init();
		}

		public ObservableGridView(Context context, IAttributeSet attrs, int defStyle)
			: base(context, attrs, defStyle)
		{
			Init();
		}


		public override void OnRestoreInstanceState(IParcelable state)
		{
			ObservableListSavedState ss = (ObservableListSavedState)state;
			_mPrevFirstVisiblePosition = ss.PrevFirstVisiblePosition;
			_mPrevFirstVisibleChildHeight = ss.PrevFirstVisibleChildHeight;
			_mPrevScrolledChildrenHeight = ss.PrevScrolledChildrenHeight;
			_mPrevScrollY = ss.PrevScrollY;
			_mScrollY = ss.ScrollY;
			_mChildrenHeights = ss.ChildrenHeights;
			base.OnRestoreInstanceState(ss.SuperState);
		}

		public override IParcelable OnSaveInstanceState()
		{
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
							MotionEvent event2 = MotionEvent.ObtainNoHistory(ev);
							event2.OffsetLocation(offsetX, offsetY);

							if (parent.OnInterceptTouchEvent(event2))
							{
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

		public void AddFooterView(View v)
		{
			AddFooterView(v, null, true);
		}

		public void AddFooterView(View v, Object data, bool isSelectable)
		{
			IListAdapter mAdapter = Adapter;
			if (mAdapter != null && !(mAdapter is HeaderViewGridAdapter))
			{
				throw new IllegalStateException(
						"Cannot add header view to grid -- setAdapter has already been called.");
			}

			ViewGroup.LayoutParams lyp = v.LayoutParameters;

			FixedViewInfo info = new FixedViewInfo();
			FrameLayout fl = new FullWidthFixedViewLayout(this, Context);

			if (lyp != null)
			{
				v.LayoutParameters = new FrameLayout.LayoutParams(lyp.Width, lyp.Height);
				fl.LayoutParameters = new LayoutParams(lyp.Width, lyp.Height);
			}
			fl.AddView(v);
			info.View = v;
			info.ViewContainer = fl;
			info.Data = data;
			info.IsSelectable = isSelectable;
			_mFooterViewInfos.Add(info);

			if (mAdapter != null)
			{
				((HeaderViewGridAdapter)mAdapter).NotifyDataSetChanged();
			}
		}

		public bool RemoveFooterView(View v)
		{
			if (_mFooterViewInfos.Any())
			{
				bool result = false;
				IListAdapter adapter = Adapter;
				if (adapter != null && ((HeaderViewGridAdapter)adapter).RemoveFooter(v))
				{
					result = true;
				}
				RemoveFixedViewInfo(v, _mFooterViewInfos);
				return result;
			}
			return false;
		}


		public override void SetOnScrollListener(IOnScrollListener l)
		{
			// Don't set l to base.setOnScrollListener().
			// l receives all events through mScrollListener.
			_mOriginalScrollListener = l;
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


		public override void SetClipChildren(bool clipChildren)
		{
			// Ignore, since the header rows depend on not being clipped
		}


		public override IListAdapter Adapter
		{
			get { return base.Adapter; }
			set
			{
				if (_mHeaderViewInfos.Any())
				{
					HeaderViewGridAdapter headerViewGridAdapter = new HeaderViewGridAdapter(_mHeaderViewInfos, _mFooterViewInfos, value);
					int numColumns = GetNumColumnsCompat();
					if (1 < numColumns)
					{
						headerViewGridAdapter.SetNumColumns(numColumns);
					}
					base.Adapter = headerViewGridAdapter;
				}
				else
				{
					base.Adapter = value;
				}
			}
		}

		[Obsolete("Please use the Adapter property setter")]
		public override void SetAdapter(IListAdapter adapter)
		{
			if (_mHeaderViewInfos.Any())
			{
				HeaderViewGridAdapter headerViewGridAdapter = new HeaderViewGridAdapter(_mHeaderViewInfos, _mFooterViewInfos, adapter);
				int numColumns = GetNumColumnsCompat();
				if (1 < numColumns)
				{
					headerViewGridAdapter.SetNumColumns(numColumns);
				}
				base.SetAdapter(headerViewGridAdapter);
			}
			else
			{
				base.SetAdapter(adapter);
			}
		}

		public void AddHeaderView(View v, Object data, bool isSelectable)
		{
			IListAdapter adapter = Adapter;
			if (adapter != null && !(adapter is HeaderViewGridAdapter))
			{
				throw new IllegalStateException("Cannot add header view to grid -- setAdapter has already been called.");
			}
			FixedViewInfo info = new FixedViewInfo();
			FrameLayout fl = new FullWidthFixedViewLayout(this, Context);
			fl.AddView(v);
			info.View = v;
			info.ViewContainer = fl;
			info.Data = data;
			info.IsSelectable = isSelectable;
			_mHeaderViewInfos.Add(info);
			// in the case of re-adding a header view, or adding one later on,
			// we need to notify the observer
			if (adapter != null)
			{
				((HeaderViewGridAdapter)adapter).NotifyDataSetChanged();
			}
		}

		public void AddHeaderView(View v)
		{
			AddHeaderView(v, null, true);
		}

		public int GetHeaderViewCount()
		{
			return _mHeaderViewInfos.Count;
		}

		public bool RemoveHeaderView(View v)
		{
			if (_mHeaderViewInfos.Any())
			{
				bool result = false;
				IListAdapter adapter = Adapter;
				HeaderViewGridAdapter headerViewGridAdapter = adapter as HeaderViewGridAdapter;
				if (headerViewGridAdapter != null && headerViewGridAdapter.RemoveHeader(v))
				{
					result = true;
				}
				RemoveFixedViewInfo(v, _mHeaderViewInfos);
				return result;
			}
			return false;
		}


		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
			IListAdapter adapter = Adapter;
			HeaderViewGridAdapter gridAdapter = adapter as HeaderViewGridAdapter;
			if (gridAdapter != null)
			{
				gridAdapter.SetNumColumns(GetNumColumnsCompat());
			}
		}

		private void Init()
		{
			_mChildrenHeights = new SparseIntArray();
			_mHeaderViewInfos = new List<FixedViewInfo>();
			_mFooterViewInfos = new List<FixedViewInfo>();
			base.SetClipChildren(false);

			ScrollStateChanged += (sender, args) =>
			{
				if (_mOriginalScrollListener != null)
				{
					_mOriginalScrollListener.OnScrollStateChanged(args.View, args.ScrollState);
				}
			};

			Scroll += (sender, args) =>
			{
				if (_mOriginalScrollListener != null)
				{
					_mOriginalScrollListener.OnScroll(args.View, args.FirstVisibleItem, args.VisibleItemCount, args.TotalItemCount);
				}
				// AbsListView#invokeOnItemScrollListener calls onScrollChanged(0, 0, 0, 0)
				// on Android 4.0+, but Android 2.3 is not. (Android 3.0 is unknown)
				// So call it with onScrollListener.
				OnScrollChanged();
			};

		}
		
		private int GetNumColumnsCompat()
		{
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
			{
				return NumColumns;
			}

			int columns = 0;
			if (ChildCount > 0)
			{
				int width = GetChildAt(0).MeasuredWidth;
				if (width > 0)
				{
					columns = Width / width;
				}
			}
			return columns > 0 ? columns : AUTO_FIT;
		}

		private void OnScrollChanged()
		{
			if (_mCallbacks != null)
			{
				if (ChildCount > 0)
				{
					int firstVisiblePosition = FirstVisiblePosition;
					for (int i = FirstVisiblePosition, j = 0; i <= LastVisiblePosition; i++, j++)
					{
						if (_mChildrenHeights.IndexOfKey(i) < 0 || GetChildAt(j).Height != _mChildrenHeights.Get(i))
						{
							if (i % GetNumColumnsCompat() == 0)
							{
								_mChildrenHeights.Put(i, GetChildAt(j).Height);
							}
						}
					}

					View firstVisibleChild = GetChildAt(0);
					if (firstVisibleChild != null)
					{
						if (_mPrevFirstVisiblePosition < firstVisiblePosition)
						{
							// scroll down
							int skippedChildrenHeight = 0;
							if (firstVisiblePosition - _mPrevFirstVisiblePosition != 1)
							{
								for (int i = firstVisiblePosition - 1; i > _mPrevFirstVisiblePosition; i--)
								{
									if (0 < _mChildrenHeights.IndexOfKey(i))
									{
										skippedChildrenHeight += _mChildrenHeights.Get(i);
									}
								}
							}
							_mPrevScrolledChildrenHeight += _mPrevFirstVisibleChildHeight + skippedChildrenHeight;
							_mPrevFirstVisibleChildHeight = firstVisibleChild.Height;
						}
						else if (firstVisiblePosition < _mPrevFirstVisiblePosition)
						{
							// scroll up
							int skippedChildrenHeight = 0;
							if (_mPrevFirstVisiblePosition - firstVisiblePosition != 1)
							{
								for (int i = _mPrevFirstVisiblePosition - 1; i > firstVisiblePosition; i--)
								{
									if (0 < _mChildrenHeights.IndexOfKey(i))
									{
										skippedChildrenHeight += _mChildrenHeights.Get(i);
									}
								}
							}
							_mPrevScrolledChildrenHeight -= firstVisibleChild.Height + skippedChildrenHeight;
							_mPrevFirstVisibleChildHeight = firstVisibleChild.Height;
						}
						else if (firstVisiblePosition == 0)
						{
							_mPrevFirstVisibleChildHeight = firstVisibleChild.Height;
						}
						if (_mPrevFirstVisibleChildHeight < 0)
						{
							_mPrevFirstVisibleChildHeight = 0;
						}
						_mScrollY = _mPrevScrolledChildrenHeight - firstVisibleChild.Top;
						_mPrevFirstVisiblePosition = firstVisiblePosition;

						_mCallbacks.OnScrollChanged(_mScrollY, _mFirstScroll, _mDragging);
						if (_mFirstScroll)
						{
							_mFirstScroll = false;
						}

						if (_mPrevScrollY < _mScrollY)
						{
							_mScrollState = ObservableScrollState.Up;
						}
						else if (_mScrollY < _mPrevScrollY)
						{
							_mScrollState = ObservableScrollState.Down;
						}
						else
						{
							_mScrollState = ObservableScrollState.Stop;
						}
						_mPrevScrollY = _mScrollY;
					}
				}
			}
		}

		private void RemoveFixedViewInfo(View v, List<FixedViewInfo> where)
		{
			int len = where.Count;
			for (int i = 0; i < len; ++i)
			{
				FixedViewInfo info = where[i];
				if (info.View == v)
				{
					where.RemoveAt(i);
					break;
				}
			}
		}
	}
}