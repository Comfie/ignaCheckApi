/**
 * User roles in the system
 *
 * SuperAdmin - System-wide administrator, manages entire platform, onboards customers/workspaces
 * Owner - Workspace owner/admin, manages users within their workspace
 * Admin - Workspace administrator with elevated permissions
 * Contributor - Can create, edit, and manage projects within workspace
 * Viewer - Read-only access to workspace data
 */
export enum UserRole {
  SuperAdmin = 'SuperAdmin',
  Owner = 'Owner',
  Admin = 'Admin',
  Contributor = 'Contributor',
  Viewer = 'Viewer'
}
