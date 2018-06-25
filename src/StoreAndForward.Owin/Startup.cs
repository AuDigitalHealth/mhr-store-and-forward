using System;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Tracing;
using Autofac;
using Autofac.Integration.WebApi;
using DigitalHealth.StoreAndForward.Core.Data;
using DigitalHealth.StoreAndForward.Core.Mhr;
using DigitalHealth.StoreAndForward.Core.Notification;
using DigitalHealth.StoreAndForward.Core.Package;
using DigitalHealth.StoreAndForward.Core.Queue;
using DigitalHealth.StoreAndForward.Infrastructure.Data;
using DigitalHealth.StoreAndForward.Infrastructure.Mhr;
using DigitalHealth.StoreAndForward.Infrastructure.Package;
using DigitalHealth.StoreAndForward.Owin.Config;
using DigitalHealth.StoreAndForward.Owin.Errors;
using DigitalHealth.StoreAndForward.Owin.Jobs;
using DigitalHealth.StoreAndForward.Owin.Logging;
using DigitalHealth.StoreAndForward.Owin.Services;
using Hangfire;
using Microsoft.Owin.Cors;
using Owin;
using Serilog;
using Swashbuckle.Application;
using TraceLevel = System.Web.Http.Tracing.TraceLevel;

namespace DigitalHealth.StoreAndForward.Owin
{
    /// <summary>
    /// OWIN startup.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configuration.
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {
            // Load the configuration
            IConfigurationService configurationService = new JsonConfigurationService($"{Environment.CurrentDirectory}/config.json");

            string hangfireConnectionString = ConfigurationManager.ConnectionStrings["Hangfire"].ConnectionString;
            Log.Information("Hangfire connection string {hangfireConnection}", hangfireConnectionString);


            // Autofac API container
            var apiContainerBuilder = new ContainerBuilder();
            apiContainerBuilder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            apiContainerBuilder.RegisterType<SendDocumentsJob>();
            apiContainerBuilder.RegisterType<StoreAndForwardDbContext>().AsSelf()
                .OnActivating(d => Log.Information("OnActivating - StoreAndForwardDbContext"))
                .OnRelease(d => // Uses Autofac automatic disposal for the DB context
                {
                    Log.Information("OnRelease - StoreAndForwardDbContext");
                    d.Dispose();
                });

            apiContainerBuilder.RegisterType<QueueManager>().As<IQueueManager>()
                .WithParameter("retryLimit", configurationService.RetryLimit);
            apiContainerBuilder.RegisterType<DataStore>().As<IDataStore>();
            apiContainerBuilder.RegisterType<SignalrNotificationService>().As<INotificationService>();
            apiContainerBuilder.RegisterType<CdaPackageService>().As<ICdaPackageService>();
            apiContainerBuilder.RegisterType<MhrDocumentUploadClient>().As<IMhrDocumentUploadClient>();
            apiContainerBuilder.RegisterType<MhrDocumentUploadClient>().As<IMhrDocumentUploadClient>()
                .WithParameter("endpoint", configurationService.UploadDocumentEndpoint)
                .WithParameter("certificate", configurationService.Certificate)
                .WithParameter("facilityType", configurationService.HealthcareFacility)
                .WithParameter("practiceSetting", configurationService.PracticeSetting)
                .WithParameter("clientSystem", configurationService.ClientSystemType)
                .WithParameter("productInfo", configurationService.ProductInfo);

            // HTTP configuration
            var httpConfiguration = new HttpConfiguration();
            httpConfiguration.MapHttpAttributeRoutes();
            httpConfiguration.Formatters.Remove(httpConfiguration.Formatters.XmlFormatter);
            httpConfiguration.Formatters.Add(httpConfiguration.Formatters.JsonFormatter);
            httpConfiguration.Services.Replace(typeof(IExceptionHandler), new StoreAndForwardExceptionHandler());

            //config.EnableSystemDiagnosticsTracing();
            SystemDiagnosticsTraceWriter diagnosticsTraceWriter = new SystemDiagnosticsTraceWriter
            {
                MinimumLevel = TraceLevel.Off,
                IsVerbose = true
            };
            httpConfiguration.Services.Replace(typeof(ITraceWriter), (object)diagnosticsTraceWriter);
            httpConfiguration.MessageHandlers.Add(new SerilogLoggingMessageHandler());

            // OpenAPI
            httpConfiguration.EnableSwagger(c =>
                {
                    c.SingleApiVersion("v1", "Store and Forward API");
                    c.IncludeXmlComments(GetXmlCommentsPath());
                    c.RootUrl(r => $"{r.RequestUri.Scheme}://{r.RequestUri.Authority}/storeandforward");
                })
                .EnableSwaggerUi(c =>
                {
                    c.DisableValidator(); // Prevent Swashbuckle using external validation
                });

            var apiContainer = apiContainerBuilder.Build();
            httpConfiguration.DependencyResolver = new AutofacWebApiDependencyResolver(apiContainer);

            // Hangfire setup
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(hangfireConnectionString)
                .UseAutofacActivator(apiContainer);

            // Setup CORS
            var corsPolicy = new CorsPolicy
            {
                AllowAnyMethod = true,
                AllowAnyHeader = true,
                AllowAnyOrigin = true,
                SupportsCredentials = false             
            };
            
            var corsOptions = new CorsOptions
            {
                PolicyProvider = new CorsPolicyProvider
                {
                    PolicyResolver = context => Task.FromResult(corsPolicy)
                }
            };
            app.UseCors(corsOptions);
             
            // Add SignalR
            app.MapSignalR();

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                // Allow all IPs to connect
                Authorization = new[] { new DebugHangfireDashboardAuthorizationFilter() }
            });
            app.UseHangfireServer();

            // Web API
            app.UseAutofacMiddleware(apiContainer);
            app.UseAutofacWebApi(httpConfiguration);
            app.UseWebApi(httpConfiguration);

            // Create a recurring job
            RecurringJob.AddOrUpdate<SendDocumentsJob>(c => c.Execute(), Cron.MinuteInterval(configurationService.SendIntervalInMinutes));            
        }

        /// <summary>
        /// Gets the XML comments path for Swashbuckle.
        /// </summary>
        /// <returns>Path.</returns>
        private static string GetXmlCommentsPath()
        {
            return $"{Environment.CurrentDirectory}/DigitalHealth.StoreAndForward.Owin.xml";
        }
    }
}
