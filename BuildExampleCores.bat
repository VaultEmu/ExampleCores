@echo off

echo.
echo ####### BUILDING EXAMPLE CORES #######
echo.

rd /q /s "./PublishedCores" 2>nul

dotnet clean
dotnet restore --force 
dotnet publish --configuration Debug --force --output "./PublishedCores/ExampleCore"

echo.
echo.
echo ####### EXAMPLE CORES BUILT #######
echo Copy the folders inside 'PublishedCores' to the cores folder next to the VaultGUI exe to use
echo.

pause