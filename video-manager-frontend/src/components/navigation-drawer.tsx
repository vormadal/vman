'use client';

import { useState } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { Menu, Database, FolderOpen, Grid3x3, LogOut, Film, Image as ImageIcon, Tag, Users } from 'lucide-react';
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { useLogout } from '@/lib/hooks/useAuth';
import { useAuthStore } from '@/lib/store/authStore';
import { cn } from '@/lib/utils';

const navigationItems = [
  {
    title: 'Items',
    href: '/items',
    icon: Grid3x3,
    description: 'Browse all media items',
  },
  {
    title: 'Videos',
    href: '/videos',
    icon: Film,
    description: 'View video content',
  },
  {
    title: 'Images',
    href: '/images',
    icon: ImageIcon,
    description: 'View image content',
  },
  {
    title: 'Collections',
    href: '/collections',
    icon: FolderOpen,
    description: 'Manage collections',
  },
  {
    title: 'Tags',
    href: '/tags',
    icon: Tag,
    description: 'Manage tags',
  },
  {
    title: 'Sync',
    href: '/sync',
    icon: Database,
    description: 'Sync with providers',
  },
];

const adminItems = [
  {
    title: 'Users',
    href: '/admin/users',
    icon: Users,
    description: 'Manage users & invites',
  },
];

export function NavigationDrawer() {
  const [open, setOpen] = useState(false);
  const pathname = usePathname();
  const logout = useLogout();
  const isAdmin = useAuthStore((state) => state.isAdmin());

  const handleLogout = () => {
    setOpen(false);
    logout();
  };

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <Button variant="ghost" size="icon" className="md:mr-2">
          <Menu className="h-5 w-5" />
          <span className="sr-only">Toggle navigation menu</span>
        </Button>
      </SheetTrigger>
      <SheetContent side="left" className="w-[280px] sm:w-[320px]">
        <SheetHeader>
          <SheetTitle>Navigation</SheetTitle>
        </SheetHeader>
        <nav className="flex flex-col gap-2 mt-6">
          {navigationItems.map((item) => {
            const Icon = item.icon;
            const isActive = pathname === item.href;
            
            return (
              <Link
                key={item.href}
                href={item.href}
                onClick={() => setOpen(false)}
                className={cn(
                  'flex items-center gap-3 rounded-lg px-3 py-2 text-sm transition-colors',
                  isActive
                    ? 'bg-primary text-primary-foreground'
                    : 'hover:bg-accent hover:text-accent-foreground'
                )}
              >
                <Icon className="h-5 w-5" />
                <div className="flex flex-col">
                  <span className="font-medium">{item.title}</span>
                  <span className={cn(
                    'text-xs',
                    isActive ? 'text-primary-foreground/80' : 'text-muted-foreground'
                  )}>
                    {item.description}
                  </span>
                </div>
              </Link>
            );
          })}
          
          {isAdmin && (
            <>
              <div className="my-4 border-t" />
              <div className="px-3 py-2">
                <p className="text-xs font-semibold text-muted-foreground uppercase">Admin</p>
              </div>
              {adminItems.map((item) => {
                const Icon = item.icon;
                const isActive = pathname === item.href;
                
                return (
                  <Link
                    key={item.href}
                    href={item.href}
                    onClick={() => setOpen(false)}
                    className={cn(
                      'flex items-center gap-3 rounded-lg px-3 py-2 text-sm transition-colors',
                      isActive
                        ? 'bg-primary text-primary-foreground'
                        : 'hover:bg-accent hover:text-accent-foreground'
                    )}
                  >
                    <Icon className="h-5 w-5" />
                    <div className="flex flex-col">
                      <span className="font-medium">{item.title}</span>
                      <span className={cn(
                        'text-xs',
                        isActive ? 'text-primary-foreground/80' : 'text-muted-foreground'
                      )}>
                        {item.description}
                      </span>
                    </div>
                  </Link>
                );
              })}
            </>
          )}
          
          <div className="my-4 border-t" />
          
          <Button
            variant="ghost"
            className="justify-start gap-3 px-3 py-2 h-auto text-sm hover:bg-destructive/10 hover:text-destructive"
            onClick={handleLogout}
          >
            <LogOut className="h-5 w-5" />
            <div className="flex flex-col items-start">
              <span className="font-medium">Logout</span>
              <span className="text-xs text-muted-foreground">Sign out of your account</span>
            </div>
          </Button>
        </nav>
      </SheetContent>
    </Sheet>
  );
}
