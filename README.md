# One-Click Installation Tool
A simple, yet fairly robust tool to install multiple programs and execute powershell/command prompt scripts for Windows 10 and newer machines. Can be fully automated to a decent degree, with more features intended in the near future.

## How to Use
From my personal experience using the tool while developing, I found that the easiest way to use this was to turn off all UAC controls and run the program. The program includes a built in registry script to reset the UAC controls to the default regardless of the prior state of the registry entries, which will be applied on restart. From here, the program will search for all .lnk shortcut files *in the current directory* and display them to the user, where they can then be individually selected to be installed. A refresh list button is built in in case it is ever needed, though the program will always refresh on launch.

Multiple options are available to the user from here, whether it be simply selecting installers to install, typing in a new name for the machine, or running a separate .bat/.ps1 file. The first two will only execute upon clicking "install", while the last one will execute upon selecting a file. The program will also generate a cfg folder and .cfg files for every shortcut it finds. CFG files will be explained in depth later in the README.

## Recommended Windows Settings

As a baseline, if the intent is to fully automate the process, setting UAC levels to the minimum is recommended. UAC levels will be reset after installation is complete, taking effect after a restart. Alongside this, due to the program's reliance on OCR for any .cfg automation, I recommend increasing the scaling on any displays with a resolution below Full HD (1920x1080) to at least 125%. If you are having trouble with getting your .cfg file to execute properly, try increasing the scaling more.
