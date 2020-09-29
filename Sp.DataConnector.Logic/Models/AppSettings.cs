namespace Sp.DataConnector.Logic.Models
{
    public class AppSettings
    {
        public string Domain { get; set; }
        public string Url { get; set; }
        public string PipelineUrl { get; set; }
        public string TikaUrl { get; set; }
        public string WebhookListenerUrl { get; set; }
    }
}
