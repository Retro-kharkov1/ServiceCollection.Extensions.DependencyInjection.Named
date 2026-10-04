# Conventions

<naming>
- One public static extension class per concern (`ServiceCollectionExtensions`,
  `AddPropertyInjectedServicesExtension`, `HttpClientBuilderNamedExtensions`, `RefitExtension`) —
  don't add unrelated extension methods to an existing class; start a new one named for the concern.
- Internal resolution machinery (`NamedServiceFactory<T>`, `INamedServiceFactory`) stays `internal` —
  never widen these to `public`; the only supported entry points are the extension methods and
  `GetNamedService`.
</naming>

<patterns>
- Every `AddNamed*` overload follows the exact method-name/parameter shape of the corresponding
  built-in `IServiceCollection`/`IHttpClientBuilder` method it augments, with `string name` added —
  keeps the API discoverable for anyone who already knows the base DI API.
- New named-wrapper features (a new "named X" capability) should follow the same find-or-create
  `NamedServiceFactory<T>` pattern already used by `ServiceCollectionExtensions`, not a parallel
  registry.
</patterns>

<testing>
- NUnit, one test class per extended type/concern (`DIInjectTest` for DI/attribute behavior,
  `Http/HttpClientNamedExtensionsTest` for the `IHttpClientBuilder` extensions).
- Constructor/property injection tests build a real `ServiceProvider` and assert on resolved
  instance behavior rather than mocking the container.
- Argument-null guards on public extension methods get an explicit `Assert.Throws<ArgumentNullException>` test.
- The Refit package now has dedicated test coverage under `Test/Refit/` (`UniqueNameTest`,
  `AuthenticatedHttpClientHandlerTest`, `AuthenticatedParameterizedHttpClientHandlerTest`, `RefitExtensionTest`).
</testing>

<branching>
Single `main` branch history; feature work has used short-lived branches merged via PR (e.g.
`copilot/add-tests-for-httpclient-extensions` → PR #1). No CI pipeline configured in this repo as of
2026-09-14 — builds/tests/packaging are run locally or via Visual Studio.
</branching>
