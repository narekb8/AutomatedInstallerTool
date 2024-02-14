# One-Click Installation Tool

A simple, yet fairly robust tool to install multiple programs and execute powershell/command prompt scripts for Windows 10 and newer machines. Can be fully automated to a decent degree, with more features intended in the near future.

- [Download](https://github.com/narekb8/AutomatedInstallerTool/releases/latest)
- [How to Use](#how-to-use)
- [Recommended Settings](#recommended-windows-settings)
- [cfg Files](#cfg-files)
- [Issues and Upcoming Features](#issues-and-potential-upcoming-features)

## How to Use

After downloading and extracting the folder to your desired location, create a shortcut to every program which you would like to be seen by the installer, and copy them into the same folder the .exe and all other files are located in.

From my personal experience using the tool while developing, I found that the easiest way to use this was to turn off all UAC controls and run the program. The program includes a built in registry script to reset the UAC controls to the default regardless of the prior state of the registry entries, which will be applied on restart. From here, the program will search for all .lnk shortcut files *in the current directory* and display them to the user, where they can then be individually selected to be installed. A refresh list button is built in in case it is ever needed, though the program will always refresh on launch.

Multiple options are available to the user from here, whether it be simply selecting installers to install, typing in a new name for the machine, or running a separate .bat/.ps1 file. The first two will only execute upon clicking "install", while the last one will execute upon selecting a file. The program will also generate a cfg folder and .cfg files for every shortcut it finds. CFG files will be explained in depth later in the README.

## Recommended Windows Settings

As a baseline, if the intent is to fully automate the process, setting UAC levels to the minimum is recommended. UAC levels will be reset after installation is complete, taking effect after a restart. Alongside this, due to the program's reliance on OCR for any .cfg automation, I recommend increasing the scaling on any displays with a resolution below Full HD (1920x1080) to at least 125% if a .cfg file is being used. If you are having trouble with getting your .cfg file to execute properly, try increasing the scaling more.

## cfg Files

In order to allow for more automation methods, this tool uses Windows' built-in OCR program to allow for step by step instructions without any hands on input. After running the program once, empty .cfg files will be generated in a separate cfg folder. Writing them is incredibly easy; for each page of the installer, write a single line into the .cfg file with the text of the button to search for and click on.

## Issues and Potential Upcoming Features

At the moment, the program has multiple cases where it may crash in bad search instances. Proper edge case exception handling and user feedback in these instances is next to come ASAP. 
Afterwards, I'd like to implement the ability to write text as well (ex: typing a new install path for a program)
