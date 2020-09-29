using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sp.DataConnector.Logic.Models;

namespace Sp.DataConnector.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolrConfigController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<SolrConfig> Get()
        {
            return new SolrConfig();
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public ActionResult<SolrConfig> Put(SolrConfig solrConfig)
        {
            return null;
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete()
        {
        }
    }
}