using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using File = Microsoft.SharePoint.Client.File;

namespace Sp.DataConnector.Logic.Interfaces
{
    public interface ISpDataHelper
    {
        Task<Dictionary<ChangeType, List<File>>> GetChanges(Guid listId);
        Task<List<File>> GetAllFiles(Guid listId);
        Task<MemoryStream> GetStream(File file);
        Task<IEnumerable<List>> GetLists();
    }
}
