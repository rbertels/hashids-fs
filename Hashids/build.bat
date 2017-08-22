dotnet build --configuration RELEASE
dotnet test Hashids.Tests\Hashids.Tests.fsproj
dotnet pack Hashids\Hashids.fsproj --configuration RELEASE --no-build --output ..\build