import { ArrowRight, Github, BookOpen, Package, Terminal, ExternalLink } from "lucide-react";
import Link from "next/link";
import Image from "next/image";

const NAV_LINKS = [
  { href: "#why", label: "Why Spur" },
  { href: "#pipeline", label: "Pipeline" },
  { href: "#ecosystem", label: "Ecosystem" },
  { href: "https://adelss04.github.io/Spur/", label: "Docs" },
];

const PACKAGES = [
  { name: "Spur", desc: "Core Result<T>, Error, and pipeline operators. Zero dependencies.", required: true },
  { name: "Spur.AspNetCore", desc: "ToHttpResult(), ProblemDetails mapping, SpurMiddleware." },
  { name: "Spur.EntityFrameworkCore", desc: "FirstOrNotFound(), SingleOrNotFound() for EF Core queries." },
  { name: "Spur.FluentValidation", desc: "Bridge FluentValidation rules into Result<T> pipelines." },
  { name: "Spur.MediatR", desc: "ResultHandler<,> base class and pipeline behaviors." },
  { name: "Spur.Testing", desc: "ShouldBeSuccess(), ShouldBeFailure() test assertions." },
  { name: "Spur.Generators", desc: "Source generators for AOT-safe Result mapping." },
  { name: "Spur.Analyzers", desc: "Compile-time checks for ignored results and unsafe access." },
];

