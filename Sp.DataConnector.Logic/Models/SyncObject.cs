namespace Sp.DataConnector.Logic.Models
{
    public class SyncObject
    {
        public string ChangeToken { get; set; }
        public string UniqueId { get; set; }
        public string listId { get; set; }
        public SyncObjectSource Source { get; set; }
        public Document SemanticDocument { get; set; }
    }
}
