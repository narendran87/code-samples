using Microsoft.SharePoint.Client;
using Sp.DataConnector.Logic.Interfaces;
using Sp.DataConnector.Logic.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sp.DataConnector.Logic
{
    public class SpDataIntializer : ISpDataIntializer
    {
        private const string query =
            "<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"FSObjType\" /><Value Type=\"Integer\">0</Value></Eq></Where></Query></View>";

        private readonly IEnumerable<string> _listToBeIndexed;
        private readonly ISharepointEventMonitor _webhookHelper;
        private readonly ILocalCacheManager _localCache;
        private readonly ISpDataHelper _spDataHelper;

        public SpDataIntializer(AppSettings appSettings,
            ISharepointEventMonitor webhookHelper,
            SolrConfig solrConfig,
            ILocalCacheManager localCache,
            ISpDataHelper spDataHelper
            )
        {
            _localCache = localCache;
            _webhookHelper = webhookHelper;
            _listToBeIndexed = solrConfig.ListsToBeIndexed;
            _spDataHelper = spDataHelper;
        }

        public async Task InitializeDataSync()
        {
            var lists = await _spDataHelper.GetLists();
            SubscribeToEvents(lists);
            SendToQueue(lists.Select(t => new SyncObject
            {
                listId = t.Id.ToString(),
                Source = SyncObjectSource.SpDataIntializer
            }));
        }

        private void SubscribeToEvents(IEnumerable<List> lists)
        {
            foreach (var list in lists)
            {
                if (_listToBeIndexed.Contains(list.Title))
                {
                    _webhookHelper.Subscribe(list);
                }
            }
        }

        private void SendToQueue(IEnumerable<SyncObject> syncObjects)
        {
            foreach (var obj in syncObjects)
            {
                _localCache.SyncQueue.Enqueue(obj);
            }
        }
    }
}