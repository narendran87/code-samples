using Sp.DataConnector.Logic.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sp.DataConnector.Logic.Interfaces
{
    public interface IIndexHelper
    {
        void Index(IEnumerable<Document> documents);
        void Delete(IEnumerable<Document> documents);
        Task DeleteAll();
    }
}
