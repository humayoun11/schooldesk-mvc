from pathlib import Path
import re, xml.etree.ElementTree as ET
root=Path(__file__).resolve().parents[1]
for p in root.rglob('*.config'): ET.parse(p)
ET.parse(root/'SchoolDesk/SchoolDesk.csproj')
for p in (root/'SchoolDesk/Controllers').glob('*.cs'):
 text=p.read_text()
 for m in re.finditer(r'\[([^\]]*HttpPost[^\]]*)\]\s*public ActionResult (\w+)',text):
  attrs,action=m.groups()
  assert 'ValidateAntiForgeryToken' in attrs or p.name=='ApiController.cs', (p,action,'Missing CSRF validation')
school=(root/'SchoolDesk/Controllers/SchoolController.cs').read_text()
for action in ['Student','ArchiveStudent','CreateClass','AssignTeacher','CreateUser','SetUserActive','SaveAttendance','Invoice','Pay','CreateExam','SaveMarks']:
 assert re.search(r'\[RoleAuthorize\(Roles=.*?HttpPost,ValidateAntiForgeryToken\]public ActionResult '+action+r'\(',school), action
assert school.count('Auth.CanClass')>=4
assert 'IsolationLevel.Serializable' in school
assert 'Payment exceeds balance' in school
assert 'Auth.StudentScope' in school
assert 'Update Students' not in school # SQL uses a consistently reviewed capitalized command.
assert 'new AuthorizeAttribute()' not in (root/'SchoolDesk/Global.asax.cs').read_text()
assert 'new SiteAuthorize()' in (root/'SchoolDesk/Global.asax.cs').read_text()
security=(root/'SchoolDesk/Infrastructure/Security.cs').read_text()
for required in ['ValidateIssuer=true','ValidateAudience=true','ValidateLifetime=true','ValidateIssuerSigningKey=true','ValidAlgorithms','SecurityVersion']:
 assert required in security,required
assert not (root/'SchoolDesk/App_Data/local.settings.config').exists()
assert not (root/'SchoolDesk/App_Data/demo-credentials.txt').exists()
print('Source checks passed: XML, POST protection, role restrictions, scoped queries, payment transaction, JWT validation and secret exclusions.')
