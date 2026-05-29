import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

// Public routes that don't require authentication
const PUBLIC_ROUTES = ['/login', '/accept-invite'];

// Routes that don't require profile completion
const PROFILE_EXEMPT_ROUTES = ['/complete-profile'];

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
    const loginUrl = new URL('/login', request.url);
    loginUrl.searchParams.set('redirect', pathname);
    return NextResponse.redirect(loginUrl);
  }

  // Redirect to profile completion if authenticated but profile incomplete
  if (isAuthenticated && !isProfileComplete && !isProfileExempt) {
    const profileUrl = new URL('/complete-profile', request.url);
    return NextResponse.redirect(profileUrl);
  }

  // Redirect to home if accessing auth pages while logged in with complete profile
  if (isPublicRoute && isAuthenticated && isProfileComplete) {
    const homeUrl = new URL('/videos', request.url);
    return NextResponse.redirect(homeUrl);
  }

  return NextResponse.next();
}

export const config = {
  // Match all routes except static files and API routes
  matcher: ['/((?!api|_next/static|_next/image|favicon.ico|.*\\..*).*)'],
};
