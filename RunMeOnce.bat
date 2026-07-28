@ECHO OFF
echo .
set mypath=%cd%
echo current path is %mypath%
cd %mypath%
p4 set P4IGNORE=.p4ignore
echo .
echo .
echo Perforce ignores : 
p4 ignores
echo .
pause