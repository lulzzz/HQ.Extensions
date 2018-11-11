REM Recursively remove /obj and /bin folders
@ECHO OFF
rmdir /s /q bin\lib\
pushd src
for /f "tokens=*" %%i in ('DIR /B /AD /S obj') do rmdir /s /q %%i
for /f "tokens=*" %%i in ('DIR /B /AD /S bin') do rmdir /s /q %%i
popd