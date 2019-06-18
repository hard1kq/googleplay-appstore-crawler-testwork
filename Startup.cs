using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StoreParsers.Domain;
using StoreParsers.Infrastructure;

namespace StoreParsers
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddHttpClient<IGooglePlayClient, GooglePlayClient>();
            services.AddHttpClient<IAppStoreClient, AppStoreClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            using (var appStoreContext = new AppStoreAppsRepository())
            {
                appStoreContext.Database.Migrate();
            }
            using (var googlePlayContext = new GooglePlayAppsRepository())
            {
                googlePlayContext.Database.Migrate();
            }
            app.UseMvc();
        }
    }
}
