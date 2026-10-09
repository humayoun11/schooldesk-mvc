using System;
using System.Data;
using System.Globalization;
using System.Security.Claims;
using System.Web;
using System.Web.Security;
using System.Runtime.Caching;
namespace SchoolDesk.Infrastructure {
 public static class Auth {
  public static int Id {get{return int.Parse(((ClaimsPrincipal)HttpContext.Current.User).FindFirst(ClaimTypes.NameIdentifier).Value);}}
  public static ClaimsPrincipal Principal(DataRow r,string method) {var i=new ClaimsIdentity(method);i.AddClaim(new Claim(ClaimTypes.NameIdentifier,r["Id"].ToString()));i.AddClaim(new Claim(ClaimTypes.Name,r["FullName"].ToString()));i.AddClaim(new Claim(ClaimTypes.Role,r["Role"].ToString()));return new ClaimsPrincipal(i);}
  public static void Restore(HttpContext context) {
   var cookie=context.Request.Cookies[FormsAuthentication.FormsCookieName];if(cookie==null)return;context.User=new ClaimsPrincipal(new ClaimsIdentity());System.Threading.Thread.CurrentPrincipal=context.User;
   try {var ticket=FormsAuthentication.Decrypt(cookie.Value);if(ticket==null||ticket.Expired)return;var users=Db.Query("SELECT Id,FullName,Role,SecurityVersion FROM Users WHERE Id=@id AND IsActive=1",Db.P("@id",int.Parse(ticket.Name)));if(users.Rows.Count!=1)return;var r=users.Rows[0];if(ticket.UserData!=r["SecurityVersion"].ToString())return;context.User=Principal(r,"Cookies");System.Threading.Thread.CurrentPrincipal=context.User;}catch{FormsAuthentication.SignOut();}
  }
  public static void SignIn(DataRow r) {var ticket=new FormsAuthenticationTicket(2,r["Id"].ToString(),DateTime.Now,DateTime.Now.AddMinutes(30),false,r["SecurityVersion"].ToString(),"/");var cookie=new HttpCookie(FormsAuthentication.FormsCookieName,FormsAuthentication.Encrypt(ticket)){HttpOnly=true,Secure=HttpContext.Current.Request.IsSecureConnection,SameSite=SameSiteMode.Lax,Path="/"};HttpContext.Current.Response.Cookies.Add(cookie);}
  public static DataRow Check(string email,string password) {
   if(string.IsNullOrWhiteSpace(email)||string.IsNullOrEmpty(password)||password.Length>128)return null;
   var key="login:"+HttpContext.Current.Request.UserHostAddress;var cache=MemoryCache.Default;int attempts=0;lock(cache){attempts=(int?)cache.Get(key)??0;if(attempts>=25)return null;cache.Set(key,attempts+1,DateTimeOffset.UtcNow.AddMinutes(10));}
   var users=Db.Query("SELECT * FROM Users WHERE Email=@email AND IsActive=1",Db.P("@email",email.Trim().ToLowerInvariant()));
   if(users.Rows.Count!=1){Passwords.Hash(password);return null;}var r=users.Rows[0];
   if(r["LockedUntil"]!=DBNull.Value&&(DateTime)r["LockedUntil"]>DateTime.UtcNow)return null;
   if(!Passwords.Verify(password,r["PasswordHash"].ToString())) {Db.Execute("UPDATE Users SET FailedLogins=FailedLogins+1,LockedUntil=CASE WHEN FailedLogins+1>=5 THEN DATEADD(minute,15,SYSUTCDATETIME()) ELSE NULL END WHERE Id=@id",Db.P("@id",r["Id"]));return null;}
   Db.Execute("UPDATE Users SET FailedLogins=0,LockedUntil=NULL WHERE Id=@id",Db.P("@id",r["Id"]));return r;
  }
  public static bool CanClass(int id) {return HttpContext.Current.User.IsInRole("Admin")||Convert.ToInt32(Db.Scalar("SELECT COUNT(*) FROM Classes WHERE Id=@c AND TeacherId=@u",Db.P("@c",id),Db.P("@u",Id)))==1;}
  public static string StudentScope {get {var u=HttpContext.Current.User;if(u.IsInRole("Admin")||u.IsInRole("Accountant"))return "1=1";if(u.IsInRole("Teacher"))return "s.ClassId IN (SELECT Id FROM Classes WHERE TeacherId=@viewer)";if(u.IsInRole("Parent"))return "s.ParentUserId=@viewer";return "s.UserId=@viewer";}}
 }
}
