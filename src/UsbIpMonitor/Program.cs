using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using NLog;
using UsbIpMonitor.Autofac;
using UsbIpMonitor.Core;

namespace UsbIpMonitor
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var logger = LogManager.Setup().GetCurrentClassLogger();

            try
            {
                logger.Info($"USB IP Monitor v{GetVersion()}.");

                await using var root = BuildRoot();
                var executor = root.Resolve<Executor>();

                using var source = OnCancelRequest(logger);

                try
                {
                    return await executor.Run(args, source.Token);
                }
                finally
                {
                    // This needs to be canceled before leaving scope.
                    source.Cancel();
                }
            }
            catch (TaskCanceledException)
            {
                // Ignored.
                return 2;
            }
            catch (Exception e)
            {
                logger.Error(e, "Fatal error during startup.");
                return 1;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        private static IContainer BuildRoot()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<AutofacModule>();
            builder.ApplyConventions(typeof(Program).Assembly);
            return builder.Build();
        }

        private static string GetVersion()
        {
            return Assembly.GetEntryAssembly()!.GetName().Version!.ToString();
        }

        private static CancellationTokenSource OnCancelRequest(ILogger logger)
        {
            var source = new CancellationTokenSource();

            Console.CancelKeyPress += (_, _) =>
            {
                if (!source.IsCancellationRequested)
                {
                    logger.Info("Caught break, attempting to exit gracefully...");
                    source.Cancel();
                }
            };

            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                if (!source.IsCancellationRequested)
                {
                    logger.Info("Accepted a signal to terminate. Attempting to exit gracefully within 2 seconds.");
                    source.Cancel();
                    Thread.Sleep(2000);
                }
            };

            return source;
        }
    }
}
