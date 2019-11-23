using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Helpers;
using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Messages;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.ViewModels
{
	sealed class DictionaryViewModel : Screen, IPasswordProvider, IShell
	{
		#region [ Injected instances ]

		private readonly IEventAggregator _eventAggregator = null;
		private readonly IBusyIndicator _busyIndicator = null;
		private readonly IErrorHandler _errorHandler = null;
        private readonly IConfigurationProvider _configurationProvider = null;

        #endregion

        #region [ Constructor ]

        public DictionaryViewModel(IEventAggregator eventAggregator,
			IBusyIndicator busyIndicator,
			IErrorHandler errorHandler,
            IConfigurationProvider configurationProvider)
		{
			DisplayName = "List";
			Dictionaries = new ObservableCollection<Dictionary>();
			_eventAggregator = eventAggregator;
			_busyIndicator = busyIndicator;
			_errorHandler = errorHandler;
			_configurationProvider = configurationProvider;
		}

		#endregion

		#region [ IPasswordProvider members ] 

		int index = 0;
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
			string password = null;

			index = 0;
			if (Dictionaries.Count > 0)
			{
				password = Dictionaries[index].GetFirst();
			}

			/* Adjust password casing, if configured in user settings. */
			StringCasing(ref password);
			return password;
		}

		/// <summary>
		/// The GetNext method returns the next string in the specified file.
		/// Returns null if the end of the input stream is reached.
		/// </summary>
		public string GetNext()
		{
			string password = Dictionaries[index].GetNext();

			if (password == null)
			{
				/* Move to the next dictionary. */
				if (index + 1 < Dictionaries.Count)
				{
					password = Dictionaries[++index].GetFirst();
				}
			}

			/* Adjust password casing, if configured in user settings. */
			StringCasing(ref password);
			return password;
		}

		/// <summary>
		/// Determines whether the dictionaries collection is empty.
		/// </summary>
		public bool IsEmpty
		{
			get { return Dictionaries.Count == 0; }
		}

		#endregion

		#region [ Properties ]

		private ObservableCollection<Dictionary> dictionaries = null;
		/// <summary>
		/// Collection of dictionaries.
		/// </summary>
		public ObservableCollection<Dictionary> Dictionaries
		{
			get { return dictionaries; }
			set
			{
				if (Set(ref dictionaries, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private Dictionary selectedDictionary = null;
		/// <summary>
		/// The selected dictionary.
		/// </summary>
		public Dictionary SelectedDictionary
		{
			get { return selectedDictionary; }
			set
			{
				selectedDictionary = value;
				NotifyOfPropertyChange();
				NotifyOfPropertyChange(nameof(CanRemoveDictionaryCmd));
				NotifyOfPropertyChange(nameof(CanMoveUpCmd));
				NotifyOfPropertyChange(nameof(CanMoveDownCmd));
			}
		}

		#endregion

		#region [ Methods ]

		/// <summary>
		/// The casing of the passwords provided by this passwordprovider.
		/// The user settings that determine the casing is set in the settings view.  
		/// </summary>
		private void StringCasing(ref string password)
		{
			if (!string.IsNullOrEmpty(password))
			{
				if (_configurationProvider.DictionaryCasing == Common.StringCasing.LowerCase)
				{
					password = CultureInfo.CurrentCulture.TextInfo.ToLower(password);
				}
				else if (_configurationProvider.DictionaryCasing == Common.StringCasing.UpperCase)
				{
					password = CultureInfo.CurrentCulture.TextInfo.ToUpper(password);
				}
				else if (_configurationProvider.DictionaryCasing == Common.StringCasing.TitleCase)
				{
					password = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(password);
				}
			}
		}

		/// <summary>
		/// Imports a dictionary, the imported dictionary is added to the dictionaries collection.  
		/// </summary>
		private async Task AddDictionaryAsync()
		{
			_busyIndicator.IsBusy = true;
			_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Info, "Opening OpenFileDialog."));

			await Task.Run(() =>
			{
				OpenFileDialog dlg = new OpenFileDialog();
				dlg.DefaultExt = ".txt"; // Default file extension
				dlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"; // Filter files by extension
				dlg.InitialDirectory = _configurationProvider.DictionariesDir;
				dlg.Title = "Import dictionaries";
				dlg.Multiselect = true;

				bool dialogResult = dlg.ShowDialog() ?? false;
				if (dialogResult)
				{
					foreach (string FileName in dlg.FileNames)
					{
						App.Current.Dispatcher.BeginInvoke(new System.Action(() =>
						{
							Dictionaries.Add(new Dictionary(FileName, Encoding.UTF8));
						}));
					}

					/* Set dialog initial directory to last visited directory. */
					string file = dlg.FileNames.FirstOrDefault();
					if (file != default(string))
					{
						_configurationProvider.DictionariesDir = Path.GetDirectoryName(file);
					}
				}
			});

			/* Select first dictionary in list. */
			SelectedDictionary = Dictionaries.FirstOrDefault();
			_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.None));
			_busyIndicator.IsBusy = false;
		}

		#endregion

		#region [ Command handlers ]

		/// <summary>
		/// Add dictionary to the list. 
		/// </summary>
		public void AddDictionaryCmd()
		{
			AddDictionaryAsync().FireAndForgetSafeAsync(_errorHandler);
		}

		/// <summary>
		/// Remove selected dictionary and restore selected dictionary to previous or
		/// next dictionary in the list.
		/// </summary>
		public void RemoveDictionaryCmd()
		{
			try
			{
				/* Index needed to restore the selected dictionary. */
				int index = Dictionaries.IndexOf(SelectedDictionary);
				Dictionaries.Remove(SelectedDictionary);

				/* Restore the selected dictionary. */
				if (Dictionaries.Count > 0 && index < Dictionaries.Count)
				{
					SelectedDictionary = Dictionaries[index];
				}
				else if (Dictionaries.Count > 0 && index == Dictionaries.Count)
				{
					SelectedDictionary = Dictionaries[index - 1];
				}
			}
			catch (Exception ex)
			{
				_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Error, ex.Message));
				_busyIndicator.ResetState();
			}
		}

		/// <summary>
		/// If a dictionary is selected, enable remove command.
		/// </summary>
		public bool CanRemoveDictionaryCmd
		{
			get
			{
				bool canRemove = false;

				if (SelectedDictionary != null)
				{
					canRemove = true;
				}

				return canRemove;
			}
		}

		/// <summary>
		/// Move dictionary up the list.
		/// </summary>
		public void MoveUpCmd()
		{
			try
			{
				int index = Dictionaries.IndexOf(SelectedDictionary);
				Dictionaries.Swap(index, index - 1);
				SelectedDictionary = Dictionaries[index - 1];
			}
			catch (Exception ex)
			{
				_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Error, ex.Message));
				_busyIndicator.ResetState();
			}
		}

		/// <summary>
		/// If dictionary is not first in list, enable up movement.
		/// </summary>
		public bool CanMoveUpCmd
		{
			get
			{
				bool canMoveUp = false;

				if (SelectedDictionary != null &&
					Dictionaries.IndexOf(SelectedDictionary) > 0)
				{
					canMoveUp = true;
				}

				return canMoveUp;
			}
		}

		/// <summary>
		/// Move dictionary down the list.
		/// </summary>
		public void MoveDownCmd()
		{
			try
			{
				int index = Dictionaries.IndexOf(SelectedDictionary);
				Dictionaries.Swap(index, index + 1);
				SelectedDictionary = Dictionaries[index + 1];
			}
			catch (Exception ex)
			{
				_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Error, ex.Message));
				_busyIndicator.ResetState();
			}
		}

		/// <summary>
		/// If dictionary is not last in list, enable down movement.
		/// </summary>
		public bool CanMoveDownCmd
		{
			get
			{
				bool canMoveDown = false;

				if (SelectedDictionary != null &&
					Dictionaries.IndexOf(SelectedDictionary) < Dictionaries.Count - 1)
				{
					canMoveDown = true;
				}

				return canMoveDown;
			}
		}

		#endregion
	}
}
