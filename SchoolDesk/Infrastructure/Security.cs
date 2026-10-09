using System;
using System.Configuration;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web.Mvc;
using Microsoft.IdentityModel.Tokens;
namespace SchoolDesk.Infrastructure {
 public class RoleAuthorize:AuthorizeAttribute {
  protected override void HandleUnauthorizedRequest(AuthorizationContext c){if(c.HttpContext.User.Identity.IsAuthenticated){c.Result=new HttpStatusCodeResult(403);return;}base.HandleUnauthorizedRequest(c);}
 }
 public class SiteAuthorize:RoleAuthorize {
  public override void OnAuthorization(AuthorizationContext c){if(c.ActionDescriptor.ControllerDescriptor.ControllerName=="Api")return;base.OnAuthorization(c);}
 }
 public class SecurityHeaders:ActionFilterAttribute,IExceptionFilter {
  public void OnException(ExceptionContext c){if(c.Exception is System.Web.Mvc.HttpAntiForgeryException){c.ExceptionHandled=true;c.Result=new HttpStatusCodeResult(400,"Invalid form token.");}else if(c.Exception is System.Data.SqlClient.SqlException && ((System.Data.SqlClient.SqlException)c.Exception).Number==50001){c.ExceptionHandled=true;c.Result=new HttpStatusCodeResult(400,"Invalid record selection.");}}

  public override void OnActionExecuting(ActionExecutingContext c){if(!c.HttpContext.Request.IsLocal&&!c.HttpContext.Request.IsSecureConnection)c.Result=new HttpStatusCodeResult(400,"HTTPS is required.");}
  public override void OnResultExecuting(ResultExecutingContext c) {c.HttpContext.Response.Headers["Content-Security-Policy"]="default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; font-src 'self'; object-src 'none'; base-uri 'self'; frame-ancestors 'none'; form-action 'self'";c.HttpContext.Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);c.HttpContext.Response.Cache.SetNoStore();}
 }
 public static class Tokens {
  static string Setting(string k){return ConfigurationManager.AppSettings[k];}
  static SymmetricSecurityKey Key(){var s=Setting("JwtSigningKey");if(string.IsNullOrEmpty(s)||Encoding.UTF8.GetByteCount(s)<32)throw new InvalidOperationException("Run Setup.ps1 to create the local JWT key.");return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s));}
  public static string Issue(DataRow r){var claims=new[]{new Claim("sub",r["Id"].ToString()),new Claim("ver",r["SecurityVersion"].ToString()),new Claim("jti",Guid.NewGuid().ToString("N"))};return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(Setting("JwtIssuer"),Setting("JwtAudience"),claims,DateTime.UtcNow,DateTime.UtcNow.AddMinutes(15),new SigningCredentials(Key(),SecurityAlgorithms.HmacSha256)));}
  public static ClaimsPrincipal Validate(string text){var handler=new JwtSecurityTokenHandler{MapInboundClaims=false};SecurityToken token;var principal=handler.ValidateToken(text,new TokenValidationParameters{ValidateIssuer=true,ValidIssuer=Setting("JwtIssuer"),ValidateAudience=true,ValidAudience=Setting("JwtAudience"),ValidateIssuerSigningKey=true,IssuerSigningKey=Key(),ValidateLifetime=true,RequireExpirationTime=true,RequireSignedTokens=true,ValidAlgorithms=new[]{SecurityAlgorithms.HmacSha256},ClockSkew=TimeSpan.FromSeconds(30)},out token);var rows=Db.Query("SELECT Id,FullName,Role,SecurityVersion FROM Users WHERE Id=@id AND IsActive=1",Db.P("@id",int.Parse(principal.FindFirst("sub").Value)));if(rows.Rows.Count!=1||rows.Rows[0]["SecurityVersion"].ToString()!=principal.FindFirst("ver").Value)throw new SecurityTokenValidationException("Revoked token");return Auth.Principal(rows.Rows[0],"Bearer");}
 }
 public class BearerAuthorize:AuthorizeAttribute {
  public override void OnAuthorization(AuthorizationContext c){try{var h=c.HttpContext.Request.Headers["Authorization"];if(h==null||!h.StartsWith("Bearer ",StringComparison.OrdinalIgnoreCase))throw new Exception();c.HttpContext.User=Tokens.Validate(h.Substring(7));base.OnAuthorization(c);}catch{c.HttpContext.Response.SuppressFormsAuthenticationRedirect=true;c.Result=new HttpStatusCodeResult(401);}}
  protected override void HandleUnauthorizedRequest(AuthorizationContext c){c.HttpContext.Response.SuppressFormsAuthenticationRedirect=true;c.Result=new HttpStatusCodeResult(c.HttpContext.User.Identity.IsAuthenticated?403:401);}
 }
}
