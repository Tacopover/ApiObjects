@echo off
setlocal

:: Setup directories
set "WorkingDirectory=%~dp0"
if "%WorkingDirectory:~-1%"=="\" set "WorkingDirectory=%WorkingDirectory:~0,-1%"
set "ProjectFolder=%~dp0.."
for %%I in ("%ProjectFolder%") do set "ProjectFolder=%%~fI"
set "RevitTestFramework=%ProjectFolder%\packages\RevitTestFramework.1.19.23\tools"

:: Setup file paths
set "PackageFile=%RevitTestFramework%\RevitTestFrameworkConsole.exe"
set "Assembly=%ProjectFolder%\UnitTest\bin\Release\UnitTest_2022.dll"
set "Revit2022=C:\Program Files\Autodesk\Revit 2022\Revit.exe"

:: Copy .rvt files to RevitTestFramework directory
echo Copying .rvt files to RevitTestFramework directory...
for /r "%WorkingDirectory%" %%f in (*.rvt) do (
    copy "%%f" "%RevitTestFramework%"
)

:: Run the package file with these arguments
echo Running the package file with these arguments...
"%PackageFile%" --dir "%WorkingDirectory%" -a "%Assembly%" -r TestResults.xml -revit:"%Revit2022%" --continuous --groupByModel --clean

if %ERRORLEVEL% equ 0 (
    echo Execution completed successfully.
) else (
    echo Error: Execution failed with error code %ERRORLEVEL%.
)

:: Delete .rvt files from RevitTestFramework directory after the run
echo Deleting .rvt files from RevitTestFramework directory...
for /r "%RevitTestFramework%" %%f in (*.rvt) do (
    del "%%f"
)

echo Press the Enter key to close this window...
pause >nul

endlocal