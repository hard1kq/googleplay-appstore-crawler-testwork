using System.Collections.Generic;

namespace StoreParsers.Domain
{
    public class Application
    {
        public int ApplicationId { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public double Rating { get; set; }
        public SearchRequest SearchRequest { get; set; }
        public int SearchRequestId { get; set; }
        public ICollection<Screenshot> Screenshots { get; set; }
    }
}
