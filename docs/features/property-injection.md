# Property / constructor injection by name

<overview>
Attribute-driven injection on top of the normal DI container: `[Named(name)]` picks a named
registration for a constructor parameter or a property; plain `[Inject]` resolves a property by
type with no name. Requires one explicit opt-in call, `AddNamedInjectedServices()`, made **last**
after all other registrations.
</overview>

<api>
```CSharp
public class BusinesRepOne : IBusinesOne
{
    public BusinesRepOne([Named("name1")] IRepository repository) { /* ... */ }
}

public class BusinesPropRepOne
{
    [Named("name1")]
    public IRepository Repository { get; set; }
}

public class BusinesPropInject
{
    [Inject]
    public IRepository Repository { get; set; } // resolved by type, no name
}

ServiceProvider = new ServiceCollection()
    .AddNamedScoped<IRepository, RepositoryOne>("name1")
    .AddScoped<IBusinesOne, BusinesRepOne>()
    .AddNamedInjectedServices()     // must be last
    .BuildServiceProvider();
```
</api>

<caveats>
- `AddNamedInjectedServices()` snapshots the service collection at call time — anything registered
  after it runs is not processed.
- Properties are only injected if annotated with `[Inject]` or `[Named]` (`NamedAttribute` inherits
  `InjectAttribute`) — plain public settable properties are left untouched.
</caveats>
