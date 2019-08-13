using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Helpers;
using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Messages;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace EasyPasswordRecoveryWiFi.ViewModels
{
	sealed class RegexViewModel : Screen, IPasswordProvider, IShell
	{
		#region [ Injected instances ] 

		private readonly IEventAggregator _eventAggregator = null;
		private readonly AddRegexViewModel _addRegexViewModel = null;
		private readonly IWindowManager _windowManager = null;
		private readonly IRegExService _regExService = null;
		private readonly IBusyIndicator _busyIndicator = null;

		#endregion

		#region [ Constructor ]

		public RegexViewModel(IWindowManager windowManager,
			IEventAggregator eventAggregator,
			AddRegexViewModel addRegexViewModel,
			IRegExService regExService,
			IBusyIndicator busyIndicator)
		{
			_windowManager = windowManager;
			_eventAggregator = eventAggregator;
			_addRegexViewModel = addRegexViewModel;
			_regExService = regExService;
			_busyIndicator = busyIndicator;
			PasswordRegExs = new ObservableCollection<PasswordRegEx>();
			DisplayName = "Smart";
		}

		#endregion

		#region [ IPasswordProvider members ] 

		int index = 0;
		/// <summary>
		/// The GetFirst method returns the first string that meets the regular expression.
		/// Returns null if no string meets the regular expression. 
		/// </summary>
		/// <remarks>
		/// The GetFirst method rewinds the RegEx generator. 
		/// </remarks>
		public string GetFirst()
		{
			string password = null;

			index = 0;
			if (PasswordRegExs.Count > 0)
			{
				password = PasswordRegExs[index].GetFirst();
			}

			return password;
		}

		/// <summary>
		/// The GetNext method returns the next string generated using RegEx.
		/// Returns null if no more strings can be generated using the RegEx.
		/// </summary>
		public string GetNext()
		{
			string password = PasswordRegExs[index].GetNext();

			if (password == null)
			{
				/* Move to the next RegEx. */
				if (index + 1 < PasswordRegExs.Count)
				{
					password = PasswordRegExs[++index].GetFirst();
				}
			}

			return password;
		}

		/// <summary>
		/// Determines whether the RegEx collection is empty.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return PasswordRegExs.Count == 0;
			}
		}

		#endregion

		#region [ Properties ]

		private ObservableCollection<PasswordRegEx> passwordRegExs = null;
		/// <summary>
		/// Collection of regular expressions used to generate passwords.
		/// </summary>
		public ObservableCollection<PasswordRegEx> PasswordRegExs
		{
			get { return passwordRegExs; }
			set
			{
				if (Set(ref passwordRegExs, value))
				{
					NotifyOfPropertyChange();
				}
			}
		}

		private PasswordRegEx selectedRegEx = null;
		/// <summary>
		/// The selected regular expression.
		/// </summary>
		public PasswordRegEx SelectedRegEx
		{
			get { return selectedRegEx; }
			set
			{
				selectedRegEx = value;
				NotifyOfPropertyChange();
				NotifyOfPropertyChange(nameof(CanRemoveRegExCmd));
				NotifyOfPropertyChange(nameof(CanMoveUpCmd));
				NotifyOfPropertyChange(nameof(CanMoveDownCmd));
			}
		}

		#endregion

		#region [ Command handlers ]

		/// <summary>
		/// Adds a regular expression using addRegexView.  
		/// </summary>
		public void AddRegExCmd()
		{
			try
			{
				bool dialogResult = _windowManager.ShowDialog(_addRegexViewModel) ?? false;
				if (dialogResult)
				{
					PasswordRegExs.Add(new PasswordRegEx(_regExService, _addRegexViewModel.RegEx));
					/* Select first RegEx in list. */
					SelectedRegEx = PasswordRegExs.FirstOrDefault();
				}
			}
			catch (Exception ex)
			{
				_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Error, ex.Message));
				_busyIndicator.ResetState();
			}
		}

		/// <summary>
		/// Removes the selected regular expression.  
		/// </summary>
		public void RemoveRegExCmd()
		{
			try
			{
				/* Index needed to restore the selected RegEx position. */
				int index = PasswordRegExs.IndexOf(SelectedRegEx);
				PasswordRegExs.Remove(SelectedRegEx);

				/* Restore the selected RegEx position. */
				if (PasswordRegExs.Count > 0 && index < PasswordRegExs.Count)
				{
					SelectedRegEx = PasswordRegExs[index];
				}
				else if (PasswordRegExs.Count > 0 && index == PasswordRegExs.Count)
				{
					SelectedRegEx = PasswordRegExs[index - 1];
				}
			}
			catch (Exception ex)
			{
				_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Error, ex.Message));
				_busyIndicator.ResetState();
			}
		}

		/// <summary>
		/// If a RegEx is selected, enable remove command.
		/// </summary>
		public bool CanRemoveRegExCmd
		{
			get
			{
				bool canRemove = false;

				if (SelectedRegEx != null)
				{
					canRemove = true;
				}

				return canRemove;
			}
		}

		/// <summary>
		/// Move RegEx up the list.
		/// </summary>
		public void MoveUpCmd()
		{
			try
			{
				int index = PasswordRegExs.IndexOf(SelectedRegEx);
				PasswordRegExs.Swap(index, index - 1);
				SelectedRegEx = PasswordRegExs[index - 1];
			}
			catch (Exception ex)
			{
				_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Error, ex.Message));
				_busyIndicator.ResetState();
			}
		}

		/// <summary>
		/// If RegEx is not first in list, enable up movement.
		/// </summary>
		public bool CanMoveUpCmd
		{
			get
			{
				bool canMoveUp = false;

				if (SelectedRegEx != null &&
					PasswordRegExs.IndexOf(SelectedRegEx) > 0)
				{
					canMoveUp = true;
				}

				return canMoveUp;
			}
		}

		/// <summary>
		/// Move RegEx down the list.
		/// </summary>
		public void MoveDownCmd()
		{
			try
			{
				int index = PasswordRegExs.IndexOf(SelectedRegEx);
				PasswordRegExs.Swap(index, index + 1);
				SelectedRegEx = PasswordRegExs[index + 1];
			}
			catch (Exception ex)
			{
				_eventAggregator.PublishOnUIThread(new StatusMsg(SeverityType.Error, ex.Message));
				_busyIndicator.ResetState();
			}
		}

		/// <summary>
		/// If RegEx is not last in list, enable down movement.
		/// </summary>
		public bool CanMoveDownCmd
		{
			get
			{
				bool canMoveDown = false;

				if (SelectedRegEx != null &&
					PasswordRegExs.IndexOf(SelectedRegEx) < PasswordRegExs.Count - 1)
				{
					canMoveDown = true;
				}

				return canMoveDown;
			}
		}

		#endregion
	}
}
