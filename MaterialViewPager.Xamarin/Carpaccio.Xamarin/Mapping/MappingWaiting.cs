using System;
using Android.Views;
using Carpaccio.Model;

namespace Carpaccio.Mapping
{
	public class MappingWaiting
	{
		public View View { get; private set; }
		public CarpaccioAction CarpaccioAction { get; private set; }
		public string Call { get; private set; } // "user.getName()"
		public string ObjectName { get; private set; } //"user"

		public MappingWaiting(View view, CarpaccioAction carpaccioAction, string call, string objectName)
		{
			View = view;
			CarpaccioAction = carpaccioAction;
			Call = call;
			ObjectName = objectName;
		}
	}
}