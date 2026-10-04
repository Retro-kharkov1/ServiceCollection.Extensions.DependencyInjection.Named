# Components

<component name="ServiceCollectionExtensions">
Public static class. `AddNamedScoped/Singleton/Transient<TService,TImplementation>(name)` and the
`Func<IServiceProvider,TService>` overloads; `GetNamedService(Type,string)` /
`GetNamedService<TService>(string)` on `IServiceProvider`. Owns the find-or-create logic for the
per-`TService` `NamedServiceFactory<TService>` singleton.
File: `DependencyInjection.Named/ServiceCollectionExtensions.cs`
</component>

<component name="NamedServiceFactory<TService> (internal)">
Holds a `ConcurrentDictionary<(name,Type), Func<IServiceProvider,object>>` for one `TService`.
Registered itself as a DI singleton so the same instance accumulates registrations across multiple
`AddNamed*` calls for that `TService`. Implements `INamedServiceFactory` so it can be looked up and
invoked without knowing `TService` at compile time (used by attribute-driven injection).
`Register<TImplementation>(name, lifetime, constructIndependently)` picks the resolver per named
slot: `sp.GetService<TImplementation>()` when this is the sole registration for that concrete type
(full container caching), or an `ActivatorUtilities.CreateInstance`-based resolver cached per
`IServiceProvider` via `ConditionalWeakTable` when `constructIndependently` is `true` — i.e. another
registration already claimed that `TImplementation` type. See
`docs/architecture/overview.md` `<caveats>` ("Same `TImplementation` under two names").
`Register<TImplementation>(name, func, lifetime)` (used by the `Func<IServiceProvider,TService>`
overloads) applies the same lifetime-aware caching to the caller-supplied delegate: a single shared
instance for `Singleton`, a `ConditionalWeakTable`-cached instance per `IServiceProvider` for
`Scoped`, and no caching for `Transient`. See `docs/architecture/overview.md` `<caveats>` ("Func
overload respects lifetime caching").
File: `DependencyInjection.Named/NamedServiceFactory.cs`
</component>

<component name="AddPropertyInjectedServicesExtension">
Public static class, single method `AddNamedInjectedServices()`. Post-processes every
`ServiceDescriptor` already in the collection: derives the implementation `Type` (from
`ImplementationType`, or the generic argument of `ImplementationFactory`, or
`ImplementationInstance`'s type), then replaces the descriptor with a factory that (a) builds the
instance — via the original factory/instance, or by resolving constructor parameters (honoring
`[Named]` on parameters), and (b) assigns any `[Inject]`/`[Named]`-annotated public writable
properties. Skips types that are themselves `INamedServiceFactory` to avoid infinite recursion.
File: `DependencyInjection.Named/AddPropertyInjectedServicesExtension.cs`
</component>

<component name="NamedAttribute / InjectAttribute">
`InjectAttribute` (property/field target) marks a property for post-processing injection with no
name (resolves the property's declared type normally). `NamedAttribute` (property/parameter target,
inherits `InjectAttribute`) additionally carries a `Name` used for named resolution — applicable to
constructor parameters (read by `AddPropertyInjectedServicesExtension`'s ctor-parameter path) and to
properties (read by its property-injection path).
Files: `DependencyInjection.Named/{NamedAttribute,InjectAttribute}.cs`
</component>

<component name="HttpClientBuilderNamedExtensions">
Public static class extending `IHttpClientBuilder`: `ConfigureNamedHttpClient`,
`AddNamedHttpMessageHandler<THandler>`, `ConfigureNamedPrimaryHttpMessageHandler<THandler>`,
`AddNamedTypedClient<TClient,TImplementation>` / `AddNamedTypedClient<TClient>(factory)`. Bridges
this package's named registry into `Microsoft.Extensions.Http`'s own per-name
`HttpClientFactoryOptions` configuration.
File: `DependencyInjection.Named/Http/HttpClientBuilderNamedExtensions.cs`
</component>

<component name="RefitExtension (DependencyInjection.Named.Refit)">
Public static class, `AddNamedRefitClient<T>(services, name, settings|settingsAction)`. Registers a
named `SettingsFor<T>`, a named `IRequestBuilder<T>` built from those settings, wires
`AddHttpClient(name)` with an optional inner `HttpMessageHandler` derived from
`RefitSettings.AuthorizationHeaderValueGetter`/`AuthorizationHeaderValueWithParamGetter` (via
`AuthenticatedHttpClientHandler`/`AuthenticatedParameterizedHttpClientHandler`), and finally adds a
named typed client that calls `RestService.For<T>`.
File: `DependencyInjection.Named.Refit/RefitExtension.cs`
</component>

<component name="DependencyInjection.Named.Test">
NUnit test project (targets net8.0, references the core project). `DIInjectTest` covers
constructor-named, property-named, and default/"custom name" injection scenarios end-to-end through
a real `ServiceProvider`. `Http/HttpClientNamedExtensionsTest` covers the `HttpClientBuilder`
extension methods (argument-null guards + "does not throw" smoke coverage) — added in PR #1
(2026, "Add comprehensive tests for HttpClient named extensions").
</component>
