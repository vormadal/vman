import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

// Public routes that don't require authentication
const PUBLIC_ROUTES = ['/login', '/register'];

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // Check if route is public
  const isPublicRoute = PUBLIC_ROUTES.some(route => pathname.startsWith(route));

  // Get auth token from localStorage (stored as JSON by zustand persist)
  // Since middleware runs on edge, we need to check via cookie or header
  // Let's use a cookie approach
  const authCookie = request.cookies.get('auth-storage');
  
  let isAuthenticated = false;
  if (authCookie) {
    try {
      const authData = JSON.parse(authCookie.value);
      isAuthenticated = !!authData?.state?.accessToken;
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

  // Redirect to home if accessing auth pages while logged in
  if (isPublicRoute && isAuthenticated) {
    return NextResponse.redirect(new URL('/videos', request.url));
  }

  return NextResponse.next();
}

export const config = {
  // Match all routes except static files and API routes
  matcher: ['/((?!api|_next/static|_next/image|favicon.ico|.*\\..*).*)'],
};
