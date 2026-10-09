# SchoolDesk ko local par kaise chalana hai

Ye classic ASP.NET MVC 5 / .NET Framework 4.8 ka project hai. Laravel, XAMPP aur `php artisan` se nahi chalega.

1. ZIP extract karo. Misal: `C:\Projects\SchoolDesk`.
2. Visual Studio Installer mein `ASP.NET and web development` workload, `IIS Express` aur `SQL Server Express LocalDB` installed hon.
3. Supported .NET SDK (8 ya newer) installed ho. PowerShell mein `dotnet --info` se check karo.
4. Extracted SchoolDesk folder kholo jahan README.md aur scripts folder hai. Address bar mein `powershell` likh kar Enter karo.
5. Ye command chalao:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Setup.ps1 -Demo
```

Pehli baar Internet chahiye. Is se database, tables, fictional demo records, unique account passwords aur build tayyar hoga.

6. Setup successful ho to:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Run.ps1
```

7. Browser mein `http://localhost:5080` kholo. Terminal khula rehne do.
8. Login details `SchoolDesk\App_Data\demo-credentials.txt` mein hongi. Admin email `admin@schooldesk.example` hai; password file se lo.
9. Pehle Admin phir Teacher, Accountant, Parent aur Student accounts se check karo.
10. App chal raha ho to doosre PowerShell mein tests chala sakte ho:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Test-Local.ps1
```

Koi error aaye to error text bhejo. JWT secret aur account passwords share mat karna. Ye file GitHub par bhi upload mat karna. Dobara launch karne ke liye sirf Run.ps1 chalao.

Windows/IIS/SQL Server par final functional verification tumhare laptop par karni hai. Source package ke saath exact verification status docs/VALIDATION.md mein diya hai.
