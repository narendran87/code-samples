using Microsoft.SharePoint.Client;
using System.Threading;

namespace Sp.DataConnector.Logic.Interfaces
{
    public interface ISharepointEventMonitor
    {
        Timer SpCheckTimer { get; set; }
        void Subscribe(List list);
        void RetrieveSpChanges(object state);
    }
}
