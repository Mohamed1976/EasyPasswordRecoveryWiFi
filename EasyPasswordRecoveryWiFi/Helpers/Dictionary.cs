using Caliburn.Micro;
using System;
using System.IO;
using System.Text;

namespace EasyPasswordRecoveryWiFi.Helpers
{
	public class Dictionary : Screen, IDisposable
	{
		#region [ private members ]

		private readonly StreamReader _streamReader = null;
		private readonly FileInfo _fileInfo = null;

		#endregion

		#region [ Constructor ]

		/// <summary>
		/// Constructor creates FileInfo and StreamReader object.
		/// </summary>
		/// <param name="path">File path of the dictionary.</param>
		/// <param name="encoding">The encoding of dictionary.</param>
		/// <remarks>
		/// Throws an FileNotFoundException() if the specified file (dictionary) does not exist. 
		/// </remarks>
		public Dictionary(string path, Encoding encoding)
		{
			_fileInfo = new FileInfo(path);

			if (!_fileInfo.Exists)
				throw new FileNotFoundException();

			_streamReader = new StreamReader(path, encoding, true, 1024);
		}

		#endregion

		#region [ Dispose ]

		// Track whether Dispose has been called.
		private bool _disposed = false;

		// Implement IDisposable.
		// Do not make this method virtual.
		// A derived class should not be able to override this method.
		public void Dispose()
		{

			Dispose(true);
			GC.SuppressFinalize(this);
		}

		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				// If disposing equals true, dispose all managed
				// and unmanaged resources.
				if (disposing)
				{
					// Dispose managed resources.
					_streamReader.Close();
					_streamReader.Dispose();
				}

				_disposed = true;
			}
		}

		#endregion

		#region [ Properties ]

		/// <summary>
		/// Gets the size, in bytes, of the current file.
		/// </summary>
		public long Length
		{
			get { return _fileInfo.Length; }
		}

		/// <summary>
		/// Gets a string representing the directory's full path.
		/// </summary>
		public string DirectoryName
		{
			get { return _fileInfo.DirectoryName; }
		}

		/// <summary>
		/// Gets the name of the file.
		/// </summary>
		public string Name
		{
			get { return _fileInfo.Name; }
		}

		private int lineNumber = 0;

		/// <summary>
		/// Returns the number of lines read from file.
		/// </summary>
		public int LineNumber
		{
			get { return lineNumber; }
			set
			{
				if (Set(ref lineNumber, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		#endregion

		#region [ Methods ] 

		/// <summary>
		/// The GetFirst method returns the first string in the specified file.
		/// Returns null if the end of the input stream is reached.
		/// </summary>
		/// <remarks>
		/// The GetFirst method rewinds the stream to the first line of the file.
		/// Blank lines in file are skipped. 
		/// </remarks>
		public string GetFirst()
		{
			LineNumber = 0;
			string line = string.Empty;
			_streamReader.BaseStream.Position = 0;
			_streamReader.DiscardBufferedData();

			/* Skipping Blank lines in read file. */
			while ((line = _streamReader.ReadLine()) != null &&
				(string.IsNullOrEmpty(line) ||
				string.IsNullOrWhiteSpace(line)))
			{ };

			if (line != null)
			{
				LineNumber++;
			}

			return line;
		}

		/// <summary>
		/// The GetNext method returns the next string in the specified file.
		/// Returns null if the end of the input stream is reached.
		/// </summary>
		public string GetNext()
		{
			string line = string.Empty;

			/* Skipping Blank lines in read file. */
			while ((line = _streamReader.ReadLine()) != null &&
				(string.IsNullOrEmpty(line) ||
				string.IsNullOrWhiteSpace(line)))
			{ };

			if (line != null)
			{
				LineNumber++;
			}

			return line;
		}

		#endregion
	}
}
