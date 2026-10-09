using System;
using System.Security.Cryptography;
namespace SchoolDesk.Infrastructure {
 public static class Passwords {
  public const int Iterations=210000;
  public static string Hash(string value) {var salt=new byte[16];using(var r=RandomNumberGenerator.Create())r.GetBytes(salt);using(var k=new Rfc2898DeriveBytes(value,salt,Iterations,HashAlgorithmName.SHA256))return "pbkdf2-sha256$"+Iterations+"$"+Convert.ToBase64String(salt)+"$"+Convert.ToBase64String(k.GetBytes(32));}
  public static bool Verify(string value,string encoded) {try{var p=encoded.Split('$');if(p.Length!=4||p[0]!="pbkdf2-sha256")return false;var n=int.Parse(p[1]);if(n<100000||n>1000000)return false;var expected=Convert.FromBase64String(p[3]);using(var k=new Rfc2898DeriveBytes(value,Convert.FromBase64String(p[2]),n,HashAlgorithmName.SHA256)){var actual=k.GetBytes(expected.Length);int delta=0;for(int i=0;i<actual.Length;i++)delta|=actual[i]^expected[i];return delta==0;}}catch{return false;}}
 }
}
