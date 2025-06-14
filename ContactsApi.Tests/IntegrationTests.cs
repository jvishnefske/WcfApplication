dotnet clean WcfApplication.sln
rm -rf **/bin **/obj
dotnet build WcfApplication.sln
dotnet test
