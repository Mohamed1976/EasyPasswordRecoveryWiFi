using EasyPasswordRecoveryWiFi.Common;
using EasyPasswordRecoveryWiFi.Interfaces;
using System.IO;
using System;
using Caliburn.Micro;

namespace EasyPasswordRecoveryWiFi.Helpers
{
    public class ConfigurationProvider : PropertyChangedBase, IConfigurationProvider
    {
        #region [ IConfigurationProvider interface implementation ]

        /// <summary>
        /// Modifies string casing of words found in dictionaries (lower, UPPER, Title).  
        /// </summary>
        public StringCasing DictionaryCasing
        {
            get { return (StringCasing)Settings.Default.DictionaryCasing; }
            set
            {
                Settings.Default.DictionaryCasing = (int)value;
                Settings.Default.Save();
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Connection timeout used when connecting to access point.
        /// </summary>
        public int Timeout
        {
            get { return Settings.Default.Timeout; }
            set
            {
                Settings.Default.Timeout = value;
                Settings.Default.Save();
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Minimum signal strength of access point to be displayed in the WiFi search view.
        /// </summary>
        public int Threshold
        {
            get { return Settings.Default.Threshold; }
            set
            {
                Settings.Default.Threshold = value;
                Settings.Default.Save();
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// File name of the password storage file.
        /// </summary>
        public string FileName
        {
            get { return Settings.Default.FileName; }
            set
            {
                if (FileNameIsValid(value))
                {
                    Settings.Default.FileName = value;
                    Settings.Default.Save();
                    NotifyOfPropertyChange();
                }
            }
        }

        /// <summary>
        /// Directory where the password storage file will be stored.
        /// </summary>
        public string PasswordStorageDir
        {
            get
            {
                string filePath = null;

                if (string.IsNullOrEmpty(Settings.Default.FilePath))
                {
                    filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                else
                {
                    filePath = Settings.Default.FilePath;
                }

                return filePath;
            }
            set
            {
                if (Directory.Exists(value))
                {
                    Settings.Default.FilePath = value;
                    Settings.Default.Save();
                    NotifyOfPropertyChange();
                }
            }
        }

        /// <summary>
        /// Overwrite password storage file if found on storage location.
        /// </summary>
        public bool OverwriteFile
        {
            get { return Settings.Default.OverwriteFile; }
            set
            {
                Settings.Default.OverwriteFile = value;
                Settings.Default.Save();
                NotifyOfPropertyChange();
            }
        }

        /// <summary>
        /// Last visited dictionaries import directory.
        /// </summary>
        public string DictionariesDir
        {
            get
            {
                string dictionariesDir = null;

                if (string.IsNullOrEmpty(Settings.Default.DictionariesDir))
                {
                    dictionariesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                else
                {
                    dictionariesDir = Settings.Default.DictionariesDir;
                }

                return dictionariesDir;
            }
            set
            {
                if (Directory.Exists(value))
                {
                    Settings.Default.DictionariesDir = value;
                    Settings.Default.Save();
                    NotifyOfPropertyChange();
                }
            }
        }

        /// <summary>
        /// Last visited profile import directory.
        /// </summary>
        public string ImportDir
        {
            get
            {
                string importDir = null;

                if (string.IsNullOrEmpty(Settings.Default.ImportDir))
                {
                    importDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                else
                {
                    importDir = Settings.Default.ImportDir;
                }

                return importDir;
            }
            set
            {
                if (Directory.Exists(value))
                {
                    Settings.Default.ImportDir = value;
                    Settings.Default.Save();
                    NotifyOfPropertyChange();
                }
            }
        }

        /// <summary>
        /// Last visited profile export directory.
        /// </summary>
        public string ExportDir
        {
            get
            {
                string exportDir = null;

                if (string.IsNullOrEmpty(Settings.Default.ExportDir))
                {
                    exportDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }
                else
                {
                    exportDir = Settings.Default.ExportDir;
                }

                return exportDir;
            }
            set
            {
                if (Directory.Exists(value))
                {
                    Settings.Default.ExportDir = value;
                    Settings.Default.Save();
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Determines whether the file name is valid and does not contain any invalid characters.  
        /// </summary>
        /// <param name="fileName">FileName to check for validity.</param>
        /// <returns>True if file name is valid, false otherwise.</returns>
        private bool FileNameIsValid(string fileName)
        {
            bool isValid = false;

            /*  -1 if no character in IndexOfAny was found. */
            if (!string.IsNullOrEmpty(fileName) &&
                fileName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1)
            {
                isValid = true;
            }

            return isValid;
        }

        #endregion

    }
}
