using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SpDataConnectorV02.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndexController : ControllerBase
    {
        [HttpDelete]
        public HttpResponseMessage Delete()
        {
            return new HttpResponseMessage();
        }

        [HttpPut]
        public HttpResponseMessage Put()
        {
            return new HttpResponseMessage();
        }
    }
}