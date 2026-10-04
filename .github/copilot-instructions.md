<docs-reference>
Before implementing any feature or fixing any bug — read `docs/README.md` and follow links to the
relevant section. Ensure implementation matches the documented architecture, patterns, and
component responsibilities. See especially `docs/architecture/overview.md` and
`docs/architecture/components.md`.
</docs-reference>

<reuse-first>
Always search for an existing extension class/pattern before writing new code. Priority order:
1. Extend an existing extension class if the new method belongs to the same concern
   (`ServiceCollectionExtensions`, `AddPropertyInjectedServicesExtension`,
   `HttpClientBuilderNamedExtensions`, `RefitExtension`).
2. Follow the existing find-or-create `NamedServiceFactory<T>` pattern for any new "named X"
   capability.
3. New extension class only if the concern is genuinely new.
</reuse-first>

<deduplication>
If the same logic appears in 2+ places during implementation — extract it into a shared private
helper in the owning extension class. Don't duplicate the find-or-create-factory logic that already
exists in `ServiceCollectionExtensions`.
</deduplication>

<context7>
For any NuGet package used in this project (`Microsoft.Extensions.DependencyInjection`,
`Microsoft.Extensions.Http`, `Refit`/`Refit.HttpClientFactory`, `NUnit`) — always fetch docs via
Context7 before implementing. Use: resolve-library-id → get-library-docs with focused topic. Never
guess API signatures from memory.
</context7>

<architecture-boundaries>
This project follows a flat, single-purpose extension-library style (see
`docs/architecture/overview.md`) — no Clean/Onion layering. The one boundary that matters:
`NamedServiceFactory<TService>` and `INamedServiceFactory` are `internal` — never widen them to
`public`. The public surface is only the extension methods
(`ServiceCollectionExtensions`/`AddPropertyInjectedServicesExtension`/
`HttpClientBuilderNamedExtensions`/`RefitExtension`) and `GetNamedService`. If a change seems to
require exposing the internal factory, stop and flag it rather than quietly widening its access.
</architecture-boundaries>

<code-style>
- One public static extension class per concern; method names mirror the base
  `IServiceCollection`/`IHttpClientBuilder` method they augment, with a `string name` parameter
  added.
- `AddNamedInjectedServices()` must remain safe to call only once, last, after all other
  registrations — do not change it to be called multiple times or mid-chain without updating
  `docs/features/property-injection.md`.
</code-style>

<patterns>
- Find-or-create singleton registry per `TService`: `ServiceCollectionExtensions.AddNamedServiceImpl`.
- Post-processing service-descriptor replacement pass:
  `AddPropertyInjectedServicesExtension.AddNamedInjectedServices`.
- Attribute-driven metadata: `NamedAttribute` (inherits `InjectAttribute`) for named injection,
  `InjectAttribute` alone for unnamed property injection.
</patterns>

<tech-stack>
- `DependencyInjection.Named` / `DependencyInjection.Named.Refit`: net461 + netstandard2.1;
  `Microsoft.Extensions.DependencyInjection` 6.0.0, `Microsoft.Extensions.Http` 6.0.0,
  `Refit.HttpClientFactory` 6.3.2 (Refit project only).
- `DependencyInjection.Named.Test`: net8.0, NUnit.
- Both library projects pack a NuGet package on every build (`GeneratePackageOnBuild=True`).
</tech-stack>
