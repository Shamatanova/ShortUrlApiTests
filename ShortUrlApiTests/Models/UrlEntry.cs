using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShortURLsPostmanRequests
{
    public class UrlEntry
    {
        public string Url { get; set; }
        public string ShortCode { get; set; }
        public string ShortURL { get; set; }
        public string DateCreated { get; set; }
        public string Visits { get; set; }
    }
}
