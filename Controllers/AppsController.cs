using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreParsers.Domain;
using StoreParsers.Infrastructure;

namespace StoreParsers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppsController : ControllerBase
    {
        private readonly IGooglePlayClient _googlePlayClient;
        private readonly IAppStoreClient _appStoreClient;

        public AppsController(IGooglePlayClient googlePlayClient, IAppStoreClient appStoreClient)
        {
            _appStoreClient = appStoreClient;
            _googlePlayClient = googlePlayClient;
        }

        // GET api/values
        //[HttpGet]
        [Route("/api/v1/GetAppsInfoByKeyword")]
        [HttpGet]
        public async Task<IActionResult> GetAppsInfoByKeyword(string keyword)
        {
            var getGooglePlayAppsTask = GetGooglePlayApplicationsAsync(keyword);
            var getAppStoreAppsTask = GetAppStoreApplicationsAsync(keyword);
            var googlePlayApps = await getGooglePlayAppsTask;
            var appStoreApps = await getAppStoreAppsTask;

            return Ok(new
            {
                GooglePlayApps = googlePlayApps.Select(gpApp => new
                {
                    Name = gpApp.Name,
                    Icon = gpApp.Icon,
                    Rating = gpApp.Rating,
                    Screenshots = gpApp.Screenshots.Select(s => s.Url)
                }),
                AppStoreApps = appStoreApps.Select(asApp => new
                {
                    Name = asApp.Name,
                    Icon = asApp.Icon,
                    Rating = asApp.Rating,
                    Screenshots = asApp.Screenshots.Select(s => s.Url)
                })
            });
        }

        private async Task<List<Application>> GetGooglePlayApplicationsAsync(string keyword)
        {
            using (var db = new GooglePlayAppsRepository())
            {
                var apps = db.SearchRequests
                    .Include(s => s.Applications)
                    .ThenInclude(a => a.Screenshots)
                    .FirstOrDefault(s => s.Keyword == keyword)?.Applications.ToList();

                if (apps == null)
                {
                    await Console.Out.WriteLineAsync("Get data from web resourse");
                    List<string> top3GooglePlayAppsNames = await _googlePlayClient.GetTop3SearchSuggestAsync(keyword);
                    var getGooglePlayAppsInfoTask = top3GooglePlayAppsNames.Select(async n =>
                    {
                        return await _googlePlayClient.GetApplicationInfo(n);
                    });
                    var applications = await Task.WhenAll(getGooglePlayAppsInfoTask);
                    db.SearchRequests.Add(new SearchRequest { Keyword = keyword, Applications = applications });
                    db.SaveChanges();
                    return applications.ToList();
                }
                await Console.Out.WriteLineAsync("Get data from database");
                return apps;
            }
        }

        private async Task<List<Application>> GetAppStoreApplicationsAsync(string keyword)
        {
            using (var db = new AppStoreAppsRepository())
            {
                var apps = db.SearchRequests
                    .Include(s => s.Applications)
                    .ThenInclude(a => a.Screenshots)
                    .FirstOrDefault(s => s.Keyword == keyword)?.Applications.ToList();

                if (apps == null)
                {
                    await Console.Out.WriteLineAsync("Get data from web resourse");
                    List<string> top3AppStoreAppsNames = await _appStoreClient.GetTop3SearchSuggestAsync(keyword);
                    var getAppStoreAppsInfoTask = top3AppStoreAppsNames.Select(async n =>
                    {
                        return await _appStoreClient.GetApplicationInfo(n);
                    });
                    var applications = await Task.WhenAll(getAppStoreAppsInfoTask);
                    db.SearchRequests.Add(new SearchRequest { Keyword = keyword, Applications = applications });
                    db.SaveChanges();
                    return applications.ToList();
                }
                await Console.Out.WriteLineAsync("Get data from database");
                return apps;
            }
        }
    }
}
