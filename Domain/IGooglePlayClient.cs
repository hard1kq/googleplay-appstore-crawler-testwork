using System.Collections.Generic;
using System.Threading.Tasks;

namespace StoreParsers.Domain
{
    public interface IGooglePlayClient
    {
        Task<List<string>> GetTop3SearchSuggestAsync(string keyword);
        Task<Application> GetApplicationInfo(string packageName);
    }
}
