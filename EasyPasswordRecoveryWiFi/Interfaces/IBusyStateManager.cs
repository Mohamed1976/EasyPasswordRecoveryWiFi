using EasyPasswordRecoveryWiFi.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyPasswordRecoveryWiFi.Interfaces
{
    public interface IBusyStateManager
    {
        void EnterBusy();
        void ExitBusy();
        void ClearBusy();
        bool IsBusy { get; }
        void SetMessage(SeverityType severityType, string message = null);
        string Message { get; }
        event PropertyChangedEventHandler PropertyChanged;
    }
}
