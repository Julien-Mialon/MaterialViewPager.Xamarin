using Android.Support.V7.Widget;
using Android.Views;
using Java.Lang;

namespace MaterialViewPager.Adapter
{
	public class RecyclerViewMaterialAdapter : RecyclerView.Adapter
	{

		//the constants value of the header view
		const int TYPE_PLACEHOLDER = Integer.MinValue;

		//the size taken by the header
		private readonly int _mPlaceholderSize = 1;

		//the actual RecyclerView.Adapter
		private readonly RecyclerView.Adapter _mAdapter;

		/**
		 * Construct the RecyclerViewMaterialAdapter, which inject a header into an actual RecyclerView.Adapter
		 *
		 * @param adapter The real RecyclerView.Adapter which displays content
		 */
		public RecyclerViewMaterialAdapter(RecyclerView.Adapter adapter)
		{
			_mAdapter = adapter;

			RegisterAdapterObserver();
		}

		/**
		 * Construct the RecyclerViewMaterialAdapter, which inject a header into an actual RecyclerView.Adapter
		 *
		 * @param adapter         The real RecyclerView.Adapter which displays content
		 * @param placeholderSize The number of placeholder items before real items, default is 1
		 */
		public RecyclerViewMaterialAdapter(RecyclerView.Adapter adapter, int placeholderSize)
		{
			_mAdapter = adapter;
			_mPlaceholderSize = placeholderSize;

			RegisterAdapterObserver();
		}



		protected void RegisterAdapterObserver()
		{
			if (_mAdapter != null)
			{

				RegisterAdapterDataObserver(new MaterialAdapterDataObserver(_mAdapter));
			}
		}


		public override int GetItemViewType(int position)
		{
			if (position < _mPlaceholderSize)
				return TYPE_PLACEHOLDER;
			return _mAdapter.GetItemViewType(position - _mPlaceholderSize); //call getItemViewType on the adapter, less mPlaceholderSize
		}

		//dispatch getItemCount to the actual adapter, add mPlaceholderSize

		public override int ItemCount
		{
			get { return _mAdapter.ItemCount + _mPlaceholderSize; }
		}


		//add the header on first position, else display the true adapter's cells

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			switch (viewType)
			{
				case TYPE_PLACEHOLDER:
					View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.material_view_pager_placeholder, parent, false);
					return new MaterialViewHolder(view);
				default:
					return _mAdapter.OnCreateViewHolder(parent, viewType);
			}
		}

		//dispatch onBindViewHolder on the actual mAdapter

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			switch (GetItemViewType(position))
			{
				case TYPE_PLACEHOLDER:
					break;
				default:
					_mAdapter.OnBindViewHolder(holder, position - _mPlaceholderSize);
					break;
			}
		}

		public void mvp_notifyDataSetChanged()
		{
			_mAdapter.NotifyDataSetChanged();
			NotifyDataSetChanged();
		}

		public void mvp_notifyItemChanged(int position)
		{
			_mAdapter.NotifyItemChanged(position - 1);
			NotifyItemChanged(position);
		}

		public void mvp_notifyItemInserted(int position)
		{
			_mAdapter.NotifyItemInserted(position - 1);
			NotifyItemInserted(position);
		}

		public void mvp_notifyItemRemoved(int position)
		{
			_mAdapter.NotifyItemRemoved(position - 1);
			NotifyItemRemoved(position);
		}

		public void mpv_notifyItemRangeChanged(int startPosition, int itemCount)
		{
			_mAdapter.NotifyItemRangeChanged(startPosition - 1, itemCount - 1);
			NotifyItemRangeChanged(startPosition, itemCount);
		}

		public void mpv_notifyItemRangeInserted(int startPosition, int itemCount)
		{
			_mAdapter.NotifyItemRangeInserted(startPosition - 1, itemCount - 1);
			NotifyItemRangeInserted(startPosition, itemCount);
		}

		public void mpv_notifyItemRangeRemoved(int startPosition, int itemCount)
		{
			_mAdapter.NotifyItemRangeRemoved(startPosition - 1, itemCount - 1);
			NotifyItemRangeRemoved(startPosition, itemCount);
		}

	}
}