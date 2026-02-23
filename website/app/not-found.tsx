import Link from "next/link";

export default function NotFound() {
  return (
    <div className="min-h-screen flex items-center justify-center px-6">
      <div className="text-center">
        <h1 className="text-6xl font-extrabold text-gray-900 mb-4">404</h1>
        <p className="text-lg text-gray-500 mb-8">Page not found.</p>
        <Link
          href="/"
          className="text-sm font-medium px-5 py-2.5 rounded-lg bg-gray-900 text-white hover:bg-gray-800 transition-colors"
        >
          Back to home
        </Link>
      </div>
    </div>
  );
}
