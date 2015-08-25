using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace MaterialViewPager.Controllers
{
	public class ViewController {

    protected int recyclerColumnCount = 1;

    public void materialColumns(View view, int number)
    {
	    RecyclerView recyclerView = view as RecyclerView;
	    if (recyclerView != null) {
            recyclerColumnCount = number;

            recyclerView.SetLayoutManager(new GridLayoutManager(recyclerView.Context, number));
        }
    }

	public void materialAdapter(View view, string mappedName, string layoutName) {
        final int layoutResId = getLayoutIdentifierFromString(view.getContext(), layoutName);
        final Carpaccio carpaccio = CarpaccioHelper.findParentCarpaccio(view);
        if (carpaccio != null && layoutResId != -1 && view instanceof RecyclerView) {
            CommonViewController commonViewController = new CommonViewController();

            MaterialCarpaccioRecyclerViewAdapter adapter = new MaterialCarpaccioRecyclerViewAdapter(recyclerColumnCount, carpaccio, layoutResId, mappedName);

            commonViewController.setAdapterForRecyclerView(view, mappedName, layoutName, adapter);

            MaterialViewPagerHelper.registerRecyclerView((Activity) view.getContext(), (RecyclerView) view, null);
        }
    }

    public class MaterialCarpaccioRecyclerViewAdapter : CarpaccioRecyclerViewAdapter {

        //the constants value of the header view
        static final int TYPE_PLACEHOLDER = int.MinValue;

        //the size taken by the header
        protected int mPlaceholderSize = 1;

        public MaterialCarpaccioRecyclerViewAdapter(int mPlaceholderSize, Carpaccio carpaccio, int layoutResId, string mappedName) {
            super(carpaccio, layoutResId, mappedName);
            this.mPlaceholderSize = mPlaceholderSize;
        }

        public int getItemViewType(int position) {
            if (position < mPlaceholderSize)
                return TYPE_PLACEHOLDER;
            return super.getItemViewType(position);
        }

        
        public int getItemCount() {
            int itemCount = super.getItemCount();
            if( itemCount > 0)
                return itemCount+mPlaceholderSize;
            else
                return 0;
        }

        
        public object getItemForRow(View view, int position) {
            if(position >= mPlaceholderSize) {
                return super.getItemForRow(view, position - mPlaceholderSize);
            }else
                return null;
        }

        
        public Holder onCreateViewHolder(ViewGroup parent, int viewType) {
            if (viewType == TYPE_PLACEHOLDER) {
                View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.material_view_pager_placeholder, parent, false);
                return new Holder(view);
            } else
                return super.onCreateViewHolder(parent, viewType);
        }
    }

}
}