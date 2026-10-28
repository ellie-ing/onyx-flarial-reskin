using System.Diagnostics;
using System.IO;
using Flarial.Runtime.Exceptions;
using Flarial.Runtime.Unmanaged;
using Windows.ApplicationModel;
using Windows.Win32.Foundation;
using static Windows.Win32.Foundation.WAIT_EVENT;
using static Windows.Win32.PInvoke;
using static Windows.Win32.System.Threading.PROCESS_ACCESS_RIGHTS;

namespace Flarial.Runtime.Game;

unsafe partial class Minecraft
{
    static readonly string s_path, s_filename;

    static uint? Activate()
    {
        if (IsInstalled)
        {
            if (GetProcessId() is { } processId)
                return processId;

            var path = Path.Combine(Package.InstalledPath, ProcessName);
            if (!File.Exists(path)) throw new MinecraftNotFoundException();

            using var process = Process.Start(new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = s_filename,
                ArgumentList = { "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-Command", string.Format(Command, path) }
            });

            process?.WaitForExit();
            return GetProcessId();
        }
        return null;
    }

    internal static uint? Launch()
    {
        if (!GamingServices.IsInstalled)
            throw new GamingServicesNotInstalledException();

        if (GetWindow() is { IsVisible: true } minecraftWindow)
        {
            minecraftWindow.SetActive();
            return minecraftWindow._processId;
        }

        if (Activate() is not { } processId)
            return null;

        if (PROCESS_SYNCHRONIZE.Open(processId) is not { } process)
            return null;

        using (process)
        {

            /*
                - TEST BUILD (early injection): proceed as soon as the game's
                  window is visible, instead of waiting for the main menu to
                  finish loading (upstream waited for the game's
                  *menu_load_lock file to be deleted first). The client gets
                  loaded while the game is still on its loading screen, so
                  the menu comes up with the client already in.
                - The sideloaded path always worked this way; this extends
                  the same timing to regular installs.
            */

            while (process.Wait(1))
                if (GetWindow(processId) is { IsVisible: true })
                    return processId;
            return null;
        }
    }
}