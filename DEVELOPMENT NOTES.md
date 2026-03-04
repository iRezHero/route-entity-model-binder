# Development notes

## Re-pack the nuget package

1. Execute a build of the project with `dotnet build -c Release` or `dotnet build` for debug version
2. Pack the executable launching `dotnet pack -c Release` or `dotnet pack`
3. Copy the nuget package under `./bin/Release/irezhero.EntityModelBinder.1.0.0.nupkg` or `./bin/Debug/irezhero.EntityModelBinder.1.0.0.nupkg` in your local nuget folder
4. Run `dotnet nuget locals all --clear` (nuget dude is cache aggressive lol)
5. Enjoy testing
