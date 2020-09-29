using Microsoft.SharePoint.Client;
using System.Collections.Generic;
using System.Net.Http;

namespace Sp.DataConnector.Logic.Models
{
    public class Document
    {
        public string id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string DocumentType { get; set; }
        public ByteArrayContent File { get; set; }
        public string SampleQuestions { get; set; }
        public string MetaTags { get; set; }
        public string Author { get; set; }
        public string AboutTheAuthor { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public string Description { get; set; }
        public string FileExtension { get; set; }
    }
}