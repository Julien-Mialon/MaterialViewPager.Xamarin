using System.Linq;
using Android.OS;
using Android.Util;
using Android.Views;
using Java.Interop;
using Java.Lang;
using Debug = System.Diagnostics.Debug;

namespace ObservableScrollView
{
	public class ObservableListSavedState : View.BaseSavedState
	{
		public int PrevFirstVisiblePosition;
		public int PrevFirstVisibleChildHeight = -1;
		public int PrevScrolledChildrenHeight;
		public int PrevScrollY;
		public int ScrollY;
		public SparseIntArray ChildrenHeights;

		/**
		 * Called by onSaveInstanceState.
		 */
		internal ObservableListSavedState(IParcelable superState)
			: base(superState)
		{

		}

		/**
		 * Called by CREATOR.
		 */
		private ObservableListSavedState(Parcel input)
			: base(input)
		{

			PrevFirstVisiblePosition = input.ReadInt();
			PrevFirstVisibleChildHeight = input.ReadInt();
			PrevScrolledChildrenHeight = input.ReadInt();
			PrevScrollY = input.ReadInt();
			ScrollY = input.ReadInt();
			ChildrenHeights = new SparseIntArray();
			int numOfChildren = input.ReadInt();
			if (0 < numOfChildren)
			{
				for (int i = 0; i < numOfChildren; i++)
				{
					int key = input.ReadInt();
					int value = input.ReadInt();
					ChildrenHeights.Put(key, value);
				}
			}
		}


		public override void WriteToParcel(Parcel output, ParcelableWriteFlags flags)
		{
			base.WriteToParcel(output, flags);
			output.WriteInt(PrevFirstVisiblePosition);
			output.WriteInt(PrevFirstVisibleChildHeight);
			output.WriteInt(PrevScrolledChildrenHeight);
			output.WriteInt(PrevScrollY);
			output.WriteInt(ScrollY);
			int numOfChildren = ChildrenHeights == null ? 0 : ChildrenHeights.Size();
			output.WriteInt(numOfChildren);

			Debug.Assert(ChildrenHeights != null, "childrenHeights != null");
			for (int i = 0; i < numOfChildren; i++)
			{
				output.WriteInt(ChildrenHeights.KeyAt(i));
				output.WriteInt(ChildrenHeights.ValueAt(i));
			}
		}

		[ExportField("CREATOR")]
		// ReSharper disable once UnusedMember.Local
		static SavedStateCreator InititalizeCreator()
		{
			return new SavedStateCreator();
		}

		class SavedStateCreator : Object, IParcelableCreator
		{
			public Object CreateFromParcel(Parcel source)
			{
				return new ObservableListSavedState(source);
			}

			public Object[] NewArray(int size)
			{
				return (new ObservableListSavedState[size]).Cast<Object>().ToArray();
			}
		}
	}
}