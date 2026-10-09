# Verification status

Verified on 9 October 2026 in a Linux build environment. This document distinguishes source/build verification from Windows runtime verification.

## Completed

- NuGet restore completed successfully.
- Release build of the .NET Framework 4.8 MVC application passed with 0 warnings and 0 errors, using .NET SDK 10.0.401 and the .NET Framework reference-assembly package.
- NuGet vulnerability audit, including transitive packages, reported no known vulnerable packages in the configured package sources at the time of the check. This is not a security audit or a guarantee about future advisories.
- Six executable password checks passed: valid password, invalid password, random salts, malformed hashes, excessive iteration counts, and tampered hashes.
- All four PowerShell scripts parsed successfully using the PowerShell language parser (System.Management.Automation 7.5.4).
- Source checks passed for configuration XML, anti-forgery protection on browser write actions, role restrictions, class scoping, payment transactions, JWT validation flags and absence of generated private credential/configuration files.
- MVC and Razor configuration assembly versions match the restored assemblies.

## Required on Windows before publishing

The Linux environment cannot execute System.Web under IIS Express or SQL Server LocalDB. Setup, SQL migrations/seeding, Razor view rendering, browser workflows and the provided HTTP smoke tests have not been executed against those services here.

1. Run `scripts/Setup.ps1 -Demo` and `scripts/Run.ps1` as described in the README.
2. Run `scripts/Test-Local.ps1` while the app is running with the original demo data. This checks role scopes, selected denied actions, CSRF rejection and bearer-token rejection/revocation.
3. Manually verify student admission/edit/archive, class assignment, attendance save, fee invoice/partial payment/overpayment rejection, exam/result entry, printable reports, password change and account disable. Include invalid inputs and cross-role access attempts.
4. Check the responsive layout in your browser and record screenshots only after successful execution.

Do not describe this package as production-tested or free of all vulnerabilities. Share any setup/runtime error without passwords or signing keys so it can be corrected before adding the live link to a CV.

## Optional developer checks

`python scripts/Check-Source.py` requires Python 3. The separate password-check harness targets .NET 10 and can be run using `dotnet run --project tests/PasswordChecks/PasswordChecks.csproj`. That optional harness SDK requirement does not change the main application's .NET Framework 4.8 target or its documented build requirements.
