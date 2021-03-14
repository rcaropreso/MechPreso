using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    //TODO: Deprecated - jogue fora
    public class NotifyEventArgs : EventArgs
    {
        public string message;
    }

    public interface IMessageComm
    {
        event EventHandler NotifyEvent;

        void SendMessageAsync(string message);
    }
}
