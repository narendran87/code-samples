using Sp.DataConnector.Logic.Interfaces;
using Sp.DataConnector.Logic.Models;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sp.DataConnector.Logic
{
    public class TikaExtractHelper : ITikaExtractHelper
    {
        private readonly string _tikaUrl;

        public TikaExtractHelper(AppSettings appSettings)
        {
            _tikaUrl = appSettings.TikaUrl;
        }
        public async Task<string> GetTextFromDoc(MemoryStream memory)
        {
            HttpClient client = new HttpClient();
            var task = await client.PutAsync($"{_tikaUrl}/tika", new ByteArrayContent(memory.ToArray()));
            return await task.Content.ReadAsStringAsync();
        }
    }
}
