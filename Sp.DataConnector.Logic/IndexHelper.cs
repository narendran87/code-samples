using Newtonsoft.Json;
using Sp.DataConnector.Logic.Interfaces;
using Sp.DataConnector.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sp.DataConnector.Logic
{
    public class IndexHelper : IIndexHelper
    {
        private readonly Dictionary<string, string> _mappings;
        private readonly string _solrUrl;

        public IndexHelper(SolrConfig solrConfig,
            AppSettings appSettings
            )
        {
            _mappings = solrConfig.Mappings;
            _solrUrl = appSettings.SolrUrl;
        }

        public void Index(IEnumerable<Document> documents)
        {
            var serializedDocuments = ModifyIntoIndexSchema(documents.ToList());

            Parallel.ForEach(serializedDocuments, async t =>
             {
                 var httpClientHandler = new HttpClientHandler();
                 httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                 using (var client = new HttpClient(httpClientHandler))
                 {

                     var req = new HttpRequestMessage(HttpMethod.Post, $"{_solrUrl}/spdocs/update/json/docs?commit=true&overwrite=true")
                     {
                         Content = new StringContent(t, Encoding.UTF8, "application/json")
                     };

                     var res = await client.SendAsync(req);
                     if (!res.IsSuccessStatusCode)
                         throw new Exception($"Indexing of Document {t} failed due to {await res.Content.ReadAsStringAsync()}");
                 }
             });
        }

        private IEnumerable<string> ModifyIntoIndexSchema(List<Document> documents)
        {
            var listOfDicts = new List<Dictionary<string, object>>();

            foreach (var doc in documents)
            {
                var dict = new Dictionary<string, object>();
                foreach (var property in doc.Properties)
                {
                    dict.Add(property.Key, property.Value);
                }
                foreach (var propertyInfo in doc.GetType().GetProperties())
                {
                    dict.Add(propertyInfo.Name, propertyInfo.GetValue(doc));
                }

                //Modify the Dict as per the Index Schema
                dict  = dict.Select(t =>
                {
                    if (_mappings.ContainsKey(t.Key))
                    {
                       return new KeyValuePair<string,object>(_mappings[t.Key], dict[t.Key]);
                    }
                    return t;
                }).ToDictionary(z=> z.Key,z=>z.Value);

                listOfDicts.Add(dict);
            }
            return listOfDicts.Select(t => JsonConvert.SerializeObject(t));
        }



        private string GetSchema()
        {
            return string.Empty;
        }   

        public void Delete(IEnumerable<Document> documents)
        {
            Parallel.ForEach(documents, async t =>
            {
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                using (var client = new HttpClient(httpClientHandler))
                {
                    var req = new HttpRequestMessage(HttpMethod.Post, $"{_solrUrl}/spdocs/update?commit=true&overwrite=true")
                    {
                        Content = new StringContent($"<delete><query>id:{t.Id}</query></delete>", Encoding.UTF8, "text/xml")
                    };

                    var res = await client.SendAsync(req);
                    if (!res.IsSuccessStatusCode)
                        throw new Exception($"Indexing of Document {t} failed due to {await res.Content.ReadAsStringAsync()}");
                }
            });
        }

        public async Task DeleteAll()
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            using (var client = new HttpClient(httpClientHandler))
            {
                
                var req = new HttpRequestMessage(HttpMethod.Post, $"{_solrUrl}/spdocs/update?commit=true")
                {
                    Content = new StringContent($"<delete><query>*:*</query></delete>", Encoding.UTF8, "text/xml")
                };

                var res =await  client.SendAsync(req);
                if (!res.IsSuccessStatusCode)
                    throw new Exception($"Deletion of all documents failed");
            }
        }
    }
}
