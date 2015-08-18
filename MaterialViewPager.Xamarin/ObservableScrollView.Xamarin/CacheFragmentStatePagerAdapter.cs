using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Java.Lang;

namespace ObservableScrollView
{
	public abstract class CacheFragmentStatePagerAdapter : FragmentStatePagerAdapter
	{
		private static readonly string _stateSuperState = "superState";
		private static readonly string _statePages = "pages";
		private static readonly string _statePageIndexPrefix = "pageIndex:";
		private static readonly string _statePageKeyPrefix = "page:";

		private readonly FragmentManager _fm;
		private readonly SparseArray<Fragment> _pages;

		protected CacheFragmentStatePagerAdapter(FragmentManager fm) : base(fm)
		{
			_pages = new SparseArray<Fragment>();
			_fm = fm;
		}

		public override IParcelable SaveState()
		{
			IParcelable p = base.SaveState();
			Bundle bundle = new Bundle();
			bundle.PutParcelable(_stateSuperState, p);

			bundle.PutInt(_statePages, _pages.Size());
			if (0 < _pages.Size())
			{
				for (int i = 0; i < _pages.Size(); i++)
				{
					int position = _pages.KeyAt(i);
					bundle.PutInt(CreateCacheIndex(i), position);
					Fragment f = _pages.Get(position);
					_fm.PutFragment(bundle, CreateCacheKey(position), f);
				}
			}
			return bundle;
		}

		public override void RestoreState(IParcelable state, ClassLoader loader)
		{
			Bundle bundle = (Bundle)state;
			int pages = bundle.GetInt(_statePages);
			if (0 < pages)
			{
				for (int i = 0; i < pages; i++)
				{
					int position = bundle.GetInt(CreateCacheIndex(i));
					Fragment f = _fm.GetFragment(bundle, CreateCacheKey(position));
					_pages.Put(position, f);
				}
			}

			IParcelable p = (IParcelable)bundle.GetParcelable(_stateSuperState);
			base.RestoreState(p, loader);
		}

		/**
		 * Get a new Fragment instance.
		 * Each fragments are automatically cached in this method,
		 * so you don't have to do it by yourself.
		 * If you want to implement instantiation of Fragments,
		 * you should override {@link #createItem(int)} instead.
		 *
		 * {@inheritDoc}
		 *
		 * @param position position of the item in the adapter
		 * @return fragment instance
		 */
		public override Fragment GetItem(int position)
		{
			Fragment f = CreateItem(position);
			// We should cache fragments manually to access to them later
			_pages.Put(position, f);
			return f;
		}

		public override void DestroyItem(ViewGroup container, int position, Object obj)
		{
			if (0 <= _pages.IndexOfKey(position))
			{
				_pages.Remove(position);
			}
			base.DestroyItem(container, position, obj);
		}

		/**
		 * Get the item at the specified position in the adapter.
		 *
		 * @param position position of the item in the adapter
		 * @return fragment instance
		 */
		public Fragment GetItemAt(int position)
		{
			return _pages.Get(position);
		}

		/**
		 * Create a new Fragment instance.
		 * This is called inside {@link #getItem(int)}.
		 *
		 * @param position position of the item in the adapter
		 * @return fragment instance
		 */
		protected abstract Fragment CreateItem(int position);

		/**
		 * Create an index string for caching Fragment pages.
		 *
		 * @param index index of the item in the adapter
		 * @return key string for caching Fragment pages
		 */
		protected string CreateCacheIndex(int index)
		{
			return _statePageIndexPrefix + index;
		}

		/**
		 * Create a key string for caching Fragment pages.
		 *
		 * @param position position of the item in the adapter
		 * @return key string for caching Fragment pages
		 */
		protected string CreateCacheKey(int position)
		{
			return _statePageKeyPrefix + position;
		}
	}
}