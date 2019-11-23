using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Interfaces;
using System;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace EasyPasswordRecoveryWiFi.ViewModels
{
    public class SettingsViewModel : Screen, IShell
	{
		#region [ Constructor ]

		public SettingsViewModel(IConfigurationProvider configurationProvider)
		{
            ConfigurationProvider = configurationProvider;
        }

        #endregion

        #region [ Injected IConfigurationProvider ]

        /// <summary>
        /// Reference to ConfigurationProvider which stores and provides all application settings.    
        /// </summary>
        public IConfigurationProvider ConfigurationProvider { get; private set; }

        #endregion

		#region [ Command handlers ]

		/// <summary>
		/// Select folder for password storage file.
		/// The following Microsoft Nuget Package are needed in order to use CommonOpenFileDialog.
		/// Microsoft.WindowsAPICodePack-Core
		/// Microsoft.WindowsAPICodePack-Shell
		/// </summary>
		public void SelectFolderCmd()
		{
			CommonOpenFileDialog dialog = new CommonOpenFileDialog();
			dialog.InitialDirectory = ConfigurationProvider.PasswordStorageDir;
			dialog.IsFolderPicker = true;
			if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
			{
                ConfigurationProvider.PasswordStorageDir = dialog.FileName;
			}
		}

		#endregion
	}
}
