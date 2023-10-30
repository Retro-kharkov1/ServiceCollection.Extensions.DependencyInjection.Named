# Named bindings
It wokds as extenstions for service collection 
use `AddNamed...` functions and finiehs with `AddNamedInjectedServices` call

```CSharp
 ServiceProvider =  new Microsoft.Extensions.DependencyInjection.ServiceCollection()
   .AddNamedScoped<IRepository, RepositoryOne>(Consts.Name1)
   .AddNamedScoped<IRepository, RepositoryTwo>(Consts.Name2);
   .AddNamedScoped<IRepository>((sp) => new Repository(Consts.CustomName1), Consts.CustomName1);
   .AddNamedInjectedServices()
   .BuildServiceProvider();
```

Class examples use `Named` and `Inject` attribute 
```CSharp
    public class BusinesRepOne : IBusinesOne
    {
        public BusinesRepOne([Named(Consts.Name1)] IRepository repository) {
            //Your code ...
        }

    }

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
