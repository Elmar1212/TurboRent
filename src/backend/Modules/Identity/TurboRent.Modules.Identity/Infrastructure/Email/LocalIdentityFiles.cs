using System.Security.AccessControl;
using System.Security.Principal;

namespace TurboRent.Modules.Identity.Infrastructure.Email;

public static class LocalIdentityFiles
{
    public static string Root
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "TurboRent.slnx")))
                directory = directory.Parent;
            if (directory is null) throw new InvalidOperationException("Local development repository root not found.");
            return Path.Combine(directory.FullName, "artifacts", "identity-local");
        }
    }

    public static void ProtectDirectory(string path, bool skipUnixPermissionChanges = false)
    {
        if (!Path.IsPathFullyQualified(path)) throw new InvalidOperationException("Local storage needs an absolute path.");
        var directory = Directory.CreateDirectory(path);
        if (OperatingSystem.IsWindows())
        {
            var security = new DirectorySecurity();
            security.SetAccessRuleProtection(true, false);
            security.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User!,
                FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None, AccessControlType.Allow));
            directory.SetAccessControl(security);
        }
        else if (!skipUnixPermissionChanges)
        {
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
    }
}
