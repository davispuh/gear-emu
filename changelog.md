Read more in Parallax forum threads:
* [Improved GEAR Emulator](http://forums.parallax.com/discussion/156347/improved-gear-emulator)
* [More GEAR - Improved Emulation of the Propeller](https://forums.parallax.com/discussion/100380/more-gear-improved-emulation-of-the-propeller)
* [GEAR does not seem to emulate SUBS correctly. (Now resolved/fixed)](http://forums.parallax.com/discussion/116940/gear-does-not-seem-to-emulate-subs-correctly-now-resolved-fixed)
* [VT100 terminal plugin for the GEAR emulator](https://forums.parallax.com/discussion/106651/vt100-terminal-plugin-for-the-gear-emulator)
* [GEAR: Propeller Debugging Environment](https://forums.parallax.com/discussion/91084/gear-propeller-debugging-environment)

## Version 20.10.01 - Added icons & images, some refresh improvements

* Icons added to almost all windows and tools, based on Visual Studio 2019 Image Library. They are in svg native format (Gear/Resources/Icons/svg & Gear/Resources/Images/svg).
* Some changes from commit #17 Prevent pathological copying of brushes, event storms on Linux (@jlunder): using double-buffer flag on Windows.Forms controls and DoubleBufferedPanel class, on every form and some controls (including PluginBase class) to prevent flickering.
* Updated `readme.md`: Gear Developers list and Third Party Components.
* Updated header of every file for Gear Developers.

## Version 20.09.02 - Enhanced UI and fixed bug #20

* Correction of bug number corrected (should be Fix #20).
* New Cummulative version released.
* Changelog updated.

## Commit fe5274f - Message on oversize binary load

* On binary load, now it is validating its size and show message error. Fix Issue #20.

## Version 20.09.01 - UI Improvemnts.

* New time units dedicated ComboBox and ToolStripComboBox. They are used in the new field "elapsed time" in Hub and Logic views (Time Frame and Tick Mark Grid fields). Their values are remembereded as program properties.
* Added tooltips to explain enhancements in Hub and Logic views.
* Added time units & number formats in hub view. You can cycle between None, System default and Parallax (separator "_"), using the mouse button.
* Added program properties editor form.
* Enhanced plugin system to v1.0, with better XML format, and the possibility to write code to a separate .cs file
* Migrated to .Net Framework 4.7.2, supporting version 7.3 of c# language.
* A lot of documentation improvements and error corrections, using better tools of VisualStudio 2019.

## Version 20.08.01

* Updated version to 20.08.01 in files and documentation, syncronizing some code changes with UI-Improvements branch.
* Updated changelog.md with corrected links to propeller forums and version changes.
* Added comment headers to some files without it, and some documentation improvements.

## Commit 5474027 - Enhanced plugin system and editor.

* Enhanced plugin format in XML files, also accepting old format. A plugin can have the c# code in a separate file (*.cs), to enable debugging into visual studio. Added validation of xml format.
* Added tab size of plugin editor to program properties.
* Enhanced comments on plugin template for new plugins, and also in shipped plugins.

## Commit 70e132c - Merge pull request #19 from davispuh/Remember-Dirs

* Remember last directories binary files and plugins loaded or edited.

## Commit 970064d - Merge pull request #18 from Memotech-Bill/master

* Corrected timing of Video Generator Frame Reload.
* Added the ability to set breaks on video frame reloads, either all or just those that do not coinside with a WAIT_VID.
* Fixed refresh when switching between tabs.

## Commit decdc0d - Remember last directories binary files and plugins loaded or edited.

* The last directories of binary and plugins loaded are remembered on main windows and plugin editor.
* Better documentation of PluginTemplate and PluginBase.
* A lot of documentation improvements and error corrections.

## Commit 961cddc - Merge pull request #16 from mbaeten/master

* Fixed Z-flag behavior of instructions SUMC and SUMN

## Commit 74c355d - Bug correction on Logic View creation

* Corrected bug on Logic View: on creation of logic view form, the last grid settings was not updated on form creation.

## Commit b41c66a - Plugin 1.0 Changes 3

* Overhead eliminated on run emulator step (calling to a default setting every time).
* Some DOXYGEN documentation on Emulator.cs

## Commit 7eb43c6 - Plugin 1.0 Changes 2

* Correction on some missing Z & C flags on NativeCogs.cs.
* A lot of DOXYGEN documentation on *.cs.
* Ordering on Using libraries on *.cs.
* Some corrections on mispellings.

## Commit 8d43068 - Plugin 1.0 Changes 1

* Improved syntax highlighting in Plugin Editor: faster processing of text, with progress bar for user feedback.
* Auto detected class name on the code, to prevent possible inconsistences on the plugin instance name of the XML with the class name on the code.
* Error grid more intelligent: shown only on errors, with more space on the grid.
* References List improved: Added name to it, improved tool tips to Add & Remove buttons.
* Some DOXYGEN documentation fixed.

## Commit d2a38e8 - Merge Correction 3

* Corrected spellings errors.

## Commit 789b325 - Merge Correction 2

* EmulationCore\PropellerCPU.cs - for PropellerCPU class changed ancestor to Propeller.DirectMemory and consequences, spelling errors. Now it compiles well.
* EmulationCore\Cog.cs - changed references of CogSpecialAddress to Assembly.RegisterAddress, and references of CogConditionCodes to Assembly.ConditionCodes, using the definitions of Propeller\AssemblyRegisters.cs & Propeller\Conditions.cs. Also deleted obsolete definitions of enum CogSpecialAddress and CogConditionCodes. Corrected spelling errors.
* Propeller\AssemblyRegisters.cs - Added comments from old code in EmulationCore\Cogs.cs, principally by adding the correction for PAR register (allowing writes) in PASM.
* GUI\SpinView.cs - Changed invocations of methods Propeller.ReadYYY() & Propeller.WriteYYY() to Propeller.DirectReadYYY() & Propeller.DirectWriteYYY(), following the changes in Propeller\MemoryManager.cs.

## Commit 2eb7f4e - Merge Correction 1

* EmulationCore\Cog.cs - Corrected some changes (#include Gear.Propeller header, memory writes & reads invocations with DirectXXYYY methods, spelling errors). Temporary correction:  commented code to memory access. TODO: clear compile errors on dependencies,
* EmulationCore\PropellerCPU.cs - Corrected memory writes & reads invocations with DirectXXYYY methods.
* EmulationCore\InterpretedCog.cs -> validated corrections in commit 230d27d
* EmulationCore\NativeCog.cs -> validated corrections in commit 230d27d

## Commit 30ca271 - Merge pull request #2 from gatuno1/Rel_Candidate2015_03

* Version 2015.03.26

## Commit bd306e9 - Final commit to Rel.Cand2015_03

* Fix the date to 15.03.26

## Commit 2d703bf - Version corrected to Rel.Cand.15_03

* Corrected version & date in About dialog and Assembly version.

## Commit 3451731 - Version reference corrected to Rel.Cand.15_03

* Correction on all version references to points to V15.03.31.
* Some spellings corrected in coments.

## Commit ca1a8cc - Bug corrections of LastPlugin application setting

* Bug Correction on first plugin loading on a fresh instalation of GEAR, related to LastPlugin application setting.
* Corrected a bug in references list: it was possible to add a blank line to the list.

## Version 15.03.26

* Corrections on all the effects for PASM hub operations (zero, carry and return): CLKSET. COGID, COGINIT, COGSTOP, LOCKNEW, LOCKRET, LOCKSET, LOCKCLR. There was some missing values for carry & zero flags.

* Algorithm optimization for PropellerCPU.PinChanged() to determinate the pin state faster for DIR and OUT.

* Corrected reset events invocations, affecting pins & lock state, and logic view. Now all of them are reset effectively.

* Improvements on LogicView, to show more helpful messages on errors, labels on buttons and text boxes.

* In Plugin Editor now you can start with a default plugin template (new default) or empty window (old default style). The program recovers it from "Resource\PluginTemplate.cs.

* Updated PluginBase class structure, so all the old plugins have to be updated:
    -Constructor invocation must call Base constructor.
    -Extra parameter on OnClock() method for current clock number in tick clocks.
    -Method PresentChip() with no param, beacuse Chip reference now is included in plugin base class definition.
    -New method OnClose() is called for every plugin before closing the emulator (to perform cleanup).

* Added program settings to remember them between sessions (stored in "Gear.exe.config" file in this version): 
    -TimeFrame and TickMarkGrid on logic view.
    -LastBinary & LastPlugin on Emulator, GearDesktop & PluginEditor.
    -UpdateEachSteps to enable changes on the screen refresh rate.
    -Added "UseNoTemplate" program setting to enable load plugin editor empty (old default style).

* Changed names and tooltips on buttons "open plugin", "load plugin", "open binary" on GearDesktop & Emulator.

* Memory leaks prevention: Corrections for Disposable members in CogView, LogicView, MemoryView & PluginEditor.

* Improved general documentation of source code, including specific pages for sequence of callings for PropellerCPU.Step() and loading a plugin in memory after compilation.


## Version 14.07.03

* Faster emulation.
    - In my own testing, GEAR runs now about 30% or 35% faster than Gear V09_10_26, maybe because the executable now use MS .NET framework 4.0 instead of 2.0. The drawback is the need to download the framework, but in windows 7 or 8, probably it is installed already.
    - Also, updated the project files to MS Visual C# 2010 Express (the old proyect was in MS Visual C# 2005). I use that because it was the only I could find.

* Show Cog RAM Special Purpose Registers values.
    - Now the values of special registers in cog memory (between $1F0 - PAR and $1FF - VSCL) are displayed correctly in memory or PASM view.

* Logic modes in counters.
    - Logic modes (CTRMODE between %10000 and %11111) didn't work well in past versions of Gear.

* Correction to enable PAR register changes.
    - As there was some reports of prop forum users that GEAR was of not capable to change the PAR cog register (forum thread ["PASM simulator / debugger?"](http://forums.parallax.com/showthread.php/115909-PASM-simulator-debugger)) then I enabled it. They claims that some parallax video drivers in PASM changes the PAR register, and GEAR didn't emulate that. The Propeller Manual V1.2 specify that is a read-only register, but if parallax did that drivers that really run on the real one...

* Fixes in Logic View.
    - When you reload a binary or reset it, and you had run it before using pins, the logic view show a mix between new and old samples.
    - If you are using windows in other languages than english, -with other number formats-, the preset values for time frame and tick mark (with "." decimal separator) will not work if you press the update button.

* Flickering correction on cog view over PASM lines.
    - When you were on cog view (PASM code), the tool tip text sometimes flickers.

* Tooltips over pins and locks in hub view.
    - Added tooltips on hub view on pins and locks views (IN*, DIR*, Floating, Lock Free, Locks), to identify easily which pin or lock is.

* Plugin editor enhancements.
    - Experimental C# sintax highlighting of code. Now the name of the file is displayed on the title of the window. Added tooltips to be more self explaining. 


## Version 09.10.26 26 October 2009

* Many thanks to Bob Anderson for identifying bugs in and subsequently testing improvements to the emulation of SUBS, SUBSX, CMPSX and REV.


## Version 09.06.05 (140) 5 June 2009

* Many thanks to Ben Levitt for improving the behaviour of GEAR. (Reload Binary, Open to PlugIn, Close PlugIn)
* Improvements in SerialIO plugin (Ben Levitt).


## Version 09.05.12 (50) 11 May 2009

* Fixed memory references of the form `LONG[ &MyVar ][ 2 ]`, this case was being emulated as `LONG[ 2 ][ &MyVar ]`.
* Included SerialIO plugin (with thanks to Ben Levitt) in the distribution.


## Version 08.10.16 (151) 16 October 2008

* An extra function has been added which allows hot keys to be disabled for a plugin.
Simply add the following function to your plugin:
`public override Boolean AllowHotKeys { get { return false; } }`


## Version 08.04.29 (159) 29 April 2008
* The lastest stimulus.zip is now included in V08_04_29.zip.
* 'R' and 'S' keys can now be used to run, stop and step the currently active cog.
* Floating the cursor over an assembly cog page shows the values stored at the source and destination registers for the instruction line that's under the mouse. The values are shown as both hex and decimal.

### On 23 January 2008

* Updated stimulus.zip. Still works with V08_01_18 of GEAR.
* The stimulus plug-in page has a context menu which allows a new stimulus file to be loaded and the current one to be saved.
* Editing of the stimulus file is allowed - previously it was read-only.
* As part of the stimulus file, it is now possible to include the words reset and stop.

reset - simulates a reset of the processor.
stop - causes a breakpoint - click Run to go to the next stop / breakpoint

### On 21 January 2008

* Added stimulus.zip which updates stimulus.xml and the example stimulus file. It is now possible to do clock and pwm generation.


## Version 08_01_18 (140)

* Pin toggling by means of a stimulus file.


## Version 08.01.13 (86)

* Better disassembly of PASM
* Some fixes in assembly language emulation
* An alternative / short instruction set for the spin bytecodes (the original is still available)
* The ability to display numbers as hex or decimal
* The ability to add a single breakpoint (just click on a line of code).
* Improved simulation of the timing from the output pinsV08_01_13 (86)
* Better disassembly of PASM
* Some fixes in assembly language emulation
* An alternative / short instruction set for the spin bytecodes (the original is still available)
* The ability to display numbers as hex or decimal
* The ability to add a single breakpoint (just click on a line of code).
* Improved simulation of the timing from the output pins


## Version 1.11.0.0

* Miscellaneous bug fixes
* Television plugin


## Version 1.10.0.0

* Fixed HUBOP instruction (source and destination fields were switched)
* Fixed PAR, now it's the 14 bit value passed, not the data stored at that address
* Fixed jitter in the analog view


## Version 1.9.0.0

* Fixed carry in addition \ subtraction operations (most of them at least)
* Changed the VGA plug-in to be sensitive on sync edges.
* First official open source release.


## Version 1.8.0.0

* Fixed MIN/MAX/MINS/MAXS instructions
* Improved and Consolidated PLL code
* General cleanups
* Fixed broadcast video emulation


## Version 1.7.0.0

* Now you can add pin ranges in the text box (12..14 is the equilivent to 12,13,14)
* Reduced stack usage for interpreted cogs
* Fixed COGINIT/COGNEW for interpreted calls
* Fixed nested function calls
* Added broadcast emulation (possibly incorrect)
* Changed the way PLLs work, allows for multiple aural hooks (since they are used now)


## Version 1.6.0.0

* Implemented proper interpreted random output (thanks Paul and chip!)
* Added analog channels to the logic view
* Reduced inital pins in logic view to 32 (0 - 31) since those are the only ones anyone should be using anyway
* Double clicking a channel in the logic view deletes it now
* Added the ability to add digital channels (incase you deleted it)
* Fixed CMPSUB, frequencys are properly calculated in TV based applications
* Increased the number of samples allowed in the logic view from 256 to 1024
* Added a good ammount of initial condition checking to the GUI (although there is something wrong if the events occur before the emulator is presented to a plugin)
* Other little fixes here and there.


## Version (1.5.0.0)

* Proper handling of condition field on native instructions
* VSCL is now buffered between WAITVID frames (VCFG is not)
* Changed behavior of Forward and Reverse random (negitive feedback LSFR, rather than rotating buffer with XOR)
* Timing fixed on instructions
* Fixed parity and other various instruction flag issues
* A cog stops rather than reboots the chip if the interpreter returns from it's starting function (stack underflow)
* Increased the size of the virtual VGA monitor plug-in (Bitmap 512x384 VGA runs at 1024x768 with pixel / line doubling)
* Included a PinNoise plugin, which plays with some pins.
* PLLs dejitter is now more accurate (uses sub-system clock accuracy)


## Version 1.4.0.0

* Fixed a HUGE problem involving inplace effected memory (interpreted)
* x /= y was being processed as x := y / x (Which is bad).. Order of operations is now appropriate
* Changed the name of BusModule to PluginBase (makes more sense to me)
* Sped up the interpreted core a little bit (doesn't use the stack for internal temporary variables)
* Cleaned up the source significantly
* Fixed a minor bug in the composite video generator phase shifting code (s-video)
* Numerous bug fixes scattered about the native and interpreted cores


## Version 1.3.0.0

* Support for Frequency Generator modes 1-3 (PLL), and VGA video generators.
* New SPIN memory viewer, but it's incomplete

