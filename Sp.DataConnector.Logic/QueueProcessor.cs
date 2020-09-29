using Microsoft.SharePoint.Client;
using Sp.DataConnector.Logic.Interfaces;
using Sp.DataConnector.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using File = Microsoft.SharePoint.Client.File;

namespace Sp.DataConnector.Logic
{
    public class QueueProcessor : IQueueProcessor
    {
        private readonly ISpDataHelper _spDataHelper;
        private readonly ITikaExtractHelper _tikaExtractHelper;
        private readonly IIndexHelper _indexHelper;
        private readonly ILocalCacheManager _localCache;
        private readonly List<string> _configuredChangeTypes;

        public Timer Timer { get; set; }
        public Queue<Task> Tasks { get; set; } = new Queue<Task>();

        public QueueProcessor(ISpDataHelper spDataHelper,
            ITikaExtractHelper tikaExtractHelper,
            IIndexHelper indexHelper,
            ILocalCacheManager localCache,
            AppSettings appSettings,
            SolrConfig spConfig,
            TimerConfig timerConfig
            )
        {
            Timer = new Timer(new TimerCallback(CheckQueue), null, timerConfig.TimerStartInMs, timerConfig.TimerIntervalInMs);
            _configuredChangeTypes = spConfig.ChangeTypesToBeIncluded;
            _spDataHelper = spDataHelper;
            _localCache = localCache;
            _tikaExtractHelper = tikaExtractHelper;
            _indexHelper = indexHelper;
        }

        public void CheckQueue(object state)
        {
            var syncObjects = new List<SyncObject>();
            var removeTask = CheckAndRemoveCompletedTasks();

            while (_localCache.SyncQueue.Count > 0)
            {
                syncObjects.Add(_localCache.SyncQueue.Dequeue());
            }
            var queuedObjFromIntializer = syncObjects.Where(z => z.Source.Equals(SyncObjectSource.SpDataIntializer));
            var queuedObjFromEventMonitor = syncObjects.Where(z => z.Source.Equals(SyncObjectSource.SharepointEventMonitor));

            Tasks.Enqueue(removeTask);
            if (queuedObjFromIntializer.Any())
            {
                Tasks.Enqueue(ProcessSyncObjectsBatchFromConnector(queuedObjFromIntializer));
            }
            if (queuedObjFromEventMonitor.Any())
            {
                //Tasks.Enqueue(ProcessSyncObjectsFromEventMonitor(queuedObjFromEventMonitor));
            }
        }

        private async Task CheckAndRemoveCompletedTasks()
        {
            bool flag = true;
            while (flag)
            {
                if (Tasks.Count > 0)
                {
                    var t = Tasks.Peek();
                    if (t.IsCompleted)
                    {
                        await Tasks.Dequeue();
                        flag = true;
                        continue;
                    }

                }
                flag = false;
            }
        }

        private async Task ProcessSyncObjectsBatchFromConnector(IEnumerable<SyncObject> syncObjects)
        {
            var docs = new List<Document>();
            foreach (var syncObject in syncObjects)
            {
                var fileList = await _spDataHelper.GetAllFiles(Guid.Parse(syncObject.listId));
                foreach (var file in fileList)
                {
                    docs.Add(await GetDocument(file));
                }
            }
            _indexHelper.Index(docs);
        }

        //private async Task ProcessSyncObjectsFromEventMonitor(IEnumerable<SyncObject> syncObjects)
        //{
        //    var docs = new List<Document>();
        //    var t = new List<Task>();
        //    foreach (var syncObject in syncObjects)
        //    {
        //        var dict = await _spDataHelper.GetChanges(Guid.Parse(syncObject.listId));
        //        foreach (var key in dict.Keys)
        //        {
        //            t.Add((GetItemProcessingTask(key))(dict[key]));
        //        }
        //    }
        //    await Task.WhenAll(t);
        //    //_indexHelper.Index(docs);
        //}

        //private Func<List<File>, Task> GetItemProcessingTask(ChangeType changeType)
        //{
        //    switch (changeType)
        //    {
        //        case ChangeType.DeleteObject: return DeleteDocuments;
        //        default: return IndexDocuments;
        //    }
        //}

        //private async Task IndexDocuments(List<File> files)
        //{
        //    _indexHelper.Index(await GetDocuments(files));
        //}

        //private async Task DeleteDocuments(List<File> files)
        //{
        //    _indexHelper.Delete(await GetDocuments(files));
        //}

        private async Task<Document> GetDocument(File file)
        {
            return new Document
            {
                Author = file.Author.Title,
                id = file.UniqueId.ToString(),
                Url = file.LinkingUrl,
                Properties = file.Properties.FieldValues,
                Title = CheckValueIsPresentAndReturn(file.Name, file.Title),
                File = new ByteArrayContent((await _spDataHelper.GetStream(file)).ToArray()),
                FileExtension = file.Name.Split('.').LastOrDefault()
            };

        }

        private string CheckValueIsPresentAndReturn(string name, string title)
        {
            if (!string.IsNullOrWhiteSpace(title) && !title.Trim().Equals("undefined"))
            {
                return title;
            }
            return name.Split('.')[0];
        }

    }
}
