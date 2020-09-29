using Microsoft.SharePoint.Client;
using Sp.DataConnector.Logic.Interfaces;
using Sp.DataConnector.Logic.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using File = Microsoft.SharePoint.Client.File;

namespace Sp.DataConnector.Logic
{
    public class SpDataHelper : ISpDataHelper
    {
        private const string query =
          "<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name=\"FSObjType\" /><Value Type=\"Integer\">0</Value></Eq></Where></Query></View>";
        private readonly IEnumerable<string> _listToBeIndexed;
        private readonly ClientContext _context;
        private readonly ILocalCacheManager _localCache;

        public SpDataHelper(AppSettings appSettings,
            ILocalCacheManager localCache,
            SpCredentials spCredentials,
            SolrConfig solrConfig,
            ClientContext clientContext)
        {
            _listToBeIndexed = solrConfig.ListsToBeIndexed;
            _context = clientContext;
            _localCache = localCache;
        }

        public async Task<Dictionary<ChangeType, List<File>>> GetChanges(Guid listId)
        {
            try
            {
                var token = _localCache.ListCache[listId];

                var lists = _context.LoadQuery(_context.Web.Lists.Where(l => l.BaseType == BaseType.DocumentLibrary));
                await _context.ExecuteQueryAsync();
                var list = lists.Where(t => t.Id.Equals(listId)).FirstOrDefault();
                var changes = list.GetChanges(GetChangeQuery(token.StringValue));

                _context.Load(changes);

                await _context.ExecuteQueryAsync();
                return await GetChangedFiles(changes, list);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<Dictionary<ChangeType, List<File>>> GetChangedFiles(ChangeCollection changes, List list)
        {
            var changeDict = new Dictionary<ChangeType, List<File>>();
            foreach (ChangeItem change in changes)
            {
                if (!changeDict.ContainsKey(change.ChangeType))
                {
                    changeDict.Add(change.ChangeType, new List<File>());
                }
                var item = list.GetItemById(change.ItemId);
                _context.Load(item, i => i.File.LinkingUrl, i => i.File,
                    i => i.File.Author,
                    i => i.Id,
                    i => i.File.LinkingUrl);
                changeDict[change.ChangeType].Add(list.GetItemById(change.ItemId).File);
            }
            await _context.ExecuteQueryAsync();
            return changeDict;
        }

        private ChangeQuery GetChangeQuery(string token)
        {
            var cq = new ChangeQuery(false, false);
            cq.Item = true;
            cq.Add = true;
            cq.DeleteObject = true;
            cq.Move = true;
            cq.Restore = true;
            cq.Update = true;
            cq.ChangeTokenStart = new ChangeToken() { StringValue = token };
            return cq;
        }

        private CamlQuery CreateAllFilesQuery()
        {
            var qry = new CamlQuery();
            qry.ViewXml = query;
            return qry;
        }

        public async Task<IEnumerable<List>> GetLists()
        {
            var lists = _context.LoadQuery(_context.Web.Lists.Where(l => l.BaseType == BaseType.DocumentLibrary));
            await _context.ExecuteQueryAsync();
            return lists.Where(t => _listToBeIndexed.Contains(t.Title));
        }

        public async Task<List<File>> GetAllFiles(Guid listId)
        {
            var t = new List<Task>();
            var results = new Dictionary<string, IEnumerable<SyncObject>>();
            ListItemCollection items;

            var list = _context.LoadQuery(_context.Web.Lists.Where(l => l.BaseType == BaseType.DocumentLibrary));
            await _context.ExecuteQueryAsync();

            items = list.Where(z => z.Id.Equals(listId))
                .FirstOrDefault()
                .GetItems(CreateAllFilesQuery());

            _context.Load(items, icol => icol.Include(i => i.File),
                icol => icol.Include(i => i.File.Author),
                icol => icol.Include(i => i.Id),
                icol => icol.Include(i => i.ServerRedirectedEmbedUrl));
            await _context.ExecuteQueryAsync();

            return items.Select(i => i.File).ToList();
        }

        public async Task<MemoryStream> GetStream(File file)
        {
            var stream = file.OpenBinaryStream();
            await _context.ExecuteQueryAsync();
            var memoryStream = new MemoryStream();
            stream.Value.CopyTo(memoryStream);
            return memoryStream;

        }
    }
}
