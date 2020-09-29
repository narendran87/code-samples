using System.Threading;

namespace Sp.DataConnector.Logic.Interfaces
{
    public interface IQueueProcessor
    {
        Timer Timer { get; set; }
        void CheckQueue(object state);
    }
}
