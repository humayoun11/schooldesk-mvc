using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
namespace SchoolDesk.Infrastructure {
 public static class Db {
  public static SqlConnection Open() { var c=new SqlConnection(ConfigurationManager.ConnectionStrings["SchoolDesk"].ConnectionString);c.Open();return c; }
  public static SqlParameter P(string name,object value) {return new SqlParameter(name,value??DBNull.Value);}
  public static DataTable Query(string sql,params SqlParameter[] ps) {using(var c=Open())using(var cmd=new SqlCommand(sql,c)){cmd.Parameters.AddRange(ps);using(var a=new SqlDataAdapter(cmd)){var t=new DataTable();a.Fill(t);return t;}}}
  public static int Execute(string sql,params SqlParameter[] ps) {using(var c=Open())using(var cmd=new SqlCommand(sql,c)){cmd.Parameters.AddRange(ps);return cmd.ExecuteNonQuery();}}
  public static object Scalar(string sql,params SqlParameter[] ps) {using(var c=Open())using(var cmd=new SqlCommand(sql,c)){cmd.Parameters.AddRange(ps);return cmd.ExecuteScalar();}}
  public static void Audit(int actor,string action,string entity,int? id=null) {Execute("INSERT AuditLogs(ActorId,Action,Entity,EntityId) VALUES(@a,@b,@c,@d)",P("@a",actor),P("@b",action),P("@c",entity),P("@d",id));}
 }
}
