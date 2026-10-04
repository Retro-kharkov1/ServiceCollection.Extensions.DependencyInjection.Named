# Overview

<overview>
`ServiceCollection.Extensions.DependencyInjection.Named` is a personal open-source NuGet package
(author: Retro-kharkov1) that extends Microsoft's `IServiceCollection` / `IHttpClientBuilder` with
**named** service registration and resolution, plus **property/constructor injection by name**.
The base .NET DI container only supports one registration per service type; this package adds a
side-channel keyed-by-string registry on top of it without replacing or forking the container.
</overview>

<problem>
`Microsoft.Extensions.DependencyInjection` has no first-class support for registering multiple
implementations of the same interface and resolving a specific one by a string key (a common need
when a consumer must talk to several instances of the same kind of dependency — e.g. two
repositories of the same interface, or several named `HttpClient`/Refit clients pointing at
different base addresses). Consumers otherwise resort to factory-of-factories, `IEnumerable<T>` +
manual filtering, or per-instance wrapper interfaces.
</problem>

<audience>
.NET developers (own projects and consumers of the public NuGet package) who need named DI
registrations, name-based constructor/property injection via `[Named]`/`[Inject]` attributes, or a
named `HttpClient`/Refit client without hand-rolling a keyed-service workaround.
</audience>

<scope>
In scope: named registration/resolution for the DI container, attribute-driven constructor and
property injection, named `HttpClient` configuration, named Refit client registration.
Out of scope: this package does not replace `IServiceCollection`/`IServiceProvider`, does not
provide its own DI container, and does not manage service lifetimes beyond what
`Microsoft.Extensions.DependencyInjection` itself supports (Singleton/Scoped/Transient).
</scope>

<distribution>
Published as two NuGet packages built from this repo:
- `ServiceCollection.Extensions.DependencyInjection.Named` — the core package.
- `ServiceCollection.Extensions.DependencyInjection.Named.Refit` — optional add-on for named
  Refit clients, depends on the core package + `Refit.HttpClientFactory`.
</distribution>
