#### Note: !! Tested on Windows 10 with a MCHOSE Jet 75 Keyboard with firmware v111 and v113. Use at your own risk, I'm not responsible for what happens to your keyboard.
#### Note: !! You can find the portable exe in Releases.
This program allows for easy switching between profiles for MCHOSE keyboards. Currently it supports importing profiles that were exported from the MCHOSE webdriver.
Minimizing the program puts it in system tray. Exiting the program closes it. The startup shortcut, created by checking Start on windows startup, points to the location of the exe so if the exe is moved you will need to unselect and select the start on windows startup again.

This App allows you to switch between the profiles in the following ways:
- Right click the keyboard icon in the system tray and select one of the profiles there. The profile with the check icon is the currently active one. Hover the keyboard icon in the tray to show the currently active one.
- Use Ctrl + Alt + Enter to cycle through the profiles that are marked as quick switch.
- Select processes that will trigger a profile change, per profile. When 1 of these processes takes the foreground in windows the app will switch to the first associated profile.

### Info
Tested in the following environment:
- Windows 10 version 22H2
- MCHOSE Jet 75
- Keyboard firmware v111 and v113
- Windows user account has admin rights

As I understood it the keyboard does not have internal storage to store more profiles other than the active one.
Therefore this App writes the default profile whenever it is started and starts tracking the active profile from there on.
If you change the active profile through the web driver this App will not know about it.

### Screenshots
**Main window**\
<img width="786" height="443" alt="image" src="https://github.com/user-attachments/assets/6445ebe1-a042-41e3-9dcc-0024246b847f" />

**Process selection**\
<img width="466" height="353" alt="image" src="https://github.com/user-attachments/assets/2176b9db-eee7-4fe1-bbb2-9e4ba5750256" />

**Tray menu**\
<img width="203" height="155" alt="image" src="https://github.com/user-attachments/assets/8b42d8e8-efee-4ac0-a9e9-dd568032a685" />
