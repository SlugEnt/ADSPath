Echo Creates Debug Packages and pushes to Local Nuget Repo

## Set these 2 variables only!
set project=ADSPath
set packages="..\packages\release"

## Create Packages Directory.
if not exist ..\%packages% (
  mkdir ..\Packages
  mkdir ..\Packages\Release
  )
del %packages%\*.nupkg


set program="..\src\%project%"
dotnet msbuild /p:Configuration=Debug %program%
del %packages%\*.nupkg
dotnet pack -o %packages% %program%



for %%n in (%packages%\*.nupkg) do  dotnet nuget push -s d:\a_dev\LocalNugetPackages "%%n"
