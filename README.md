# PowerToys Run: Browser Favorite plugin

This Repository is Forked from davidegiacometti/PowerToys-Run-EdgeFavorite for Brave.

It extends the functionality to allow for searching empty bookmark names and supports a list of browsers:
- Brave
- Edge
- Chrome
- FireFox

## Installation

- Download the [latest release](https://github.com/Der-Penz/PowerToys-Run-BrowserFavorite/releases/) by selecting the
  architecture that matches your machine: `x64` (more common) or `ARM64`
- Close PowerToys
- Extract the archive to `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins` or system-wide
  `%ProgramFiles%\PowerToys\RunPlugins`
- Open PowerToys

## Usage
1. Open PowerToys Run (default shortcut is Alt+Space).
2. Type @ followed by the bookmark your searching.
3. Select a bookmark and it will open in your selected browser

## Screenshots

![Search](./images/Search.png)

![Plugin Manager](./images/PluginManager.png)

## Localization

There is no localization right now, but the plugin has relatively limited user-facing strings.

## Future Plans
I don't have any plans to add new features to this plugin, there might be some bug fixes in the future or a new browser added.
Feel free to open an issue or a pull request if you have any ideas or want to contribute.

## Build

```shell
    powershell -ExecutionPolicy Bypass .\Build.ps1
```

For developing you can run the underlying powershell script to build the project and automatically copy the project to
your power toys run plugins

```shell
    powershell -ExecutionPolicy Bypass  .\Dev-Build.ps1
```

> [!NOTE]  
> Script has to run with admin privileges 

> [!TIP]  
> If you haven't installed powershell system-wide you might need to tweak some paths in `Dev-Build.ps1` to fit your environment

Logs are saved in `C:\Users\<UserName>\AppData\Local\Microsoft\PowerToys\PowerToys Run\Logs\<PowerToys Version>\<CurrentDate>.txt`  
If you build multiple versions and update the extension with the `Dev-Build.ps1` script it sometimes happens that settings are mixed up and the 
extension won't work properly anymore. To fix this, remove the plugin entry in the `settings.json` located at `C:\Users\<UserName>\AppData\Local\Microsoft\PowerToys\PowerToys Run` and restart PowerToys. 

## Contribution

Feel free to contribute to the project. Pull requests are welcome.  
Together we can build the perfect plugin

## Attribution

This repository is a fork of [octop162/PowerToys-Run-ChromeFavorite](https://github.com/octop162/PowerToys-Run-ChromeFavorite), which in turn is based on the original [davidegiacometti/PowerToys-Run-EdgeFavorite](https://github.com/davidegiacometti/PowerToys-Run-EdgeFavorite).

A big thank you to both octop162 and davidegiacometti for their foundational work and contributions to the PowerToys community! 🙏