using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using SchoolDesk.Infrastructure;
using SchoolDesk.Models;
namespace SchoolDesk.Controllers {
 public class ApiController:Controller {
  [AllowAnonymous,HttpPost]public ActionResult Token(LoginForm f){Response.SuppressFormsAuthenticationRedirect=true;if(!ModelState.IsValid)return new HttpStatusCodeResult(400);var row=Auth.Check(f.Email,f.Password);if(row==null)return new HttpStatusCodeResult(401);return Json(new{access_token=Tokens.Issue(row),token_type="Bearer",expires_in=900});}
  [BearerAuthorize,HttpGet]public ActionResult Students(int page=1){page=Math.Max(1,Math.Min(page,10000));var t=Db.Query("SELECT s.Id,s.AdmissionNumber,s.FullName,c.Name Class,c.Section FROM Students s JOIN Classes c ON c.Id=s.ClassId WHERE s.IsActive=1 AND "+Auth.StudentScope+" ORDER BY s.Id OFFSET @skip ROWS FETCH NEXT 20 ROWS ONLY",Db.P("@viewer",Auth.Id),Db.P("@skip",(page-1)*20));return Json(new{page,data=t.Rows.Cast<DataRow>().Select(r=>new{id=r["Id"],admissionNumber=r["AdmissionNumber"],fullName=r["FullName"],className=r["Class"],section=r["Section"]}).ToArray()},JsonRequestBehavior.AllowGet);}
  [BearerAuthorize,HttpPost]public ActionResult Revoke(){Db.Execute("UPDATE Users SET SecurityVersion=SecurityVersion+1 WHERE Id=@id",Db.P("@id",Auth.Id));return Json(new{message="All existing sessions and API tokens revoked."});}
 }
}
