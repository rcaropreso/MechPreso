using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.Utils;

namespace WpfApp1.ViewModel
{
    class ConnectionViewModel : BaseViewModel, IPageViewModel
    {
        private ConnectionProxy _connProxy;
        private MissionController _missionController;
        private ICommand _connect;
        private ICommand _disconnect;

        private string _IPAddress = "127.0.0.1";
        private string _Port = "50000";
        

        public string IPAddress
        {
            get { return _IPAddress; }
            set { _IPAddress = value; }
        }

        public string Port
        {
            get { return _Port; }
            set { _Port = value; }
        }

        public ICommand Connect
        {
            get
            {
                return _connect ?? (_connect = new RelayCommand(x =>
                {
                    Mediator.Notify("ClearScreen", "");
                    Mediator.Notify("Connect", "");
                }));
            }
        }

        public ICommand Disconnect
        {
            get
            {
                return _disconnect ?? (_disconnect = new RelayCommand(x =>
                {
                    Mediator.Notify("Disconnect", "");
                }));
            }
        }

        public MissionController OnConnect()
        {
            _connProxy = _connProxy ?? (new ConnectionProxy("My Connection", IPAddress, int.Parse(Port), int.Parse(Port) + 1));

            if (_connProxy.IsConnected())
            {
                StringBuilder strMessage = new StringBuilder();
                strMessage.AppendFormat("Connected on KRPC version: {0}", _connProxy.GetVersion());
                SendMessage(strMessage.ToString());

                _missionController = _missionController ?? new MissionController(_connProxy);

            }
            return _missionController;
        }

        public void OnDisconnect()
        {
            if (!HasValidData())
            {
                return;
            }

            _missionController?.ShipControl?._flightTelemetry?.StopAllTelemetry();
            Mediator.Notify("StopTimers", "");

            _connProxy?.CloseConnection();

            _connProxy = null;
            _missionController = null;

            GC.Collect();

            SendMessage("Disconnected.");
        }

        public bool HasValidData()
        {
            bool bRet = true;

            if (_connProxy == null || !_connProxy.IsConnected())
            {
                SendMessage("No connection available.");
                bRet = false;
            }
            else if (_missionController == null)
            {
                SendMessage("No mission controller available");
                bRet = false;
            }

            return bRet;
        }

        public void SendMessage(string strMessage)
        {
            Mediator.Notify("SendMessage", strMessage.ToString());
        }
    }
}
