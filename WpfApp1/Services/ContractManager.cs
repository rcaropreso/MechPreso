using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KRPC.Client;
using KRPC.Client.Services.SpaceCenter;
using WpfApp1.Services;
using WpfApp1.Utils;
using WpfApp1.Models;

namespace WpfApp1
{
    public class ContractItem
    {
        public string title {get; set;}
        public string synopsis { get; set; }
        public string desc { get; set; }

        public IList<string> parameters = new List<string>();
    }

    public class ContractManager : IAsyncMessageUpdate
    {
        private Connection m_conn = null;
        public IList<ContractItem> contracts = new List<ContractItem>();

        public ContractManager(Connection conn)
        {
            m_conn = conn;
        }

        public void SetupContractInfo()
        {
            var cm = m_conn.SpaceCenter().ContractManager;

            foreach(Contract contract in cm.ActiveContracts)
            {
                ContractItem item = new ContractItem()
                {
                    title = contract.Title,
                    synopsis = contract.Synopsis,
                    desc = contract.Description,
                };

                foreach(ContractParameter param in contract.Parameters)
                {
                    item.parameters.Add(param.Title);
                    if(param.Children.Count >  0)
                    {
                        foreach(ContractParameter childParam in param.Children)
                        {
                            item.parameters.Add(childParam.Title);
                        }
                    }
                }
            }
        }

        public void SendMessage(string strMessage)
        {
            Console.WriteLine(strMessage);
            Mediator.Notify(CommonDefs.MSG_SEND_MESSAGE, strMessage);
        }
    }
}
