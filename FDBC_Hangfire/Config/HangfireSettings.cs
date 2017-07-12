using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Dashboard;

namespace FDBC_Hangfire.Config
{
  public class HangfireSettings
  {
  }

  public class MyAuthorizationFilter : IDashboardAuthorizationFilter
  {
    public bool Authorize(DashboardContext context)
    {
      var httpContext = context.GetHttpContext();

      // Allow all authenticated users to see the Dashboard (potentially dangerous).
      return httpContext.User.Identity.IsAuthenticated;
    }
  }

  public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
  {
    private readonly string[] _roles;

    public HangfireAuthorizationFilter(params string[] roles)
    {
      _roles = roles;
    }

    public bool Authorize(DashboardContext context)
    {
      var httpContext = ((AspNetCoreDashboardContext)context).HttpContext;
      var result = _roles.Aggregate(false, (current, role) => current || httpContext.User.IsInRole(role));

      return result;
    }
  }

  // config.UseAuthorizationFilters(new DontUseThisAuthorizationFilter());
  public class NoAuthorizationFilter : IDashboardAuthorizationFilter
  {
    public bool Authorize(DashboardContext context)
    {
      var httpContext = ((AspNetCoreDashboardContext)context).HttpContext;
      return true;
    }
  }
}
