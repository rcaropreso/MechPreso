using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Services
{
    interface IAsyncMessageUpdate
    {
        void SendMessage(string strMessage);
    }
}
