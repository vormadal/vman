import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

// Public routes that don't require authentication
const PUBLIC_ROUTES = ['/login', '/accept-invite'];

// Routes that don't require profile completion
const PROFILE_EXEMPT_ROUTES = ['/complete-profile'];

// In standalone (Node runtime) behind a reverse proxy, request.url resolves to the
// internal listen address (e.g. http://localhost:3000) and is not a usable public base,
// so new URL(path, request.url) sends the browser to localhost or throws ERR_INVALID_URL.
// Next.js also re-parses the response's Location through new URL(), so a relative Location
// throws too. Build an absolute URL from the proxy's forwarded headers, which carry the
// real public origin (nginx sets X-Forwarded-Host / X-Forwarded-Proto).
function redirect(request: NextRequest, path: string) {
  const host = request.headers.get('x-forwarded-host') ?? request.headers.get('host');
  const proto = request.headers.get('x-forwarded-proto') ?? request.nextUrl.protocol.replace(':', '');
  return NextResponse.redirect(`${proto}://${host}${path}`);
}

export function proxy(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // Check if route is public
  const isPublicRoute = PUBLIC_ROUTES.some(route => pathname.startsWith(route));
  const isProfileExempt = PROFILE_EXEMPT_ROUTES.some(route => pathname.startsWith(route));

  // Get auth token from localStorage (stored as JSON by zustand persist)
  // Since middleware runs on edge, we need to check via cookie or header
  // Let's use a cookie approach
  const authCookie = request.cookies.get('auth-storage');

  let isAuthenticated = false;
  let isProfileComplete = true;
  if (authCookie) {
    try {
      const authData = JSON.parse(authCookie.value);
      isAuthenticated = !!authData?.state?.accessToken;
      isProfileComplete = authData?.state?.isProfileComplete !== false;
    } catch (error) {
      // Invalid cookie data
      isAuthenticated = false;
    }
  }

  // Redirect to login if accessing protected route without auth
  if (!isPublicRoute && !isAuthenticated) {
    return redirect(request, `/login?redirect=${encodeURIComponent(pathname)}`);
  }

  // Redirect to profile completion if authenticated but profile incomplete
  if (isAuthenticated && !isProfileComplete && !isProfileExempt) {
    return redirect(request, '/complete-profile');
  }

  // Redirect to home if accessing auth pages while logged in with complete profile
  if (isPublicRoute && isAuthenticated && isProfileComplete) {
    return redirect(request, '/videos');
  }

  return NextResponse.next();
}

export const config = {
  // Match all routes except static files and API routes
  matcher: ['/((?!api|_next/static|_next/image|favicon.ico|.*\\..*).*)'],
};
