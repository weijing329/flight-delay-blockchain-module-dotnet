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
using Serilog.Core;
using Serilog.Events;

namespace FDBC_Main
{
  public class Startup
  {
    public Startup(IHostingEnvironment env)
    {
      var levelSwitch = new LoggingLevelSwitch();
      levelSwitch.MinimumLevel = LogEventLevel.Debug;

      Log.Logger = new LoggerConfiguration()
       .MinimumLevel.ControlledBy(levelSwitch)
       .Enrich.FromLogContext()
       .WriteTo.LiterateConsole()
       .WriteTo.RollingFile("Logs/FDBC_Main-{Date}.txt")
       //.WriteTo.LiterateConsole(restrictedToMinimumLevel: LogEventLevel.Information)
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

      IServiceProvider sp = services.BuildServiceProvider();
      ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
      //loggerFactory.AddDebug();
      loggerFactory.AddSerilog();
      //loggerFactory.AddFile("Logs/FDBC_Main-{Date}.txt", isJson: false);

      ILogger<Web3GethService> web3_logger = loggerFactory.CreateLogger<Web3GethService>();

      //Web3GethService = new Web3GethService(Configuration);
      Web3GethService = new Web3GethService(Configuration, web3_logger);
      services.AddSingleton<IWeb3GethService>(Web3GethService);
      //services.AddSingleton<IWeb3GethService, Web3GethService>((ctx) =>
      //{
      //  ILoggerFactory factory = ctx.GetRequiredService<ILoggerFactory>();
      //  ILogger<Web3GethService> logger = factory.CreateLogger<Web3GethService>();
      //  return new Web3GethService(Configuration, logger);
      //});
      //services.AddSingleton<Web3GethService>();

      ILogger<EasyNetQService> rabbitmq_logger = loggerFactory.CreateLogger<EasyNetQService>();

      //services.AddRawRabbit();
      //services.AddSingleton(new RawRabbitService(Configuration));

      services.AddSingleton(new EasyNetQService(Configuration, Web3GethService, rabbitmq_logger));
      //services.AddSingleton<EasyNetQService>((ctx) =>
      //{
      //  IWeb3GethService web3geth_service = ctx.GetRequiredService<IWeb3GethService>();
      //  return new EasyNetQService(Configuration, web3geth_service);
      //});
      //services.AddSingleton<EasyNetQService>();
      //services.AddSingleton<EasyNetQService>((ctx) => {
      //  IWeb3GethService web3geth_svc = ctx.GetRequiredService<IWeb3GethService>();
      //  return new EasyNetQService(Configuration, web3geth_svc);
      //});

      // Add framework services.
      //services.AddMvc();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
    {
      //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      //loggerFactory.AddDebug();

      //loggerFactory.AddSerilog();
      //loggerFactory.AddFile("Logs/FDBC_Main-{Date}.txt", isJson: false);

      //app.UseMvc();

      // Ensure any buffered events are sent at shutdown
      appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);
    }
  }
}
