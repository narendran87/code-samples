using Microsoft.SharePoint.Client;
using Sp.DataConnector.Logic.Interfaces;
using Sp.DataConnector.Logic.Models;
using System.Linq;
using System.Threading;

namespace Sp.DataConnector.Logic
{
    public class SharepointEventMonitor : ISharepointEventMonitor
    {
        private readonly ILocalCacheManager _localCache;

        public Timer SpCheckTimer { get; set; }

        public SharepointEventMonitor(
            ILocalCacheManager localCacheManager)
        {
            _localCache = localCacheManager;
            SpCheckTimer = new Timer(new TimerCallback(RetrieveSpChanges), null, 10000, 100000);
        }

        public void Subscribe(List list)
        {
            if (!_localCache.ListCache.Keys.Contains(list.Id))
            {
                _localCache.ListCache.Add(list.Id, list.CurrentChangeToken);
            }
            else
            {
                _localCache.ListCache[list.Id] = list.CurrentChangeToken;
            }
        }

        public void RetrieveSpChanges(object state)
        {
            foreach (var id in _localCache.ListCache.Keys)
            {
                _localCache.SyncQueue.Enqueue(new SyncObject
                {
                    ChangeToken = _localCache.ListCache[id].StringValue,
                    listId = id.ToString(),
                    Source = SyncObjectSource.SharepointEventMonitor
                });
            }
        }
    }
}