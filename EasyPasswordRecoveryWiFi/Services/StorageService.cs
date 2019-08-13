using EasyPasswordRecoveryWiFi.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Services
{
	public class StorageService : IStorageService
	{
		/// <summary>
		/// Writes buffer to the specified file. 
		/// </summary>
		/// <param name="filePath">File path where the buffer will be saved.</param>
		/// <param name="fileMode">Append or overwrite file.</param>
		/// <param name="buffer">Data buffer to store.</param>
		public async Task WriteToFileAsync(string filePath, FileMode fileMode, string buffer)
		{
			int offset = 0;
			int sizeOfBuffer = 1024;
			FileStream fileStream = null;

			if (string.IsNullOrEmpty(filePath))
			{
				throw new ArgumentNullException(nameof(filePath));
			}

			if (string.IsNullOrEmpty(buffer))
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			byte[] bytesBuff = Encoding.Unicode.GetBytes(buffer);

			try
			{
				fileStream = new FileStream(filePath, fileMode, FileAccess.Write,
					FileShare.None, bufferSize: sizeOfBuffer, useAsync: true);
				await fileStream.WriteAsync(bytesBuff, offset, bytesBuff.Length);
			}
			finally
			{
				if (fileStream != null)
				{
					fileStream.Close();
					fileStream.Dispose();
					fileStream = null;
				}
			}
		}
	}
}
