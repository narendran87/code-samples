using Microsoft.SharePoint.Client;
using Sp.DataConnector.Logic.Interfaces;
using Sp.DataConnector.Logic.Models;
using System;
using System.Collections.Generic;

namespace Sp.DataConnector.Logic
{
    public class LocalCacheManager : ILocalCacheManager
    {
        public Queue<SyncObject> SyncQueue { get; set; } = new Queue<SyncObject>();
        public Dictionary<Guid, ChangeToken> ListCache { get; set; } = new Dictionary<Guid, ChangeToken>();
    }
}
