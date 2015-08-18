using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;


namespace Carpaccio
{
	class CarpaccioLogger
	{
		public static bool EnableLog = false;

		public static void i(string tag, string str)
		{
			if (EnableLog) Log.Info(tag, str);
		}

		public static void e(string tag, string str)
		{
			if (EnableLog) Log.Error(tag, str);
		}

		public static void e(string tag, string str, Exception e)
		{
			if (EnableLog) Log.Error(tag, e, str);
		}

		public static void e(string tag, string str, Throwable t)
		{
			if (EnableLog) Log.Error(tag, t, str);
		}

		public static void d(string tag, string str)
		{
			if (EnableLog) Log.Debug(tag, str);
		}

		public static void v(string tag, string str)
		{
			if (EnableLog) Log.Verbose(tag, str);
		}

		public static void w(string tag, string str)
		{
			if (EnableLog) Log.Warn(tag, str);
		}

		public static void w(string tag, Exception e)
		{
			if (EnableLog) Log.Warn(tag, e);
		}
	}
}