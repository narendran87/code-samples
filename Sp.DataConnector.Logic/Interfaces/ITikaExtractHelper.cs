using System.IO;
using System.Threading.Tasks;

namespace Sp.DataConnector.Logic.Interfaces
{
    public interface ITikaExtractHelper
    {
        Task<string> GetTextFromDoc(MemoryStream memory);
    }
}
