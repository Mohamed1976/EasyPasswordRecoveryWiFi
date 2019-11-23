using EasyPasswordRecoveryWiFi.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Interfaces
{
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Modifies string casing of words found in dictionaries (lower, UPPER, Title).  
        /// </summary>
        StringCasing DictionaryCasing { get; set; }
        
        /// <summary>
        /// Connection timeout used when connecting to access point.
        /// </summary>
        int Timeout { get; set; }

        /// <summary>
        /// Minimum signal strength of access point to be displayed in the wifi search view.
        /// </summary>
        int Threshold { get; set; }

        /// <summary>
        /// File name of the password storage file.
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Directory where the password storage file will be stored.
        /// </summary>
        string PasswordStorageDir { get; set; }

        /// <summary>
        /// Overwrite password storage file if found on storage location.
        /// </summary>
        bool OverwriteFile { get; set; }

        /// <summary>
        /// Last visited dictionaries import directory.
        /// </summary>
        string DictionariesDir { get; set; }

        /// <summary>
        /// Last visited profile import directory.
        /// </summary>
        string ImportDir { get; set; }

        /// <summary>
        /// Last visited profile export directory.
        /// </summary>
        string ExportDir { get; set; }
    }
}
