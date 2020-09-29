using System.Threading.Tasks;

namespace Sp.DataConnector.Logic.Interfaces
{
    public interface ISpDataIntializer
    {
        Task InitializeDataSync();
    }
}