export default function Home() {
  return (
    <div className="min-h-screen bg-white text-gray-900">
      {/* ───── NAV ───── */}
      <nav className="fixed top-0 inset-x-0 z-50 bg-white/80 backdrop-blur-md border-b border-gray-100">
        <div className="max-w-6xl mx-auto flex items-center justify-between h-16 px-6">
          <Link href="/" className="flex items-center gap-2.5">
            <Image src="/icon.svg" alt="Spur" width={28} height={28} className="rounded-md" />
            <span className="text-lg font-semibold tracking-tight">Spur</span>
          </Link>

          <div className="hidden md:flex items-center gap-8">
            {NAV_LINKS.map((l) => (
              <Link key={l.href} href={l.href} className="text-sm text-gray-500 hover:text-gray-900 transition-colors">
                {l.label}
              </Link>
            ))}
          </div>

          <div className="flex items-center gap-3">
            <Link
              href="https://github.com/AdelSS04/Spur"
              target="_blank"
              className="hidden sm:flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-900 transition-colors"
            >
              <Github className="w-4 h-4" />
              GitHub
            </Link>
            <Link
              href="https://www.nuget.org/packages/Spur"
              target="_blank"
              className="text-sm font-medium px-4 py-2 rounded-lg bg-gray-900 text-white hover:bg-gray-800 transition-colors"
            >
              Install
            </Link>
          </div>
        </div>
      </nav>

      {/* ───── HERO ───── */}
      <section className="pt-32 pb-20 px-6">
        <div className="max-w-4xl mx-auto text-center">
          <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-emerald-50 text-emerald-700 text-xs font-medium mb-8 ring-1 ring-emerald-200/60">
            <span className="w-1.5 h-1.5 rounded-full bg-emerald-500" />
            .NET 8+ &middot; Native AOT &middot; Zero dependencies
          </div>

          <h1 className="text-5xl sm:text-6xl lg:text-7xl font-extrabold tracking-tight leading-[1.08] mb-6">
            Error handling<br />
            <span className="text-transparent bg-clip-text bg-gradient-to-r from-blue-600 to-violet-600">
              without the exceptions
            </span>
          </h1>

          <p className="text-lg sm:text-xl text-gray-500 max-w-2xl mx-auto mb-10 leading-relaxed">
            Spur gives your .NET code a <code className="text-gray-700 bg-gray-100 px-1.5 py-0.5 rounded text-[0.9em]">Result&lt;T&gt;</code> type
            that carries values <em>or</em> errors through a fluent pipeline — no try/catch, no nulls, no surprises.
          </p>

          <div className="flex flex-col sm:flex-row items-center justify-center gap-4 mb-14">
            {/* Install command */}
            <div className="flex items-center gap-3 px-5 py-3 rounded-xl bg-gray-950 text-gray-300 font-mono text-sm select-all">
              <Terminal className="w-4 h-4 text-gray-500 shrink-0" />
              dotnet add package Spur
            </div>
            <Link
              href="https://adelss04.github.io/Spur/"
              target="_blank"
              className="flex items-center gap-2 px-5 py-3 rounded-xl text-sm font-medium text-gray-700 border border-gray-200 hover:border-gray-300 hover:bg-gray-50 transition-all"
            >
              <BookOpen className="w-4 h-4" />
              Read the docs
            </Link>
          </div>

          {/* Hero code block */}
          <div className="max-w-2xl mx-auto text-left rounded-2xl bg-gray-950 ring-1 ring-white/10 overflow-hidden shadow-2xl">
            <div className="flex items-center gap-2 px-5 py-3 border-b border-white/5">
              <span className="w-3 h-3 rounded-full bg-red-500/70" />
              <span className="w-3 h-3 rounded-full bg-yellow-500/70" />
              <span className="w-3 h-3 rounded-full bg-green-500/70" />
              <span className="ml-3 text-xs text-gray-500 font-mono">UserService.cs</span>
            </div>
            <pre className="p-6 text-sm leading-relaxed overflow-x-auto"><code className="text-gray-300">{`public async Task<Result<UserDto>> GetUser(int id)
{
    return await Result.Start(id)
        .ThenAsync(id => _db.Users.FindAsync(id))
        .Validate(
            user => user.IsActive,
            Error.Validation("User.Inactive", "Account is deactivated"))
        .Map(user => user.ToDto());
}

// In your endpoint:
app.MapGet("/users/{id}", (int id, UserService svc) =>
    svc.GetUser(id).ToHttpResult());
//  → 200 + body  or  404/422 ProblemDetails — automatically`}</code></pre>
          </div>
        </div>
      </section>

      {/* ───── WHY SPUR ───── */}
      <section id="why" className="py-24 px-6 bg-gray-50 border-y border-gray-100">
        <div className="max-w-6xl mx-auto">
          <h2 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4">Why not just throw?</h2>
          <p className="text-gray-500 mb-14 max-w-2xl">
            Exceptions are for <em>exceptional</em> things — disk failures, null refs, network drops.
            Using them for &quot;user not found&quot; or &quot;invalid email&quot; is slow, invisible to the compiler, and painful to test.
          </p>

          <div className="grid lg:grid-cols-2 gap-6">
            {/* BEFORE */}
            <div className="rounded-2xl border border-red-200 bg-white p-8">
              <div className="flex items-center gap-2 mb-5">
                <span className="w-2.5 h-2.5 rounded-full bg-red-500" />
                <span className="text-sm font-semibold text-red-600 uppercase tracking-wide">Typical .NET</span>
              </div>
              <pre className="text-sm leading-relaxed text-gray-800 overflow-x-auto"><code>{`public async Task<UserDto> GetUser(int id)
{
    var user = await _repo.FindAsync(id);
    if (user is null)
        throw new NotFoundException("User not found");
        //       ↑ invisible in the signature

    if (!user.IsActive)
        throw new BusinessException("Account disabled");
        //       ↑ caller has no idea this can happen

    return _mapper.Map<UserDto>(user);
}
// Caller must guess what to catch.
// Benchmark: ~6,000 ns per throw.`}</code></pre>
            </div>

            {/* AFTER */}
            <div className="rounded-2xl border border-emerald-200 bg-white p-8">
              <div className="flex items-center gap-2 mb-5">
                <span className="w-2.5 h-2.5 rounded-full bg-emerald-500" />
                <span className="text-sm font-semibold text-emerald-600 uppercase tracking-wide">With Spur</span>
              </div>
              <pre className="text-sm leading-relaxed text-gray-800 overflow-x-auto"><code>{`public async Task<Result<UserDto>> GetUser(int id)
//                 ↑ the signature tells the full story
{
    return await Result.Start(id)
        .ThenAsync(id => _repo.FindAsync(id))
        .Validate(u => u.IsActive,
            Error.Validation("User.Disabled", "Account disabled"))
        .Map(u => _mapper.Map<UserDto>(u));
}
// Caller sees Result<T> — must handle both paths.
// Benchmark: ~0 ns allocation (readonly struct).`}</code></pre>
            </div>
          </div>

          {/* Value props */}
          <div className="grid sm:grid-cols-3 gap-8 mt-16">
            <div>
              <div className="text-sm font-semibold text-gray-900 mb-2">readonly struct</div>
              <p className="text-sm text-gray-500 leading-relaxed">
                Result&lt;T&gt; is a value type. No heap allocation on the success path — just raw speed.
              </p>
            </div>
            <div>
              <div className="text-sm font-semibold text-gray-900 mb-2">HTTP status built in</div>
              <p className="text-sm text-gray-500 leading-relaxed">
                Every Error carries an HTTP status code. <code className="text-gray-700 bg-gray-100 px-1 rounded text-xs">ToHttpResult()</code> maps
                directly to ProblemDetails — no boilerplate.
              </p>
            </div>
            <div>
              <div className="text-sm font-semibold text-gray-900 mb-2">Compiler-enforced</div>
              <p className="text-sm text-gray-500 leading-relaxed">
                Roslyn analyzers warn when you ignore a Result or access .Value without checking. Mistakes caught at build time.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* ───── PIPELINE ───── */}
      <section id="pipeline" className="py-24 px-6">
        <div className="max-w-6xl mx-auto">
          <h2 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4">The pipeline</h2>
          <p className="text-gray-500 mb-14 max-w-2xl">
            Chain operations cleanly. Each step only runs if the previous one succeeded — errors short-circuit to the end.
          </p>

          <div className="grid lg:grid-cols-[1fr_1.2fr] gap-12 items-start">
            {/* Operator list */}
            <div className="space-y-6">
              {[
                { op: "Then", desc: "Run a fallible operation that returns Result<T>. The bread and butter of the pipeline." },
                { op: "Map", desc: "Transform the success value. Like LINQ Select — but for Results." },
                { op: "Validate", desc: "Assert a condition on the value. Fails with the error you provide." },
                { op: "Tap", desc: "Side effect on success (logging, caching) without changing the value." },
                { op: "Recover", desc: "Catch a specific error and provide a fallback value." },
                { op: "Match", desc: "Terminal operator — branch into two paths and produce a final value." },
              ].map(({ op, desc }) => (
                <div key={op} className="flex gap-4">
                  <code className="shrink-0 text-sm font-semibold text-blue-600 bg-blue-50 px-3 py-1 rounded-lg h-fit ring-1 ring-blue-100">
                    .{op}
                  </code>
                  <p className="text-sm text-gray-600 leading-relaxed pt-0.5">{desc}</p>
                </div>
              ))}
              <p className="text-xs text-gray-400 pt-2">
                Every operator has an async variant (ThenAsync, MapAsync, etc.)
              </p>
            </div>

            {/* Pipeline code example */}
            <div className="rounded-2xl bg-gray-950 ring-1 ring-white/10 overflow-hidden shadow-xl">
              <div className="flex items-center gap-2 px-5 py-3 border-b border-white/5">
                <span className="w-3 h-3 rounded-full bg-red-500/70" />
                <span className="w-3 h-3 rounded-full bg-yellow-500/70" />
                <span className="w-3 h-3 rounded-full bg-green-500/70" />
                <span className="ml-3 text-xs text-gray-500 font-mono">OrderService.cs</span>
              </div>
              <pre className="p-6 text-sm leading-relaxed overflow-x-auto"><code className="text-gray-300">{`public async Task<Result<OrderConfirmation>> PlaceOrder(
    OrderRequest request)
{
    return await Result.Start(request)
        // validate the input
        .Validate(r => r.Items.Count > 0,
            Error.Validation("Order.Empty",
                "Add at least one item"))

        // load the customer — may fail with NotFound
        .ThenAsync(async r =>
            await _customers.FindAsync(r.CustomerId))

        // check business rule
        .Validate(c => c.IsVerified,
            Error.Conflict("Customer.Unverified",
                "Complete verification first"))

        // create the order
        .ThenAsync(async customer =>
            await _orders.CreateAsync(customer, request))

        // fire-and-forget notification
        .Tap(order =>
            _bus.Publish(new OrderPlaced(order.Id)))

        // shape the response
        .Map(order => new OrderConfirmation(
            order.Id, order.Total,
            order.EstimatedDelivery));
}`}</code></pre>
            </div>
          </div>
        </div>
      </section>

      {/* ───── ASP.NET CORE INTEGRATION ───── */}
      <section className="py-24 px-6 bg-gray-50 border-y border-gray-100">
        <div className="max-w-6xl mx-auto">
          <h2 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4">First-class ASP.NET Core</h2>
          <p className="text-gray-500 mb-14 max-w-2xl">
            One line converts any Result&lt;T&gt; to the right HTTP response — 200 with a body on success, RFC 7807 ProblemDetails on failure.
          </p>

          <div className="grid lg:grid-cols-2 gap-6">
            <div className="rounded-2xl bg-gray-950 ring-1 ring-white/10 overflow-hidden">
              <div className="px-5 py-3 border-b border-white/5 text-xs text-gray-500 font-mono">Minimal API</div>
              <pre className="p-6 text-sm leading-relaxed overflow-x-auto"><code className="text-gray-300">{`builder.Services.AddSpur();

app.MapGet("/products/{id}", async (
    int id, ProductService svc) =>
{
    var result = await svc.GetProduct(id);
    return result.ToHttpResult();
    //  Success → 200 OK + JSON body
    //  NotFound → 404 ProblemDetails
    //  Validation → 422 ProblemDetails
});`}</code></pre>
            </div>

            <div className="rounded-2xl bg-gray-950 ring-1 ring-white/10 overflow-hidden">
              <div className="px-5 py-3 border-b border-white/5 text-xs text-gray-500 font-mono">MVC Controller</div>
              <pre className="p-6 text-sm leading-relaxed overflow-x-auto"><code className="text-gray-300">{`[ApiController, Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _svc.DeleteProduct(id);
        return result.ToActionResult(this);
        //  Success → 204 No Content
        //  Failure → mapped ProblemDetails
    }
}`}</code></pre>
            </div>
          </div>
        </div>
      </section>

      {/* ───── ECOSYSTEM ───── */}
      <section id="ecosystem" className="py-24 px-6">
        <div className="max-w-6xl mx-auto">
          <h2 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4">Ecosystem</h2>
          <p className="text-gray-500 mb-14 max-w-2xl">
            The core library has zero external dependencies. Add integration packages only when you need them.
          </p>

          <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {PACKAGES.map((pkg) => (
              <Link
                key={pkg.name}
                href={`https://www.nuget.org/packages/${pkg.name}`}
                target="_blank"
                className="group flex flex-col p-5 rounded-xl border border-gray-200 hover:border-blue-200 hover:shadow-md transition-all bg-white"
              >
                <div className="flex items-center justify-between mb-3">
                  <Package className="w-5 h-5 text-gray-400 group-hover:text-blue-500 transition-colors" />
                  {pkg.required && (
                    <span className="text-[10px] font-semibold uppercase tracking-wider text-blue-600 bg-blue-50 px-2 py-0.5 rounded-full">
                      Core
                    </span>
                  )}
                </div>
                <div className="text-sm font-semibold text-gray-900 mb-1.5">{pkg.name}</div>
                <p className="text-xs text-gray-500 leading-relaxed">{pkg.desc}</p>
                <div className="mt-auto pt-3 flex items-center gap-1 text-xs text-gray-400 group-hover:text-blue-500 transition-colors">
                  NuGet <ExternalLink className="w-3 h-3" />
                </div>
              </Link>
            ))}
          </div>
        </div>
      </section>

      {/* ───── CTA ───── */}
      <section className="py-24 px-6 bg-gray-950 text-white">
        <div className="max-w-3xl mx-auto text-center">
          <h2 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4">
            Ship errors your compiler can see
          </h2>
          <p className="text-gray-400 mb-10 text-lg">
            Install Spur, delete your custom exception classes, and let Result&lt;T&gt; carry the weight.
          </p>

          <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
            <Link
              href="https://github.com/AdelSS04/Spur"
              target="_blank"
              className="flex items-center gap-2.5 px-6 py-3.5 rounded-xl bg-white text-gray-900 font-semibold hover:bg-gray-100 transition-colors text-sm"
            >
              <Github className="w-5 h-5" />
              Star on GitHub
              <ArrowRight className="w-4 h-4" />
            </Link>
            <Link
              href="https://adelss04.github.io/Spur/"
              target="_blank"
              className="flex items-center gap-2.5 px-6 py-3.5 rounded-xl ring-1 ring-white/20 text-white font-semibold hover:bg-white/5 transition-colors text-sm"
            >
              <BookOpen className="w-5 h-5" />
              Documentation
            </Link>
          </div>
        </div>
      </section>

      {/* ───── FOOTER ───── */}
      <footer className="py-12 px-6 border-t border-gray-100">
        <div className="max-w-6xl mx-auto flex flex-col sm:flex-row items-center justify-between gap-6">
          <div className="flex items-center gap-2.5">
            <Image src="/icon.svg" alt="Spur" width={20} height={20} className="rounded" />
            <span className="text-sm text-gray-400">
              &copy; 2026 Spur &middot; MIT License
            </span>
          </div>
          <div className="flex items-center gap-6 text-sm text-gray-400">
            <Link href="https://adelss04.github.io/Spur/" target="_blank" className="hover:text-gray-700 transition-colors">Docs</Link>
            <Link href="https://github.com/AdelSS04/Spur" target="_blank" className="hover:text-gray-700 transition-colors">GitHub</Link>
            <Link href="https://www.nuget.org/packages/Spur" target="_blank" className="hover:text-gray-700 transition-colors">NuGet</Link>
            <Link href="https://github.com/AdelSS04/Spur/discussions" target="_blank" className="hover:text-gray-700 transition-colors">Discussions</Link>
          </div>
        </div>
      </footer>
    </div>
  );
}
