using Microsoft.SharePoint.Client;
using Sp.DataConnector.Logic.Models;
using System;
using System.Collections.Generic;

namespace Sp.DataConnector.Logic.Interfaces
{
    public interface ILocalCacheManager
    {
        Queue<SyncObject> SyncQueue { get; set; }
        Dictionary<Guid, ChangeToken> ListCache { get; set; }
    }
}
