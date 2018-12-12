using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Ray2.Grain;
using Orleans;

namespace Ray2.Host
{
    class Program
    {
       public static int Main(string[] args)
        {
            return RunMainAsync().Result;
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                var host = await StartSilo();
                Console.WriteLine("Press Enter to terminate...");
                Console.ReadLine();

                await host.StopAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            // define the cluster configuration
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()
                .UseRay(build =>
                {
                    build.AddRabbitMQ("rabbitmq", opt =>
                    {
                        opt.HostName = "127.0.0.1";
                        opt.UserName = "admin";
                        opt.Password = "admin";
                    });
                    build.AddPostgreSQL("postgresql", opt =>
                     {
                         opt.ConnectionString = "Server=localhost;Port=5432;Database=ray2;User Id=postgres; Password=sapass;Pooling=true;MaxPoolSize=50;Timeout=10;";
                     });
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "HelloWorldApp";
                })
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(Account).Assembly).WithReferences())
                .ConfigureLogging(logging => logging.AddConsole());
           

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}
