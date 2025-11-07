'use client';

export default function ApiTestPage() {
  const apiUrl = process.env.NEXT_PUBLIC_API_BASE_URL || 'NOT SET';

  return (
    <div className="container mx-auto p-8">
      <h1 className="text-2xl font-bold mb-4">API Configuration Test</h1>
      <div className="bg-gray-100 p-4 rounded">
        <p className="font-mono">
          <strong>NEXT_PUBLIC_API_BASE_URL:</strong> {apiUrl}
        </p>
      </div>
      <p className="mt-4 text-sm text-gray-600">
        This page shows the API base URL that was baked into the build.
        It should be: <code className="bg-gray-200 px-2 py-1 rounded">https://api.taktiq.app/api</code>
      </p>
    </div>
  );
}
