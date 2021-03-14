using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Utils
{
    //TODO: Deprecated - jogar fora
    public class AsyncMessageCallback : IMessageComm
    {
        public event EventHandler NotifyEvent;

        public AsyncMessageCallback()
        {

        }

        public void SendMessageAsync(string message)
        {
            NotifyEventArgs e = new NotifyEventArgs
            {
                message = message
            };

            NotifyEvent?.Invoke(this, e);
        }
    }
}
