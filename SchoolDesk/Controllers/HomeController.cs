using System;
using System.Web.Mvc;
using SchoolDesk.Infrastructure;
namespace SchoolDesk.Controllers {
 public class HomeController:Controller {
  public ActionResult Index(){ViewBag.Students=Db.Scalar("SELECT COUNT(*) FROM Students s WHERE s.IsActive=1 AND "+Auth.StudentScope,Db.P("@viewer",Auth.Id));ViewBag.Classes=Db.Scalar("SELECT COUNT(DISTINCT s.ClassId) FROM Students s WHERE s.IsActive=1 AND "+Auth.StudentScope,Db.P("@viewer",Auth.Id));ViewBag.Balance=Db.Scalar("SELECT COALESCE(SUM(i.Amount-COALESCE(p.Paid,0)),0) FROM Invoices i JOIN Students s ON s.Id=i.StudentId OUTER APPLY(SELECT SUM(Amount) Paid FROM Payments WHERE InvoiceId=i.Id)p WHERE "+Auth.StudentScope,Db.P("@viewer",Auth.Id));ViewBag.Attendance=Db.Scalar("SELECT COUNT(*) FROM Attendance a JOIN Students s ON s.Id=a.StudentId WHERE a.Day=CONVERT(date,GETDATE()) AND a.Status='Present' AND "+Auth.StudentScope,Db.P("@viewer",Auth.Id));ViewBag.Recent=Db.Query("SELECT TOP 5 s.FullName,c.Name+' / '+c.Section Class,s.AdmissionNumber FROM Students s JOIN Classes c ON c.Id=s.ClassId WHERE s.IsActive=1 AND "+Auth.StudentScope+" ORDER BY s.Id DESC",Db.P("@viewer",Auth.Id));return View();}
  [AllowAnonymous]public ActionResult Error(){Response.StatusCode=500;return View();}
 }
}
