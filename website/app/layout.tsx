import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Spur — Result-oriented error handling for .NET",
  description:
    "A lightweight Result<T> library for .NET 8+. Replace exceptions with type-safe, HTTP-aware error handling. Zero allocations, fluent pipeline API, Native AOT ready. Integrates with ASP.NET Core, EF Core, FluentValidation, and MediatR.",
  keywords: [
    "dotnet result type",
    "csharp error handling",
    "Result pattern",
    "railway oriented programming",
    "ASP.NET Core result",
    "dotnet functional programming",
    "spur nuget",
    "csharp Result T",
    "dotnet error handling without exceptions",
    "ProblemDetails",
  ],
  authors: [{ name: "AdelSS04" }],
  icons: {
    icon: "/favicon.ico",
    apple: "/apple-touch-icon.png",
  },
  metadataBase: new URL("https://adelss04.github.io"),
  alternates: {
    canonical: "/Spur",
  },
  openGraph: {
    title: "Spur — Result-oriented error handling for .NET",
    description:
      "Type-safe Result<T> with fluent pipelines, HTTP-first errors, and zero allocations. Drop-in integrations for ASP.NET Core, EF Core, MediatR & more.",
    url: "https://adelss04.github.io/Spur",
    siteName: "Spur",
    type: "website",
    locale: "en_US",
  },
  twitter: {
    card: "summary_large_image",
    title: "Spur — Result-oriented error handling for .NET",
    description:
      "Replace try/catch with Result<T>. Type-safe, zero-alloc, AOT-ready error handling for modern .NET.",
  },
  robots: {
    index: true,
    follow: true,
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className="antialiased">
        {children}
      </body>
    </html>
  );
}
