# Named registration & resolution

<overview>
Register multiple implementations of the same service type under different string names, and
resolve a specific one by name — something the base `IServiceCollection` doesn't support natively.
</overview>

<api>
```CSharp
services
  .AddNamedScoped<IRepository, RepositoryOne>("name1")
  .AddNamedScoped<IRepository, RepositoryTwo>("name2")
  .AddNamedScoped<IRepository>(sp => new Repository("customName"), "customName");

var repo = provider.GetNamedService<IRepository>("name1");
```
`AddNamedScoped` / `AddNamedSingleton` / `AddNamedTransient` — both the `<TService,TImplementation>`
and the `Func<IServiceProvider,TService>` overloads, mirroring `IServiceCollection`'s own
`Add{Lifetime}` shape.
</api>

<caveats>
- Registering the SAME `TImplementation` concrete type under two different names (same or different
  `TService`) is supported — each name resolves its own, distinct instance — but the second (and
  any later) named registration sharing that type is built independently via
  `ActivatorUtilities.CreateInstance` rather than through the container's own cached slot for that
  type, so `[Named]`/`[Inject]` constructor-parameter attributes on it are not honored by
  `AddNamedInjectedServices()`. See [Architecture — overview](../architecture/overview.md)
  `<caveats>` for the full explanation.
</caveats>

<topics>
- [Architecture — overview](../architecture/overview.md) for how the registry stores names per
  `TService`.
</topics>
