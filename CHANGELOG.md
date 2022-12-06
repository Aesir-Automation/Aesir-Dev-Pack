# Dev Pack Change Log

## v1.4.0

- Added IsEmpowering, EmpoweredLevel, and MaxEmpoweredLevel to Unit scope (works for player only)
- Added new rotation namespace
- Added rotation messaging system
  - `void InfoLog(string message);`
  - `void SuccessLog(string message);`
  - `void WarningLog(string message);`
  - `void ErrorLog(string message);`