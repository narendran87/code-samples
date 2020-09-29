using System.Collections.Generic;

namespace Sp.DataConnector.Logic.Models
{
    public class SolrConfig
    {
        public Dictionary<string, string> Mappings { get; set; } = new Dictionary<string, string>();
        public List<string> ListsToBeIndexed { get; set; } = new List<string>();
        public List<string> ChangeTypesToBeIncluded { get; set; } = new List<string>();
    }
}
