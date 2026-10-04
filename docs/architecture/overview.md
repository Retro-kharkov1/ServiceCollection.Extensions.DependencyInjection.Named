# Overview

<overview>
The solution is 3 projects, single-layer, no internal architectural boundaries beyond
"public extension API" vs. "internal resolution engine":

```
ServiceCollection.Extensions.DependencyInjection.Named.sln
├── DependencyInjection.Named            ← core package (net461; netstandard2.1)
│   ├── ServiceCollectionExtensions.cs        AddNamed{Scoped,Singleton,Transient}, GetNamedService
│   ├── AddPropertyInjectedServicesExtension.cs   AddNamedInjectedServices (post-processing pass)
│   ├── NamedServiceFactory.cs (internal)     per-TService keyed registry (ConcurrentDictionary)
│   ├── INamedServiceFactory.cs (internal)    non-generic facade used for reflection lookups
│   ├── NamedAttribute.cs / InjectAttribute.cs constructor/property injection markers
│   └── Http/HttpClientBuilderNamedExtensions.cs   named HttpClientFactory wiring
├── DependencyInjection.Named.Refit      ← optional add-on (net461; netstandard2.1)
│   ├── RefitExtension.cs                     AddNamedRefitClient<T>
│   └── Authenticated*HttpClientHandler.cs    auth header injection for Refit's inner handler
└── DependencyInjection.Named.Test       ← NUnit tests (net8.0)
    ├── DIInjectTest.cs                       constructor/property named-injection scenarios
    └── Http/HttpClientNamedExtensionsTest.cs named HttpClient extension scenarios
```
</overview>

<style>
Flat static-extension-method style (no Clean/Onion layering — appropriate for a single-purpose
DI-extension library with no domain model of its own). The one internal architectural rule that
matters: **`NamedServiceFactory<TService>` and `INamedServiceFactory` are `internal`** — the public
surface is only the extension methods in `ServiceCollectionExtensions`,
`AddPropertyInjectedServicesExtension`, `HttpClientBuilderNamedExtensions`, and `RefitExtension`.
No boundary violations found — the Refit project depends on the core project via `ProjectReference`
(one direction only), and the core project depends on nothing else in the solution.
</style>

<data-flow>
Registration time (`AddNamed*` calls):
1. Caller calls `services.AddNamedScoped<TService, TImpl>(name)` (or the `Func<IServiceProvider,T>`
   overload).
2. `ServiceCollectionExtensions` finds-or-creates a singleton `NamedServiceFactory<TService>` already
   registered in the collection, and calls `.Register(name, ...)` on it — building up a
   `(name, Type) → factory-delegate` map inside that one `NamedServiceFactory<TService>` instance.
3. The concrete implementation type is ALSO registered normally (`AddSingleton/Scoped/Transient`)
   so the container can construct it — but never under `TService`, only under its own concrete
   type, to avoid colliding with a caller's own non-named registration of `TService`.

Resolution time:
- `provider.GetNamedService<TService>(name)` → looks up the `NamedServiceFactory<TService>`
  singleton via reflection (`GetServices(typeof(NamedServiceFactory<TService>)).LastOrDefault()`),
  then calls its non-generic `INamedServiceFactory.Resolve(name, provider)`.
- `[Named(name)]` on a constructor parameter or property is only interpreted by
  `AddNamedInjectedServices()` — a one-time post-processing pass, called once after all
  registrations, that walks every `ServiceDescriptor` currently in the collection and, for services
  whose implementation type has `[Named]` constructor params or `[Inject]`/`[Named]` properties,
  **replaces** the descriptor with one whose factory builds the instance manually (resolving each
  named dependency via `GetNamedService`) and then assigns the injectable properties.
</data-flow>

<caveats>
- `AddNamedInjectedServices()` MUST be the last call in the registration chain — it snapshots
  `services.ToArray()` and processes what's registered at that point only.
- Named HttpClient/Refit wiring layers `AddNamedTransient`/`AddNamedSingleton` under
  `Microsoft.Extensions.Http`'s own named-options system (`IConfigureOptions<HttpClientFactoryOptions>`
  keyed by `IHttpClientBuilder.Name`) rather than replacing it — two independent "named" systems
  (this package's, and `IHttpClientFactory`'s built-in one) are deliberately kept aligned by name.
- **Same `TImplementation` under two names ("cross-injection"):** for the
  `AddNamed{Scoped,Singleton,Transient}<TService,TImplementation>(name)` overload, the FIRST
  registration of a given concrete `TImplementation` type still resolves via the container's own
  `sp.GetService<TImplementation>()` (full Scoped/Singleton caching, zero behavior change). If a
  LATER named registration reuses that SAME `TImplementation` type — under the same or a different
  `TService`/name — `AddNamedServiceImpl` detects the collision, skips adding another container
  descriptor for that type, and has `NamedServiceFactory<TService>` build that named slot's instance
  independently via `ActivatorUtilities.CreateInstance`, cached per `IServiceProvider` (i.e. per
  scope, or once for the singleton root) via a private `ConditionalWeakTable` rather than through the
  shared, type-keyed container slot. Before this fix, every named registration sharing a
  `TImplementation` type collapsed onto the same shared container instance, so resolving by one name
  could silently return another name's object. The trade-off: a colliding named registration's
  `[Named]`/`[Inject]` constructor-parameter attributes are NOT honored by
  `AddNamedInjectedServices()` (which only rewrites descriptors actually present in the container) —
  give colliding named implementations no attribute-driven dependencies, or register them via the
  `Func<IServiceProvider,TService>` overload instead.
- **`Func<IServiceProvider,TService>` overload respects lifetime caching:** the
  `AddNamed{Scoped,Singleton,Transient}<TService>(func, name)` overloads pass their `ServiceLifetime`
  down to `NamedServiceFactory<TService>.Register<TImplementation>(name, func, lifetime)`, which wraps
  `func` the same way the `TImplementation` overload wraps `ActivatorUtilities.CreateInstance`: a
  single cached instance for `Singleton` (shared across every `IServiceProvider`, since the factory
  itself is a container singleton), a `ConditionalWeakTable<IServiceProvider,object>`-backed cache per
  scope for `Scoped`, and no caching (fresh call to `func` every resolution) for `Transient`. Before
  this fix, every lifetime for the Func overload invoked `func` fresh on each `Resolve`/
  `GetNamedService` call, so a "singleton" Func-based registration silently behaved like a transient
  one when resolved through this package's named registry (the container's own `AddSingleton(func)`
  descriptor was still correctly cached, but nothing here ever went through it).
</caveats>
