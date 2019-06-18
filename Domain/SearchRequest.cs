using System.Collections.Generic;

namespace StoreParsers.Domain
{
    public class SearchRequest
    {
        public int SearchRequestId { get; set; }
        public string Keyword { get; set; }
        public ICollection<Application> Applications{ get; set; }
    }
}
