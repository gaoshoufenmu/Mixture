using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebUI.Controllers
{
    public class PYUtilController : ApiController
    {
        // POST api/util
        public void Post([FromBody]string value)
        {
        }


        //[HttpGet]
        // GET api/values/5
        public string Get(string pinyin)
        {
            return "Origin Input: " + pinyin;
        }
    }
}
