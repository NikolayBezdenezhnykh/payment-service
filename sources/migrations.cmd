@ECHO OFF

dotnet ef migrations add %* --startup-project src\Hosts\Api --project src\Modules\Infrastructure --verbose

pause
