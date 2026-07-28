set PROJECTDIR=%1
set ZIPNAME=%2
set ARCHIVEDIR=%PROJECTDIR%\Build\
set ZIPDIR=%PROJECTDIR%\BuildMachineArchiveZip\
mkdir BuildMachineArchiveZip
del /s /f /q %ZIPDIR%
"C:\Program Files\7-Zip\7z.exe" a -tzip %ZIPDIR%\%ZIPNAME%.zip %ARCHIVEDIR%
exit /B %ERRORLEVEL%