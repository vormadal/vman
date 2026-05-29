import type { NextRequest } from 'next/server';

// NextResponse.redirect requires an absolute URL, which breaks behind a reverse
// proxy because request.url resolves to the internal Node address (localhost:3000).
// A relative Location header is valid HTTP — the browser resolves it against the
// URL it already has, so this works correctly regardless of how the app is hosted.
function relativeRedirect(path: string) {
  return new Response(null, { status: 307, headers: { Location: path } });
}

// Public routes that don't require authentication
const PUBLIC_ROUTES = ['/login', '/accept-invite'];

// Routes that don't require profile completion
const PROFILE_EXEMPT_ROUTES = ['/complete-profile'];

export function middleware(request: NextRequest) {
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
    return relativeRedirect(`/login?redirect=${encodeURIComponent(pathname)}`);
  }

  // Redirect to profile completion if authenticated but profile incomplete
  if (isAuthenticated && !isProfileComplete && !isProfileExempt) {
    return relativeRedirect('/complete-profile');
  }

  // Redirect to home if accessing auth pages while logged in with complete profile
  if (isPublicRoute && isAuthenticated && isProfileComplete) {
    return relativeRedirect('/videos');
  }

  return NextResponse.next();
}

export const config = {
  // Match all routes except static files and API routes
  matcher: ['/((?!api|_next/static|_next/image|favicon.ico|.*\\..*).*)'],
};
