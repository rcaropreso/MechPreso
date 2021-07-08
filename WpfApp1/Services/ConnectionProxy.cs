using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KRPC.Client;
using KRPC.Client.Services.KRPC;
using WpfApp1.Services;
using WpfApp1.Utils;
using WpfApp1.Models;

namespace WpfApp1
{
    public class ConnectionProxy : IAsyncMessageUpdate
    {
        private IPAddress _address;
        private string _name;
        private int _port;
        private int _streamport;

        public Connection _connection { get; }
        private Service m_krpc;

        public ConnectionProxy(string name, string address, int port = 1000, int streamport = 1001)
        {
            _address = IPAddress.Parse(address);
            _name = name;
            _port = port;
            _streamport = streamport;

            try
            {
                _connection = new Connection(name: _name, address: _address, rpcPort: _port, streamPort: _streamport);
                Connect();
            }
            catch (SocketException e)
            {
                MethodBase m = MethodBase.GetCurrentMethod();
                StringBuilder strMessage = new StringBuilder();
                strMessage.AppendFormat("Exception on {0}{1}:{2}", m.ReflectedType.Name, m.Name, e.Message);
                SendMessage(strMessage.ToString());
            }
        }

        private bool Connect()
        {
            StringBuilder strMessage = new StringBuilder();
            bool bRet = true;
            if (_connection == null)
            {
                MethodBase m = MethodBase.GetCurrentMethod();                
                strMessage.AppendFormat("Null connection pointer on {0}{1}", m.ReflectedType.Name, m.Name);
                SendMessage(strMessage.ToString());
                bRet = false;
            }
            else
            {
                m_krpc = _connection.KRPC();
                strMessage.AppendFormat("Version: {0}", GetVersion());
                SendMessage(strMessage.ToString());
            }

            return bRet;
        }

        public bool IsConnected()
        {
            return _connection != null;
        }

        public void CloseConnection()
        {
            //Fechar todas as threads
            //Fechar todos os streams
            _connection.Dispose();
        }

        public string GetVersion()
        {
            return m_krpc?.GetStatus().Version;
        }

        public void SendMessage(string strMessage)
        {
            Console.WriteLine(strMessage);
            Mediator.Notify(CommonDefs.MSG_SEND_MESSAGE, strMessage);
        }
    }
}
