using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Carpaccio.Model
{
	public class CarpaccioAction
	{
		public string CompleteCall { get; set; }
		public string Function { get; set; }
		public string[] Args { get; set; }
		public bool IsCallMapping { get; set; }
		public ObjectAndMethod ObjectAndMethod { get; set; }

		public string[] Values { get; set; }

		public CarpaccioAction(string completeCall)
		{
			this.CompleteCall = completeCall;

			Function = CarpaccioHelper.getFunctionName(completeCall);
			Args = CarpaccioHelper.getAttributes(completeCall);
			Values = Args; //by default : values = args; if mapping, values will be calculated
			IsCallMapping = MappingManager.isCallMapping(Args);
		}

		public override bool Equals(object obj)
		{
			if (obj == this)
			{
				return true;
			}
			CarpaccioAction action = obj as CarpaccioAction;
			if (action == null)
			{
				return false;
			}
			return !(CompleteCall != null ? CompleteCall != action.CompleteCall : action.CompleteCall != null);

		}

		public override int GetHashCode()
		{
			return CompleteCall != null ? CompleteCall.GetHashCode() : 0;
		}

		public override string ToString()
		{
			return CompleteCall;
		}
	}
}