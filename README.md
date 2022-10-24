# Forge.Security.Jwt.Service.Storage.SqlServer
Forge.Security.Jwt.Service.Storage.SqlServer is a library extension that provides SqlServer storage on service side for the generated tokens.


## Installing

To install the package add the following line to you csproj file replacing x.x.x with the latest version number:

```
<PackageReference Include="Forge.Security.Jwt.Service.Storage.SqlServer" Version="x.x.x" />
```

You can also install via the .NET CLI with the following command:

```
dotnet add Forge.Security.Jwt.Service.Storage.SqlServer
```

If you're using Visual Studio you can also install via the built in NuGet package manager.

## Setup

You will need to register the storage provider services with the service collection in your _Startup.cs_ file in your server.

```c#
public void ConfigureServices(IServiceCollection services)
{
	// ... preinitialization steps
    services.AddForgeJwtServerAuthenticationCore();

	// HERE IT IS, always add this code after the "Forge.Security.Jwt.Service" library initialization
	services.AddForgeJwtServiceSqlServerStorage(config => {
		config.ConnectionString = Configuration.GetConnectionString("ServiceStorageConnection");
	});
}
``` 

Do not forget to add your connection string into your configuration file.



Please also check the following projects in my repositories:
- Forge.Yoda
- Forge.Security.Jwt.Service
- Forge.Security.Jwt.Service.Storage.SqlServer
- Forge.Security.Jwt.Client
- Forge.Security.Jwt.Client.Storage.Browser
- Forge.Wasm.BrowserStorages
- Forge.Wasm.BrowserStorages.NewtonSoft.Json
