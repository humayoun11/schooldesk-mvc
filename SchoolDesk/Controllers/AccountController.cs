using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using SchoolDesk.Infrastructure;
using SchoolDesk.Models;
namespace SchoolDesk.Controllers {
 public class AccountController:Controller {
  [AllowAnonymous,HttpGet]public ActionResult Login(string returnUrl){return View(new LoginForm{ReturnUrl=returnUrl});}
  [AllowAnonymous,HttpPost,ValidateAntiForgeryToken]public ActionResult Login(LoginForm f){if(!ModelState.IsValid)return View(f);var u=Auth.Check(f.Email,f.Password);if(u==null){ModelState.AddModelError("","Login unsuccessful. Check your details or wait if the account is locked.");return View(f);}Auth.SignIn(u);Db.Audit(Convert.ToInt32(u["Id"]),"Login","User",Convert.ToInt32(u["Id"]));return Url.IsLocalUrl(f.ReturnUrl)?(ActionResult)Redirect(f.ReturnUrl):RedirectToAction("Index","Home");}
  [HttpPost,ValidateAntiForgeryToken]public ActionResult Logout(){Db.Execute("UPDATE Users SET SecurityVersion=SecurityVersion+1 WHERE Id=@id",Db.P("@id",Auth.Id));FormsAuthentication.SignOut();return RedirectToAction("Login");}
  [HttpGet]public ActionResult Password(){return View(new PasswordForm());}
  [HttpPost,ValidateAntiForgeryToken]public ActionResult Password(PasswordForm f){if(!ModelState.IsValid)return View(f);var row=Db.Query("SELECT PasswordHash FROM Users WHERE Id=@id",Db.P("@id",Auth.Id)).Rows[0];if(!Passwords.Verify(f.CurrentPassword,row["PasswordHash"].ToString())){ModelState.AddModelError("","Current password is incorrect.");return View(f);}Db.Execute("UPDATE Users SET PasswordHash=@p,SecurityVersion=SecurityVersion+1 WHERE Id=@id",Db.P("@p",Passwords.Hash(f.NewPassword)),Db.P("@id",Auth.Id));Db.Audit(Auth.Id,"Change password","User",Auth.Id);FormsAuthentication.SignOut();return RedirectToAction("Login");}
 }
}
