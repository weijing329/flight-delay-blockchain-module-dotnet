using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
//using RawRabbit;
//using RawRabbit.vNext;
//using RawRabbit.Configuration;
using FDBC_RabbitMQ.MqServices;
using FDBC_Shared.Configuration;
using FDBC_RabbitMQ.Config;

using FDBC_Nethereum.Helpers;

namespace FDBC_Main
{
  public class Startup
  {
    public Startup(IHostingEnvironment env)
    {
      //var builder = new ConfigurationBuilder()
      //    .SetBasePath(env.ContentRootPath)
      //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
      //    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
      //    .AddEnvironmentVariables();
      //Configuration = builder.Build();
      Configuration = ConfigBuilder.NewConfiguration(env);
    }

    public IConfigurationRoot Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.Configure<RabbitMQSettings>(options => Configuration.GetSection("RabbitMQSettings").Bind(options));

      //services.Configure<RawRabbitConfiguration>(options => Configuration.GetSection("RawRabbitConfiguration").Bind(options));

      // add the configuration object
      services.AddSingleton<IConfiguration>(Configuration);

      //services.AddRawRabbit();
      //services.AddSingleton(new RawRabbitService(Configuration));
      //services.AddSingleton(new EasyNetQService(Configuration));

      // Add framework services.
      services.AddMvc();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();

      app.UseMvc();

      Web3Helper.GetValue();
    }
  }
}
