using Android.Support.V7.Widget;

namespace MaterialViewPager.Adapter
{
	public class MaterialAdapterDataObserver : RecyclerView.AdapterDataObserver
	{
		private readonly RecyclerView.Adapter _adapter;

		public MaterialAdapterDataObserver(RecyclerView.Adapter adapter)
		{
			_adapter = adapter;
		}

		public override void OnChanged()
		{
			base.OnChanged();
			_adapter.NotifyDataSetChanged();
		}
	}
}