using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SchoolDesk.Infrastructure;
namespace SchoolDesk {
 public class MvcApplication : HttpApplication {
  protected void Application_Start() {
   AreaRegistration.RegisterAllAreas();
   GlobalFilters.Filters.Add(new SiteAuthorize());
   GlobalFilters.Filters.Add(new SecurityHeaders());
   RouteTable.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
   RouteTable.Routes.MapRoute("Token","api/token",new {controller="Api",action="Token"});
   RouteTable.Routes.MapRoute("ApiStudents","api/students",new {controller="Api",action="Students"});
   RouteTable.Routes.MapRoute("Default","{controller}/{action}/{id}",new {controller="Home",action="Index",id=UrlParameter.Optional});
   System.Web.Helpers.AntiForgeryConfig.UniqueClaimTypeIdentifier = System.Security.Claims.ClaimTypes.NameIdentifier;
   MvcHandler.DisableMvcResponseHeader = true;
  }
  protected void Application_PostAuthenticateRequest() { Auth.Restore(Context); }
  protected void Application_Error() {
   var error=Server.GetLastError();
   System.Diagnostics.Trace.TraceError(error == null ? "Unhandled error" : error.ToString());
  }
 }
}
