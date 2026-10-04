# Named HttpClient

<overview>
Extensions on `IHttpClientBuilder` that let a **named** dependency (a named message handler, a
named typed-client factory, etc.) participate in configuring an `HttpClient` registered through the
normal `AddHttpClient(...)` factory pattern.
</overview>

<api>
- `builder.ConfigureNamedHttpClient(name, (sp, client) => { ... })` — registers a named
  `IConfigureOptions<HttpClientFactoryOptions>` that appends a client-configuration action.
- `builder.AddNamedHttpMessageHandler<THandler>(name)` — resolves `THandler` via
  `GetNamedService<THandler>(name)` and appends it as an additional handler.
- `builder.ConfigureNamedPrimaryHttpMessageHandler<THandler>(name)` — same, but sets it as the
  primary handler.
- `builder.AddNamedTypedClient<TClient,TImplementation>(name)` / `AddNamedTypedClient<TClient>(factory)`
  — named typed-client registration built on top of `ITypedHttpClientFactory<TImplementation>`.
</api>

<topics>
- [Named Refit client](named-refit-client.md) builds directly on `AddNamedTypedClient`.
</topics>
