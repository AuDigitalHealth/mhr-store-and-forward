using System;
using System.Configuration;
using System.Diagnostics;
using Serilog;
using Topshelf;

namespace DigitalHealth.StoreAndForward.Service
{
    public class Program
    {
        public static void Main()
        {
            // Handler of unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                if (args.ExceptionObject != null)
                {
                    Exception exception = (Exception)args.ExceptionObject;
                    Console.WriteLine(exception.Message);
                }
            };

            // Setup logging
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .ReadFrom.AppSettings()
                .CreateLogger();

            Log.Information("DB connection string {dbConnection}", ConfigurationManager.ConnectionStrings["StoreAndForwardDb"].ConnectionString);

            // Topshelf
            HostFactory.Run(x =>
            {
                x.Service<StoreAndForwardOwinService>(s =>
                {
                    s.ConstructUsing(name => new StoreAndForwardOwinService("http://*:5000/storeandforward"));
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsLocalSystem();

                x.SetDescription("Stores and forwards CDA packages to the MHR.");
                x.SetDisplayName("Store and Forward Service");
                x.SetServiceName("StoreAndForwardService");
            });
        }
    }
}