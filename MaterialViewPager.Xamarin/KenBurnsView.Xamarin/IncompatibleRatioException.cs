using System;
using System.Runtime.Serialization;

namespace KenBurnsView
{
	[Serializable]
	public class IncompatibleRatioException : Exception
	{
		//
		// For guidelines regarding the creation of new exception types, see
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
		// and
		//    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
		//

		public IncompatibleRatioException()
		{
		}

		public IncompatibleRatioException(string message) : base(message)
		{
		}

		public IncompatibleRatioException(string message, Exception inner) : base(message, inner)
		{
		}

		protected IncompatibleRatioException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}