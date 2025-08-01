#### Note: !! Tested on Windows 10 with a MCHOSE Jet 75 II Keyboard with firmware v113. Use at your own risk, I'm not responsible for what happens to your keyboard.
### ⌨️ Unofficial MCHOSE Profile Switcher
This program allows for easy switching between profiles for MCHOSE keyboards. Currently it supports importing profiles that were exported from the MCHOSE webdriver.
Minimizing the program puts it in system tray. Exiting the program closes it. The startup shortcut, created by checking Start on windows startup, points to the location of the exe so if the exe is moved you will need to unselect and select the start on windows startup again.

The app does not require installation not admin rights and uses %LocalAppData%/MCHOSE Profile Switcher to store profiles and settings.
You can build the app from this repository using dotnet/msbuild or get a binary from the release section.

This App allows you to switch between the profiles in the following ways:
- Right click the keyboard icon in the system tray and select one of the profiles there. The profile with the checkmark icon is the currently active one. Hover the keyboard icon in the tray to show the currently active one.
- Use Ctrl + Alt + Enter to cycle through the profiles that are marked as quick switch.
- Select processes that will trigger a profile change, per profile. When 1 of these processes takes the foreground in windows the app will switch to the first associated profile.

### Info
Tested in the following environment:
- Windows 10 version 22H2
- MCHOSE Jet 75 II
- Keyboard firmware v113
- Web driver v0.09 (exported profiles from the webdriver to import with this app)
- Windows user account has admin rights

As I understood it the keyboard does not have internal storage to store more profiles other than the active one.
Therefore this App writes the default profile whenever it is started and starts tracking the active profile from there on.
If you change the active profile through the web driver this App will not know about it.

**If you know why windows defender gives a false positive for this App feel free to open an Issue for it.**

### Screenshots
**Main window**\
![Main window](https://github.com/user-attachments/assets/b6ce5f99-00f7-4240-bdfe-b76ccb0e47c9)

**Process selection**\
![Process selection](https://github.com/user-attachments/assets/4f3de382-2a2b-4385-923c-17ba74d22123)

**Tray menu**\
![Tray menu](https://github.com/user-attachments/assets/41a9ed35-6820-4b27-a627-cb9c88b0d83d)

I made a similar app for the Dunk deer keyboards. You can find it over at https://github.com/l2-/DrunkDeerDriver
