set PROJECTDIR=%1
set ARCHIVEDIR=%PROJECTDIR%\Build
set FUNCTIONNAME=%2
mkdir Build
del /s /f /q %ARCHIVEDIR%
%UNITYEDITORPATH% -projectPath %PROJECTDIR% -batchmode -quit -silent-crashes -vcsMode "Visible Meta Files" -executeMethod %2 -logFile -
exit /B %ERRORLEVEL%