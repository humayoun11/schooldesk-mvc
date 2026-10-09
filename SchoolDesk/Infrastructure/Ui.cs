using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
namespace SchoolDesk.Infrastructure {
 public static class Ui {
  public static IEnumerable<SelectListItem> Options(object table,object selected=null){var t=(DataTable)table;return t.Rows.Cast<DataRow>().Select(r=>new SelectListItem{Value=r["Id"].ToString(),Text=r["Label"].ToString(),Selected=selected!=null&&r["Id"].ToString()==selected.ToString()});}
  public static string Cell(object v){if(v==null||v==DBNull.Value)return "—";if(v is DateTime)return ((DateTime)v).ToString("dd MMM yyyy");if(v is decimal)return ((decimal)v).ToString("N2");if(v is bool)return (bool)v?"Active":"Disabled";return v.ToString();}
 }
}
