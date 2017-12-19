using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigData.ES
{
    public class Config
    {
        /// <summary>
        /// ES node ip-port uris, seperated by comma.
        /// </summary>
        public string ES_CONN_STR { get; set; }
        public string ES_INDEX { get; set; }
        public string ES_TYPE { get; set; }

    }
}
