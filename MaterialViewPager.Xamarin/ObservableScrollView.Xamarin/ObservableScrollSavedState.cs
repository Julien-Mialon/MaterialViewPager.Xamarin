using System.Linq;
using Android.OS;
using Android.Views;
using Java.Interop;
using Java.Lang;

namespace ObservableScrollView
{
	public class ObservableScrollSavedState : View.BaseSavedState
	{
		public int PrevScrollY { get; set; }
		public int ScrollY { get; set; }

		/**
		 * Called by onSaveInstanceState.
		 */
		public ObservableScrollSavedState(IParcelable superState)
			: base(superState)
		{

		}

		/**
		 * Called by CREATOR.
		 */
		private ObservableScrollSavedState(Parcel source)
			: base(source)
		{
			PrevScrollY = source.ReadInt();
			ScrollY = source.ReadInt();
		}


		public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
		{
			base.WriteToParcel(dest, flags);
			dest.WriteInt(PrevScrollY);
			dest.WriteInt(ScrollY);
		}

		[ExportField("CREATOR")]
		// ReSharper disable once UnusedMember.Local
		static SavedStateCreator InititalizeCreator()
		{
			return new SavedStateCreator();
		}

		public class SavedStateCreator : Object, IParcelableCreator
		{
			public Object CreateFromParcel(Parcel source)
			{
				return new ObservableScrollSavedState(source);
			}

			public Object[] NewArray(int size)
			{
				return (new ObservableScrollSavedState[size]).Cast<Object>().ToArray();
			}
		}
	}
}