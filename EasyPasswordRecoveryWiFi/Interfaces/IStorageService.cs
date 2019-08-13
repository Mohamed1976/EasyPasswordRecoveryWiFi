using System;
using System.IO;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Interfaces
{
	/// <summary>
	/// Write buffer to specified path.
	/// </summary>
	public interface IStorageService
	{
		Task WriteToFileAsync(string filePath, FileMode fileMode, string buffer);
	}
}
