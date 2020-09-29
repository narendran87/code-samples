using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SharePoint.Client;
using SolrNet;
using Sp.DataConnector.Logic;
using Sp.DataConnector.Logic.Interfaces;
using Sp.DataConnector.Logic.Models;
using System.Collections.Generic;

namespace SpDataConnectorV02
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
            var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

            var appSettings = new AppSettings();
            var solrConfig = new SolrConfig();
            var spCredetials = new SpCredentials();
            var timerConfig = new TimerConfig();
            config.GetSection("AppSettings").Bind(appSettings);
            config.GetSection("SolrConfig").Bind(solrConfig);
            config.GetSection("SpCredentials").Bind(spCredetials);
            config.GetSection("TimerConfig").Bind(timerConfig);

            services.AddSingleton(appSettings);
            services.AddSingleton(solrConfig);
            services.AddSingleton(spCredetials);
            services.AddSingleton(timerConfig);
            var localCacheManager = new LocalCacheManager();
            services.AddSingleton<ILocalCacheManager, LocalCacheManager>(
                x => localCacheManager);
            services.AddSingleton(
                x =>
                {
                    var ctx = new ClientContext(appSettings.Url);
                    ctx.Credentials = new SharePointOnlineCredentials(spCredetials.Username, spCredetials.Password);
                    return ctx;
                });
            services.AddTransient<ISpDataIntializer, SpDataIntializer>();
            services.AddTransient<ISpDataHelper, SpDataHelper>();
            services.AddTransient<IIndexHelper, IndexHelper>();
            services.AddTransient<ITikaExtractHelper, TikaExtractHelper>();
            services.AddTransient<ISharepointEventMonitor, SharepointEventMonitor>();
            services.AddTransient<IQueueProcessor, QueueProcessor>();

            var solrIndexHelper = services.BuildServiceProvider().GetRequiredService<IIndexHelper>();
            var intializeData = services.BuildServiceProvider().GetRequiredService<ISpDataIntializer>();
            var queueProcessor = services.BuildServiceProvider().GetRequiredService<IQueueProcessor>();
            var sharepointEventMonitor = services.BuildServiceProvider().GetRequiredService<ISharepointEventMonitor>();

            //solrIndexHelper.DeleteAll();

            intializeData.InitializeDataSync().Wait();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
