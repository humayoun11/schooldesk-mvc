using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
namespace SchoolDesk.Models {
 public class LoginForm {[Required,EmailAddress,StringLength(254)]public string Email{get;set;}[Required,StringLength(128)]public string Password{get;set;}public string ReturnUrl{get;set;}}
 public class PasswordForm {[Required,StringLength(128)]public string CurrentPassword{get;set;}[Required,StringLength(128,MinimumLength=12)]public string NewPassword{get;set;}[Compare("NewPassword")]public string ConfirmPassword{get;set;}}
 public class StudentForm {public int Id{get;set;}[Required,StringLength(30)]public string AdmissionNumber{get;set;}[Required,StringLength(100)]public string FullName{get;set;}[Required,DataType(DataType.Date)]public DateTime BirthDate{get;set;}[Range(1,int.MaxValue)]public int ClassId{get;set;}[StringLength(150)]public string GuardianName{get;set;}[StringLength(30)]public string GuardianPhone{get;set;}public int? ParentUserId{get;set;}public int? UserId{get;set;}}
 public class ClassForm {[Required,StringLength(80)]public string Name{get;set;}[Required,StringLength(20)]public string Section{get;set;}[Range(1,int.MaxValue)]public int TeacherId{get;set;}}
 public class UserForm {[Required,StringLength(100)]public string FullName{get;set;}[Required,EmailAddress,StringLength(254)]public string Email{get;set;}[Required,StringLength(128,MinimumLength=12)]public string Password{get;set;}[Required]public string Role{get;set;}}
 public class InvoiceForm {[Range(1,int.MaxValue)]public int StudentId{get;set;}[Required,StringLength(150)]public string Description{get;set;}[Range(typeof(decimal),"0.01","10000000")]public decimal Amount{get;set;}[DataType(DataType.Date)]public DateTime DueDate{get;set;}}
 public class PaymentForm {[Range(1,int.MaxValue)]public int InvoiceId{get;set;}[Range(typeof(decimal),"0.01","10000000")]public decimal Amount{get;set;}[Required,StringLength(80)]public string Reference{get;set;}}
 public class ExamForm {[Required,StringLength(100)]public string Title{get;set;}[Required,StringLength(80)]public string Subject{get;set;}[Range(1,int.MaxValue)]public int ClassId{get;set;}[Range(1,1000)]public int MaxMarks{get;set;}[DataType(DataType.Date)]public DateTime ExamDate{get;set;}}
 public class TablePage {public string Title{get;set;}public string Description{get;set;}public DataTable Rows{get;set;}public int Page{get;set;}public bool HasNext{get;set;}public string Search{get;set;}}
}
