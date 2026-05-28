'use client';

import { useEffect, useState } from 'react';
import { useAuthStore } from '@/lib/store/authStore';
import { useRouter } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useToast } from '@/hooks/use-toast';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { Copy, Plus } from 'lucide-react';
import {
  useAdminUsers,
  useAdminInvites,
  useBlockUser,
  useChangeUserRole,
  useCreateInvite,
} from '@/lib/hooks/useAdmin';

export default function AdminUsersPage() {
  const router = useRouter();
  const { toast } = useToast();
  const { isAdmin } = useAuthStore();
  const [email, setEmail] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);

  const { data: usersData, isLoading: usersLoading } = useAdminUsers();
  const { data: invitesData, isLoading: invitesLoading } = useAdminInvites();
  const blockUser = useBlockUser();
  const changeUserRole = useChangeUserRole();
  const createInvite = useCreateInvite();

  useEffect(() => {
    if (!isAdmin()) {
      router.push('/videos');
    }
  }, [isAdmin, router]);

  const handleBlockUser = (userId: string, block: boolean) => {
    blockUser.mutate(
      { userId, block },
      {
        onSuccess: () => toast.success(`User ${block ? 'blocked' : 'unblocked'} successfully`),
        onError: (error) =>
          toast.error(error instanceof Error ? error.message : 'Operation failed'),
      }
    );
  };

  const handleChangeRole = (userId: string, role: string) => {
    changeUserRole.mutate(
      { userId, role },
      {
        onSuccess: () => toast.success('User role updated successfully'),
        onError: (error) =>
          toast.error(error instanceof Error ? error.message : 'Operation failed'),
      }
    );
  };

  const handleCreateInvite = () => {
    createInvite.mutate(email, {
      onSuccess: (result) => {
        const inviteLink = `${window.location.origin}${result.inviteUrl}`;
        navigator.clipboard.writeText(inviteLink);
        toast.success('Invite created!', { description: 'Invite link copied to clipboard' });
        setEmail('');
        setDialogOpen(false);
      },
      onError: (error) =>
        toast.error(error instanceof Error ? error.message : 'Failed to create invite'),
    });
  };

  const copyInviteLink = async (token: string) => {
    const inviteUrl = `${window.location.origin}/accept-invite?token=${token}`;
    await navigator.clipboard.writeText(inviteUrl);
    toast.success('Copied!', { description: 'Invite link copied to clipboard' });
  };

  if (!isAdmin()) return null;

  if (usersLoading || invitesLoading) {
    return <div>Loading...</div>;
  }

  const users = usersData?.users ?? [];
  const invites = invitesData?.invites ?? [];

  return (
    <div className="container mx-auto py-8 space-y-8">
      <Card>
        <CardHeader>
          <CardTitle>User Management</CardTitle>
          <CardDescription>Manage user accounts, roles, and access</CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Email</TableHead>
                <TableHead>Name</TableHead>
                <TableHead>Role</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Created</TableHead>
                <TableHead>Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {users.map((user) => (
                <TableRow key={user.id}>
                  <TableCell>{user.email}</TableCell>
                  <TableCell>
                    {user.firstName && user.lastName
                      ? `${user.firstName} ${user.lastName}`
                      : !user.isProfileComplete
                      ? <Badge variant="secondary">Profile Incomplete</Badge>
                      : '-'}
                  </TableCell>
                  <TableCell>
                    <Select
                      value={user.role}
                      onValueChange={(value) => handleChangeRole(user.id, value)}
                    >
                      <SelectTrigger className="w-32">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="User">User</SelectItem>
                        <SelectItem value="Admin">Admin</SelectItem>
                      </SelectContent>
                    </Select>
                  </TableCell>
                  <TableCell>
                    {user.isBlocked ? (
                      <Badge variant="destructive">Blocked</Badge>
                    ) : (
                      <Badge variant="default">Active</Badge>
                    )}
                  </TableCell>
                  <TableCell>
                    {new Date(user.createdAt).toLocaleDateString('da-DK', {
                      day: '2-digit',
                      month: '2-digit',
                      year: 'numeric',
                    })}
                  </TableCell>
                  <TableCell>
                    <Button
                      variant={user.isBlocked ? 'default' : 'destructive'}
                      size="sm"
                      onClick={() => handleBlockUser(user.id, !user.isBlocked)}
                    >
                      {user.isBlocked ? 'Unblock' : 'Block'}
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Invitations</CardTitle>
              <CardDescription>Create and manage user invitations</CardDescription>
            </div>
            <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
              <DialogTrigger asChild>
                <Button>
                  <Plus className="mr-2 h-4 w-4" />
                  Create Invite
                </Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Create Invite</DialogTitle>
                  <DialogDescription>
                    Enter the email address for the new user. An invite link will be generated.
                  </DialogDescription>
                </DialogHeader>
                <div className="space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="email">Email</Label>
                    <Input
                      id="email"
                      type="email"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      placeholder="user@example.com"
                    />
                  </div>
                </div>
                <DialogFooter>
                  <Button onClick={handleCreateInvite} disabled={!email || createInvite.isPending}>
                    {createInvite.isPending ? 'Creating...' : 'Create & Copy Link'}
                  </Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>
          </div>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Email</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Created</TableHead>
                <TableHead>Expires</TableHead>
                <TableHead>Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {invites.map((invite) => (
                <TableRow key={invite.id}>
                  <TableCell>{invite.email}</TableCell>
                  <TableCell>
                    {invite.isUsed ? (
                      <Badge variant="secondary">Used</Badge>
                    ) : invite.isExpired ? (
                      <Badge variant="destructive">Expired</Badge>
                    ) : (
                      <Badge variant="default">Active</Badge>
                    )}
                  </TableCell>
                  <TableCell>
                    {new Date(invite.createdAt).toLocaleDateString('da-DK', {
                      day: '2-digit',
                      month: '2-digit',
                      year: 'numeric',
                    })}
                  </TableCell>
                  <TableCell>
                    {new Date(invite.expiresAt).toLocaleDateString('da-DK', {
                      day: '2-digit',
                      month: '2-digit',
                      year: 'numeric',
                    })}
                  </TableCell>
                  <TableCell>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => copyInviteLink(invite.token)}
                      disabled={invite.isUsed || invite.isExpired}
                    >
                      <Copy className="mr-2 h-4 w-4" />
                      Copy Link
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </CardContent>
      </Card>
    </div>
  );
}
