# ServiceCollection.Extensions.DependencyInjection.Named

Extensions for `Microsoft.Extensions.DependencyInjection`'s `IServiceCollection` that let you
register and resolve **multiple implementations of the same service type side by side**, keyed
by a string name — something the built-in container does not support out of the box.

Three ways to consume a named registration:

- **By name, explicitly** — `provider.GetNamedService<TService>(name)`.
- **By property**, using the `[Named("...")]` / `[Inject]` attributes plus
  `AddNamedInjectedServices()`, which rewrites the container's service descriptors so injectable
  properties get populated automatically when the instance is created.
- **By constructor parameter**, using `[Named("...")]` on a constructor argument — also wired up
  by `AddNamedInjectedServices()`.

A third package, `DependencyInjection.Named.Refit`, layers a **named Refit client** and a
**named `IHttpClientBuilder`** (via `DependencyInjection.Named`'s own named `HttpClient`
extensions) on top, so you can register several typed HTTP/Refit clients against the same
interface, each with its own base configuration, handler pipeline, and authorization header.

## Install

The library is published to NuGet as two packages:

```xml
<PackageReference Include="ServiceCollection.Extensions.DependencyInjection.Named" Version="1.2.2" />

<!-- Optional: named Refit client support -->
<PackageReference Include="ServiceCollection.Extensions.DependencyInjection.Named.Refit" Version="1.2.2" />
```

or via the CLI:

```bash
dotnet add package ServiceCollection.Extensions.DependencyInjection.Named
dotnet add package ServiceCollection.Extensions.DependencyInjection.Named.Refit
```

Targets `net461` and `netstandard2.1`, so it works on both classic .NET Framework and modern
.NET (Core/5+/8+) projects. Check [nuget.org](https://www.nuget.org/packages/ServiceCollection.Extensions.DependencyInjection.Named)
for the latest published version.

## Usage

### 1. Named registration + resolution

Register several implementations of the same interface under different names, then resolve the
one you need by name:

```csharp
using Microsoft.Extensions.DependencyInjection;
using ServiceCollection.Extensions.DependencyInjection.Named;

var provider = new ServiceCollection()
    .AddNamedScoped<IRepository, RepositoryOne>(Consts.Name1)
    .AddNamedScoped<IRepository, RepositoryTwo>(Consts.Name2)
    .AddNamedScoped<IRepository>(sp => new Repository(Consts.CustomName1), Consts.CustomName1)
    .AddNamedInjectedServices()
    .BuildServiceProvider();

IRepository repoOne = provider.GetNamedService<IRepository>(Consts.Name1);
IRepository repoTwo = provider.GetNamedService<IRepository>(Consts.Name2);
```

`AddNamedScoped`/`AddNamedSingleton`/`AddNamedTransient` are available both as
`TService`/`TImplementation` pairs and as factory-delegate overloads
(`Func<IServiceProvider, TService>`).

### 2. Named injection by constructor parameter

Decorate a constructor parameter with `[Named("...")]` and let `AddNamedInjectedServices()`
resolve the correctly-named dependency automatically:

```csharp
public class BusinesRepOne : IBusinesOne
{
    public BusinesRepOne([Named(Consts.Name1)] IRepository repository)
    {
        // repository is resolved from the "Name1" registration
    }
}
```

### 3. Named injection by property (model-builder-to-action style)

Mark a settable property with `[Named("...")]` (for a specific named dependency) or plain
`[Inject]` (for a normal, unnamed dependency), and `AddNamedInjectedServices()` will populate it
after the instance is built — without you writing the wiring by hand:

```csharp
public class BusinesPropRepOne
{
    [Named(Consts.Name1)]
    public IRepository Repository { get; set; }
}

public class BusinesPropRepOne
{
    [Inject]
    public IRepository Repository { get; set; }
}
```

Call `services.AddNamedInjectedServices()` once, after all your other registrations, to activate
property/constructor injection for every service already added to the collection.

### 4. Named `HttpClient` / typed clients

`DependencyInjection.Named` also extends `IHttpClientBuilder` so `IHttpClientFactory`,
message handlers, and typed clients can be registered under a name and resolved independently
of the built-in `HttpClientFactoryOptions` name collisions:

```csharp
services.AddHttpClient(Consts.Name1)
    .ConfigureNamedHttpClient(Consts.Name1, (sp, client) =>
    {
        client.BaseAddress = new Uri("https://api.example.com/");
    })
    .AddNamedHttpMessageHandler<MyDelegatingHandler>(Consts.Name1);

var factory = provider.GetNamedService<IHttpClientFactory>(Consts.Name1);
```

### 5. Named Refit client

```csharp
using Refit;
using ServiceCollection.Extensions.DependencyInjection.Named.Refit;

services.AddNamedRefitClient<IMyApi>(Consts.Name1, sp => new RefitSettings
{
    AuthorizationHeaderValueGetter = (request, ct) => Task.FromResult("token-for-name1")
});

IMyApi client = provider.GetNamedService<IMyApi>(Consts.Name1);
```

`AddNamedRefitClient<T>` wires up a named `IHttpClientBuilder`, an `IRequestBuilder<T>`, and the
underlying `HttpClient`/`HttpMessageHandler` (including `AuthorizationHeaderValueGetter` /
`AuthorizationHeaderValueWithParamGetter` support), all scoped to the given name.

## Support this project

[![Sponsor](https://img.shields.io/badge/Sponsor-GitHub%20Sponsors-EA4AAA?logo=github-sponsors)](https://github.com/sponsors/Retro-kharkov1)

Buy Me a Coffee: [buymeacoffee.com/retro.kharkov](https://buymeacoffee.com/retro.kharkov)

USDT (crypto): <!-- TODO: add wallet address once created -->

## License

Licensed under the [MIT License](LICENSE).

## Contributing

Issues and pull requests are welcome. If you're proposing a behavioral change, please include or
update the relevant tests in `DependencyInjection.Named.Test` — the project targets `net8.0` for
tests and `net461`/`netstandard2.1` for the shipped libraries, so please make sure both build
before opening a PR.
