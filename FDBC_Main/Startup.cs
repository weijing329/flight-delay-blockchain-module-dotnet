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
using FDBC_Nethereum.Services;

using FDBC_Nethereum.Config;
using Serilog;

namespace FDBC_Main
{
  public class Startup
  {
    public Startup(IHostingEnvironment env)
    {
      Log.Logger = new LoggerConfiguration()
       .Enrich.FromLogContext()
       .WriteTo.LiterateConsole()
       .CreateLogger();

      //var builder = new ConfigurationBuilder()
      //    .SetBasePath(env.ContentRootPath)
      //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
      //    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
      //    .AddEnvironmentVariables();
      //Configuration = builder.Build();
      Configuration = ConfigBuilder.NewConfiguration(env);
    }

    public IConfigurationRoot Configuration { get; }

    public IWeb3GethService Web3GethService { get; set; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.Configure<RabbitMQSettings>(options => Configuration.GetSection("RabbitMQSettings").Bind(options));
      services.Configure<BlockchainSettings>(options => Configuration.GetSection("BlockchainSettings").Bind(options));

      //services.Configure<RawRabbitConfiguration>(options => Configuration.GetSection("RawRabbitConfiguration").Bind(options));

      // add the configuration object
      services.AddSingleton<IConfiguration>(Configuration);

      //Web3GethService = new Web3GethService(Configuration, Log.Logger);
      Web3GethService = new Web3GethService(Configuration);
      services.AddSingleton<IWeb3GethService>(Web3GethService);
      //services.AddSingleton<Web3GethService>();

      //services.AddRawRabbit();
      //services.AddSingleton(new RawRabbitService(Configuration));
      services.AddSingleton(new EasyNetQService(Configuration, Web3GethService));
      //services.AddSingleton<EasyNetQService>();

      // Add framework services.
      services.AddMvc();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
    {
      //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      //loggerFactory.AddDebug();

      loggerFactory.AddSerilog();
      loggerFactory.AddFile("Logs/FDBC_Main-{Date}.txt", isJson: false);

      app.UseMvc();

      // Ensure any buffered events are sent at shutdown
      appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);
    }
  }
}
