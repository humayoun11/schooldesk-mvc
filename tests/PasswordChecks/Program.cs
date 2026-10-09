using System;
using SchoolDesk.Infrastructure;
class Program {
 static int Main(){
  var password="Test password with spaces & symbols 42!";
  var hash=Passwords.Hash(password);var second=Passwords.Hash(password);
  Check(Passwords.Verify(password,hash),"Correct password accepted");
  Check(!Passwords.Verify("incorrect",hash),"Incorrect password rejected");
  Check(hash!=second,"Unique random salts");
  Check(!Passwords.Verify(password,"invalid"),"Malformed hash rejected");
  Check(!Passwords.Verify(password,hash.Replace("210000","99999999")),"Unsafe iteration count rejected");
  var parts=hash.Split('$');var raw=Convert.FromBase64String(parts[3]);raw[0]^=1;parts[3]=Convert.ToBase64String(raw);
  Check(!Passwords.Verify(password,string.Join("$",parts)),"Tampered hash rejected");
  Console.WriteLine("6 password checks passed.");return 0;
 }
 static void Check(bool ok,string title){if(!ok)throw new Exception(title);Console.WriteLine("PASS: "+title);}
}
