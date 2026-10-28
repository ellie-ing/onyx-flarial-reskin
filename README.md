# ONYX — a reskin for the Flarial Launcher

> **Disclaimer:** ONYX is a fan-made visual theme for the Flarial Launcher. It is **not** a standalone Minecraft client, **not** an official Flarial product, and it is not affiliated with or endorsed by the Flarial team, Mojang, or Microsoft.
>
> The launcher and the Flarial Client are created and owned by the **Flarial team and their contributors**. This repository contains only my theme and UI modifications built on top of their open-source launcher. All credit for the launcher itself, the client, and everything that makes Flarial work goes to them.

**Upstream project:** [flarialmc/Flarial.Launcher](https://github.com/flarialmc/Flarial.Launcher) — the official launcher for Flarial Client. If you want Flarial itself, get it from them: [flarial.xyz](https://flarial.xyz).

## What ONYX changes

ONYX is a black-and-white reskin of the launcher's interface:

- A monochrome ONYX home screen (animated glitch background, WELCOME block, parallelogram LAUNCH MINECRAFT button, launcher license card, launching overlay)
- An OPTIONS screen styled after the same theme (General and Versions panes)
- Custom styles for buttons, switches, radio navigation, and the segmented bar
- One small behavioural tweak: the client DLL is injected as soon as the game window appears instead of waiting for the main menu to finish loading, so the client is already in by the time the menu shows

Everything else — downloading and updating the client, launching Minecraft, accounts, settings storage — is Flarial's own code, unchanged in behaviour.

## Building

- Requires the .NET 10 SDK (Windows, with the desktop workload)
- Open `src/Flarial.Launcher/Flarial.Launcher.csproj`, and add `src/Flarial.Runtime/Flarial.Runtime.csproj` to the solution as an existing project
- Restore NuGet packages, build **Release | x64**, run

## Releases

The release zips are complete source snapshots of each version, including the binary assets (background GIF, fonts, icons) that are not stored in the git tree. Grab a release zip if you want the full, buildable source of a specific version.

Older builds are kept in the private companion archive repo (`onyx-flarial-reskin-archive`).

## License

GNU General Public License v3.0 — the same license as the upstream Flarial Launcher. The full license text is in the [LICENSE](LICENSE) file, unchanged from upstream.
