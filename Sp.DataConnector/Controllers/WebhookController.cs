using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sp.DataConnector.Logic;
using Sp.DataConnector.Logic.Interfaces;
using Sp.DataConnector.Logic.Models;
using Sp.DataConnector.Models;

namespace SSpDataConnectorV02.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly ILocalCacheManager _localCache;

        public WebhookController(ILocalCacheManager localCache)
        {
            _localCache = localCache;
        }

        [HttpPost]
        public string Post()
        {
            var queryStringParams = HttpUtility.ParseQueryString(Request.QueryString.Value);
            var response = new HttpResponseMessage();
            if (queryStringParams.AllKeys.Contains("validationtoken"))
            {
                var validationToken = queryStringParams.GetValues("validationtoken")[0].ToString();
                response.Content = new StringContent(validationToken);
                return validationToken;
            }
            else
            {
                var body = new StreamReader(Request.Body).ReadToEnd();
                var notifications = JsonConvert.DeserializeObject<NotificationApiModel>(body).Value;
                foreach (var notification in notifications)
                {
                    _localCache.SyncQueue.Enqueue(new SyncObject
                    {
                        listId = notification.Resource,
                        Source = SyncObjectSource.SharepointEventMonitor
                    }); 
                }
                return string.Empty;
            }
        }

        [HttpGet]
        public void Get()
        {

        }
    }
}
