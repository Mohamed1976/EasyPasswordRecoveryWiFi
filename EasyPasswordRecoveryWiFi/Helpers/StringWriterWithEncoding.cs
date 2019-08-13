using System.IO;
using System.Text;

namespace EasyPasswordRecoveryWiFi.Helpers
{
	/// <summary>
	/// This class inherits from StringWriter in order to override the encoding.  
	/// </summary>
	public sealed class StringWriterWithEncoding : StringWriter
	{
		public override Encoding Encoding { get; }

		public StringWriterWithEncoding(Encoding encoding)
		{
			Encoding = encoding;
		}
	}
}
