Echo Creates Release Packages

set project=ADSPath
set packages="..\packages\release"

REM # Preliminary Stuff
REM # Setup Nuget
nuget setapikey %NuGetApiKey% -source %NuGetSrc%

REM ## Create Packages Directory.
if not exist ..\%packages% (
  mkdir ..\Packages
  mkdir ..\Packages\Release
  )

set program="..\src\%project%"
dotnet msbuild /p:Configuration=Release %program%
del %packages%\*.nupkg
dotnet pack -o %packages% %program%

for %%i in (%packages%\*.nupkg) do (
  echo %%i
  nuget push %%i -ApiKey %NuGetApiKey% -src %NuGetSrc%
  )