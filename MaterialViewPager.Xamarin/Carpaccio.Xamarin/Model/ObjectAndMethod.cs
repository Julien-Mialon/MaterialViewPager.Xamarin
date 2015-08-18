using Java.Lang;
using Java.Lang.Reflect;

namespace Carpaccio.Model
{
	public class ObjectAndMethod
	{
		public Object Object { get; set; }
		public Method Method { get; set; }

		public ObjectAndMethod()
		{
			
		}

		public ObjectAndMethod(Object obj, Method method)
		{
			Object = obj;
			Method = method;
		}
	}
}