# Named Refit client

<overview>
`ServiceCollection.Extensions.DependencyInjection.Named.Refit` (separate NuGet package) registers a
[Refit](https://github.com/reactiveui/refit) client interface `T` under a string name, so multiple
Refit clients for the same interface (different base addresses/auth) can coexist.
</overview>

<api>
```CSharp
services.AddNamedRefitClient<IMyApi>("apiOne", settings: mySettings);
services.AddNamedRefitClient<IMyApi>("apiTwo", settingsAction: sp => BuildSettingsFor(sp));

var apiOne = provider.GetNamedService<IMyApi>("apiOne");
```
</api>

<architecture>
`AddNamedRefitClient<T>` wires, all under the same `name`: a named `SettingsFor<T>`, a named
`IRequestBuilder<T>` built from those settings, `AddHttpClient(name)` with an inner handler derived
from `RefitSettings.AuthorizationHeaderValueGetter` / `AuthorizationHeaderValueWithParamGetter`
(`AuthenticatedHttpClientHandler` / `AuthenticatedParameterizedHttpClientHandler`), and a named typed
client that calls `RestService.For<T>(httpClient, requestBuilder)`.
</architecture>

<topics>
- [Named HttpClient](named-httpclient.md) — the underlying `AddNamedTypedClient` this builds on.
</topics>
