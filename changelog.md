## Read more in Parallax forum threads:
* [Improved GEAR Emulator](http://forums.parallax.com/discussion/156347/improved-gear-emulator) [2014-07-03 ]
  * [Gear plugin confusion - plugin scripts and Gear are both C# !](https://forums.parallax.com/discussion/160898/gear-plugin-confusion-plugin-scripts-and-gear-are-both-c) [2015-04-26]
  * [Building a plugin to emulate audio in GEAR ](https://forums.parallax.com/discussion/comment/1327930/#Comment_1327930) [2015-04-26]
* [More GEAR - Improved Emulation of the Propeller](https://forums.parallax.com/discussion/100380/more-gear-improved-emulation-of-the-propeller) [2008-01-13]
  * [GEAR does not seem to emulate SUBS correctly. (Now resolved/fixed)](http://forums.parallax.com/discussion/116940/gear-does-not-seem-to-emulate-subs-correctly-now-resolved-fixed)
  * [VT100 terminal plugin for the GEAR emulator](https://forums.parallax.com/discussion/106651/vt100-terminal-plugin-for-the-gear-emulator) [2008-09-17]
* [GEAR: Propeller Debugging Environment](https://forums.parallax.com/discussion/91084/gear-propeller-debugging-environment) [2007-01-10]

# Change Log

## Bugfix in LogicView, New icon for program and Code cleanup 2
* Fixed bug in [`LogicView.cs`](Gear/GUI/LogicView.cs): no detecting modification of timeFrame or tickMark.
* New Icon for Gear program.
* Removed extra spaces in line endings on all files.
* Changed tag @since to @version in all documentation.


## Commit [dd92658](https://github.com/davispuh/gear-emu/commit/dd92658c7309b0872ab4a3cd41cd39bbd45ca70f) - Corrected initial directory to open plug-ins and code cleanup 1
* Corrected initial directory to open plug-ins in programs settings.
* Corrected files of the project to have coherent line endings (CRLF), and coded in UTF-8.
* Added generation of manifiest.


## Release Version [22.03.02](https://github.com/davispuh/gear-emu/releases/tag/v22.03.02) and commit [eec325f](https://github.com/davispuh/gear-emu/commit/eec325f198981b18e74c7ada62a28cc1322fdf92) - Various bugfixes.
* Fixed bug in `AssemblyInfo.cs`: missing +.
* Fixed bug corrected `LONG` registers not assigned correctly in Cog - `DIR`, `OUT` on [`Cog.cs`](Gear/EmulationCore/Cog.cs).
* Fixed bug on `[LogicAnalog](Gear/GUI/LogicProbe/LogicAnalog.cs).Draw()`: was comparing wrong time to activate.
* Fixed bug in maximum frequency of Cog Counters, to 128Mhz instead of wrong limit of 120Mhz, on [`PLL.cs`](gear/EmulationCore/PLL.cs).
* Fixed bug on [`LogicView.cs`](Gear/GUI/LogicProbe/LogicView.cs): save TimeUnitSelected with the other properties in logic view.
* Fixed bug of Form name not updated when open another binary file in [`Emulator.cs`](Gear/GUI/Emulator.cs).
* Fixed hidden vertical scroll bar on Logic View: [`LogicView.Designer.cs`](Gear/GUI/LogicProbe/LogicView.Designer.cs).
* Fixed bug of not remembering last binary open from Settings in [`Emulator.cs`](Gear/GUI/Emulator.cs) and [`GearDesktop`](Gear/GUI/GearDesktop.cs).
* Method [`PluginEditor.cs`](Gear/GUI/PluginEditor.cs).OpenButton_Click(.) refactored on using Open File Dialog.
* Updated GEAR Developers.


## Release Version [22.03.01](https://github.com/davispuh/gear-emu/releases/tag/v22.03.01) and commit [bf3cee3](https://github.com/davispuh/gear-emu/commit/bf3cee379990d150e0a27f2bcc4d2035e31008da) - Improved accuracy of Video generator and improvements in code.
* Updated version to 22.03.01, and files header copyright updated to 2007-2022.
* Updated [readme.md](readme.md) with description section, link to new Plugin notes.
* Updated [changelog.md](changelog.md) to fix dead links of Propeller threads. Added links to github commits, pull request and issues.
* Added [Plugins_notes.md](plug-ins/Plugins_notes.md) file : Details of deployed plugins, and Plugin API.
* Separated big file `NativeCog.cs` in two files: [`NativeCog.cs`](Gear/EmulationCore/NativeCog.cs) with the main definitions, and [`NativeCogInstructions.cs`](Gear/EmulationCore/NativeCogInstructions.cs) with the Instructions methods, for clarity and better mantainability.
* Created [`BinarySizeException.cs`](Gear/Utils/BinarySizeException.cs) file to contains the class `BinarySizeException`, extracted from `PropellerCPU.cs`.
* Created [`SingleInstanceException.cs`](Gear/Utils/SingleInstanceException.cs) file to contains the class `SingleInstanceException`, extracted from `GearDesktop.cs`.
* Changed VisualStudio analizer: from legacy FxCops to NETAnalyzers in Project file (Articles ["Migrate from FxCop analyzers to .NET analyzers"](https://docs.microsoft.com/en-us/visualstudio/code-quality/migrate-from-fxcop-analyzers-to-net-analyzers?view=vs-2019) and ["Install .NET analyzers" ](https://docs.microsoft.com/en-us/visualstudio/code-quality/install-net-analyzers?view=vs-2019).
* Deleted obsolete file: `Gear\EmulationCore\DisassemblerStrings.cs`.


## [Merge pull request #[29](https://github.com/davispuh/gear-emu/pull/29) from Sh1nyAnd3o3/vidgen_ordering[1c94a33](https://github.com/davispuh/gear-emu/commit/1c94a337881efa4fadef7cdc8cf2aeec30b0632d), and Commits [cfcccf4c](https://github.com/davispuh/gear-emu/commit/cfcccf4c70799645f47531117cdefc7a17ba8707), [9a386ad](https://github.com/davispuh/gear-emu/commit/9a386ad1aec027fde76de157e1d86f20b0a5481c) and [e0073bf](https://github.com/davispuh/gear-emu/commit/e0073bff348594a73951e542dc8dcce1760970d7).
* Changed VideoGenerator Video Generation Procedure to fit Verilog Code (from @github/Sh1nyAnd3o3).


## Pull request #[26](https://github.com/davispuh/gear-emu/pull/26) & commits [8d15f6b](https://github.com/davispuh/gear-emu/commit/8d15f6b7d6b77bf19028739faff331d46ea1e82a), [1f71fdb](https://github.com/davispuh/gear-emu/commit/1f71fdb318761610a69d3bbff29001fa4acbffd3) - Added carry and zero flag changes to `WAITVID` instruction
* Pull request #[26](https://github.com/davispuh/gear-emu/pull/26) by @github/Sh1nyAnd3o3 : the carry and zero flag of an `WAITVID` instruction are set like the opcode of the `WAITVID` command tells.


## Comit [94c3c2c](https://github.com/davispuh/gear-emu/commit/94c3c2c20bacaa498519d69c02b2fc8a04792e0b) - Updated http address for documentation links
* Broken links fixed: parallax forums fixed.


## Release Version [21.06.01](https://github.com/davispuh/gear-emu/releases/tag/v21.06.01) & Commit [c647059](https://github.com/davispuh/gear-emu/commit/c647059be124692e40ee51066085e997e9e43072)- Cummulative release: bugfixes #[23](https://github.com/davispuh/gear-emu/issues/23) #[24](https://github.com/davispuh/gear-emu/issues/24) and bytecode documentation
* Cummulative release for bugfixes #[23](https://github.com/davispuh/gear-emu/issues/23) & #[24](https://github.com/davispuh/gear-emu/issues/24) and bytecode documentation.
* Include missing example file for [`Stimulus`](plug-ins/Stimulus.xml) plugin: [`test.stm`](plug-ins/test.stm).


## Commits [9979938](https://github.com/davispuh/gear-emu/commit/9979938787faa6628b99dfeaa9f19ab0ada5147c), [c0fb5ae](https://github.com/davispuh/gear-emu/commit/c0fb5aea9a93505f0312575112058a8c499cbc84), [90271bd](https://github.com/davispuh/gear-emu/commit/90271bd8d456a10085d0568de6f5994e8f247fad) - Ammends to correct issue Wuerfel21#[24](https://github.com/davispuh/gear-emu/issues/24) & better documentation
* Refactored code to correct issue @github/Wuerfel21#[24](https://github.com/davispuh/gear-emu/issues/24).
* Added documentation to bytecode management, using the notation from [Cluso99's SPIN bytecode document](https://forums.parallax.com/discussion/comment/796018/#Comment_796018) on Propeller 1 Forum.
* More documentation on [InstructionDisassembler.cs](Gear/EmulationCore/InstructionDisassembler.cs) file.


## Pull request #[25](https://github.com/davispuh/gear-emu/pull/25) & commits [5eff3e6](https://github.com/davispuh/gear-emu/commit/5eff3e662ee1a773879652b14ab192667aa79089), [0935b40](https://github.com/davispuh/gear-emu/commit/0935b408ec5a62af5d75e5c38bc0c0a9cca31973) - Fix reversed assignment ops
* Commit [0935b40](https://github.com/davispuh/gear-emu/commit/0935b408ec5a62af5d75e5c38bc0c0a9cca31973) by @github/Wuerfel21 : Fix reversed assignment ops, base solution.
* Commit [5eff3e6](https://github.com/davispuh/gear-emu/commit/5eff3e662ee1a773879652b14ab192667aa79089) : Complement changes of the other commit, changing the PAMS instruction code on dissasembler window.


## Commit [2845f4c](https://github.com/davispuh/gear-emu/commit/2845f4c1b95896b2a83db95178ff6c5d924b2361) - Bugfix @github/Wuerfel21#[23](https://github.com/davispuh/gear-emu/issues/23): implement opcode `0x37` with `ROL` pasm instruction
* Bugfix issue @github/Wuerfel21#[23](https://github.com/davispuh/gear-emu/issues/23): implement opcode `0x37` (Push Packed Literal) with `ROL` pasm instruction and not `SHL`, on SPIN interpreter.
* Added some documentation placeholders and remove some `@todo Documents`.
* Corrected a propeller 1 forum link, to new web structure.


## Release Version [20.10.01](https://github.com/davispuh/gear-emu/releases/tag/v20.10.01) & Commit [c76aa43](https://github.com/davispuh/gear-emu/commit/c76aa434168acb0eb32a884c9870deefa483898d) - Added icons & images, some refresh improvements

* Icons added to almost all windows and tools, based on Visual Studio 2019 Image Library. They are in svg native format ([Gear/Resources/Icons/svg](Gear/Resources/Icons/svg) & [Gear/Resources/Images/svg](Gear/Resources/Images/svg) directories).
* Some changes from pull request #[17 Prevent pathological copying of brushes, event storms on Linux](https://github.com/davispuh/gear-emu/pull/17) (@github/jlunder): using double-buffer flag on `Windows.Forms` controls and `DoubleBufferedPanel` class, on every form and some controls (including `PluginBase` class) to prevent flickering.
* Updated [readme.md](readme.md): Gear Developers list and Third Party Components.
* Updated header of every file for Gear Developers.


## Commit [de00b07](https://github.com/davispuh/gear-emu/commit/de00b0763deccf2c336a998d8f380ed2212a729a) - Use Memory constants

* Use Memory constants instead of constants like `0xFFFF`.


## Version [20.09.02](https://github.com/davispuh/gear-emu/releases/tag/v20.09.02) & Commit [d517820](https://github.com/davispuh/gear-emu/commit/d517820435cb3a71aabe3b2e04f06d60fcb07c3e) - Enhanced UI and fixed bug #[20](https://github.com/davispuh/gear-emu/issues/20)

* Correction of bug number corrected (should be Fix #[20](https://github.com/davispuh/gear-emu/issues/20)).
* New Cummulative version 20.09.01 - UI Improvemnts.
* Changelog updated.


    ## Commit [fe5274f](https://github.com/davispuh/gear-emu/commit/fe5274fc31b432b31a405c44d4ce74693bc4d500) - Message on oversize binary load

    * On binary load, now it is validating its size and show message error. Fix Issue #[20](https://github.com/davispuh/gear-emu/issues/20).


    ## Version [20.09.01](https://github.com/davispuh/gear-emu/commit/9c76ed5be7898158032f78e877ff7eb3695877d9) - UI Improvements.

    * New time units dedicated `ComboBox` and `ToolStripComboBox`. They are used in the new field "elapsed time" in Hub and Logic views (Time Frame and Tick Mark Grid fields). Their values are remembereded as program properties.
    * Added tooltips to explain enhancements in Hub and Logic views.
    * Added time units & number formats in hub view. You can cycle between None, System default and Parallax (separator "_"), using the mouse button.
    * Added program properties editor form.
    * Enhanced plugin system to v1.0, with better XML format, and the possibility to write code to a separate `.cs` file
    * Migrated to .Net Framework 4.7.2, supporting version 7.3 of c# language.
    * A lot of documentation improvements and error corrections, using better tools of VisualStudio 2019.


## Commit [da92124](https://github.com/davispuh/gear-emu/commit/da92124b063c91f2c888b3581420b7687203f38c) - UI Improvements - time units & number formats in hub view
* Added time units & number formats in hub view. New time units dedicated combobox.
* Added program properties editor windows.
* A lot of documentation improvements and error corrections.


## Commit [ba539b3](https://github.com/davispuh/gear-emu/commit/ba539b37bf1ee9a0c27dc21d89dd69aefac74757) -  Format selection for Counter, core and xtal frequency in Hub view.
* HubView Improvement: You can change the system counter, core and xtal frequency between 3 formats, remembering the selection.


## Version [20.08.01](https://github.com/davispuh/gear-emu/releases/tag/v20.08.01) & Commits [fceddc3](https://github.com/davispuh/gear-emu/commit/fceddc3bd4ba1326c49ba76ea2d6be47a1344372) - Cummulative Release

* Updated version to 20.08.01 in files and documentation, syncronizing some code changes with UI-Improvements branch.
* Updated changelog.md with corrected links to propeller forums and version changes.
* Added comment headers to some files without it, and some documentation improvements.


    ## Commit [5474027](https://github.com/davispuh/gear-emu/commit/54740271844681c893d1526360124dd80d012fc5) - Enhanced plugin system and editor.

    * Enhanced plugin format in XML files, also accepting old format. A plugin can have the c# code in a separate file (`*.cs`), to enable debugging into visual studio. Added validation of xml format.
    * Added tab size of plugin editor to program properties.
    * Enhanced comments on plugin template for new plugins, and also in shipped plugins.


    ## Commit [70e132c](https://github.com/davispuh/gear-emu/commit/70e132cb76eb3d98c2e02f48f92fe0ceb8ed09a8) - Merge pull request #[19](https://github.com/davispuh/gear-emu/issues/19) from davispuh/Remember-Dirs

    * Remember last directories binary files and plugins loaded or edited.


    ## Commit [970064d](https://github.com/davispuh/gear-emu/commit/970064d03c85c995e4dd3c3e1dfff49983beecb1) & [a35ab06](https://github.com/davispuh/gear-emu/commit/a35ab062dc844b0a768878bd1c49e6ab5f1b4ea3)- Merge pull request #[18](https://github.com/davispuh/gear-emu/issues/18) from Memotech-Bill/master

    * Corrected timing of Video Generator Frame Reload.
    * Added the ability to set breaks on video frame reloads, either all or just those that do not coinside with a `WAIT_VID`.
    * Fixed refresh when switching between tabs.


    ## Commit [decdc0d](https://github.com/davispuh/gear-emu/commit/decdc0d51bd2885348903a712f3982a877b0f5d0) - Remember last directories binary files and plugins loaded or edited.

    * The last directories of binary and plugins loaded are remembered on main windows and plugin editor.
    * Better documentation of `PluginTemplate` and `PluginBase` classes.
    * A lot of documentation improvements and error corrections.


    ## Commit [961cddc](https://github.com/davispuh/gear-emu/commit/961cddc870d07aad093cb0107c33564e92a1b86c) - Merge pull request #[16](https://github.com/davispuh/gear-emu/issues/16) from mbaeten/master

    * Fixed Z-flag behavior of instructions `SUMC` and `SUMN`.


    ## Commit [74c355d](https://github.com/davispuh/gear-emu/commit/74c355d2fba735d648b7264a309b9be108335cde) - Bug correction on Logic View creation

    * Corrected bug on Logic View: on creation of logic view form, the last grid settings was not updated on form creation.


    ## Commit [b41c66a](https://github.com/davispuh/gear-emu/commit/b41c66a01b5de4b55cfd4a624a37683c588f7b05) - Plugin 1.0 Changes 3

    * Overhead eliminated on run emulator step (calling to a default setting every time).
    * Some DOXYGEN documentation on [Emulator.cs](Emulator.cs)


    ## Commit [7eb43c6](https://github.com/davispuh/gear-emu/commit/7eb43c69b2b98f3efe33c48328d2d8b6e2ebcb2f) - Plugin 1.0 Changes 2

    * Correction on some missing `Z` and `C` flags on [NativeCogs.cs](Emulator.cs).
    * A lot of DOXYGEN documentation on `*.cs`.
    * Ordering on Using libraries on `*.cs`.
    * Some corrections of mispellings.


    ## Commit [8d43068](https://github.com/davispuh/gear-emu/commit/8d43068b7bd6bc8e932e7f7b35e7995bbadfdc4b) - Plugin 1.0 Changes 1

    * Improved syntax highlighting in Plugin Editor: faster processing of text, with progress bar for user feedback.
    * Auto detected class name on the code, to prevent possible inconsistences on the plugin instance name of the XML with the class name on the code.
    * Error grid more intelligent: shown only on errors, with more space on the grid.
    * References List improved: Added name to it, improved tool tips to Add & Remove buttons.
    * Some DOXYGEN documentation fixed.


    ## Commit [d2a38e8](https://github.com/davispuh/gear-emu/commit/d2a38e88aa4e6478e5a28a63799aa9891c501b2a) - Merge Correction 3

    * Corrected spellings errors.


    ## Commit [789b325](https://github.com/davispuh/gear-emu/commit/789b325cf9421fa42f6dc905e1c8ecf496ede4b4) - Merge Correction 2

    * [EmulationCore/PropellerCPU.cs](Gear/EmulationCore/PropellerCPU.cs) - for `PropellerCPU` class changed ancestor to `Propeller.DirectMemory` and consequences, spelling errors. Now it compiles well.
    * [EmulationCore/Cog.cs](Gear/EmulationCore/Cog.cs) - changed references of `CogSpecialAddress` to `Assembly.RegisterAddress`, and references of `CogConditionCodes` to `Assembly.ConditionCodes`, using the definitions of [Propeller/AssemblyRegisters.cs](Gear/Propeller/AssemblyRegisters.cs) and [Propeller/Conditions.cs](Gear/Propeller/Conditions.cs). Also deleted obsolete definitions of enum `CogSpecialAddress` and `CogConditionCodes`. Corrected spelling errors.
    * [Propeller/AssemblyRegisters.cs](Gear/Propeller/AssemblyRegisters.cs) - Added comments from old code in [EmulationCore/Cogs.cs](Gear/EmulationCore/Cogs.cs), principally by adding the correction for `PAR` register (allowing writes) in PASM.
    * [GUI/SpinView.cs](Gear/GUI/SpinView.cs) - Changed invocations of methods `Propeller.ReadYYY()` and `Propeller.WriteYYY()` to `Propeller.DirectReadYYY()` and `Propeller.DirectWriteYYY()`, following the changes in [Propeller/MemoryManager.cs](Gear/Propeller/MemoryManager.cs).


    ## Commit [2eb7f4e](https://github.com/davispuh/gear-emu/commit/2eb7f4eacc3f56de3d77a32d96e57b308e7c1288) - Merge Correction 1

    * [EmulationCore/Cog.cs](Gear/EmulationCore/Cog.cs) - Corrected some changes (`#include Gear.Propeller` header, memory writes & reads invocations with `DirectXXYYY()` methods, spelling errors). Temporary correction:  commented code to memory access. TODO: clear compile errors on dependencies,
    * [EmulationCore/PropellerCPU.cs](Gear/EmulationCore/PropellerCPU.cs) - Corrected memory writes & reads invocations with `DirectXXYYY()` methods.
    * [EmulationCore/InterpretedCog.cs](Gear/EmulationCore/InterpretedCog.cs) - validated corrections in commit 230d27d
    * [EmulationCore/NativeCog.cs](Gear/EmulationCore/NativeCog.cs) - validated corrections in commit 230d27d


    ## Commit [30ca271](https://github.com/davispuh/gear-emu/commit/30ca27126e9a419ab74ae531981c49dc7f6f4fe4) - Merge pull request #2 from gatuno1/Rel_Candidate2015_03

    * Version 2015.03.26


    ## Commit [bd306e9](https://github.com/davispuh/gear-emu/commit/bd306e98d50dd945bb7df5d180a6c79dec1da135) - Final commit to Rel.Cand2015_03

    * Fix the date to 15.03.26


    ## Commit [2d703bf](https://github.com/davispuh/gear-emu/commit/2d703bfd7b4fd4cebc28428f727a19cb79ea4385) - Version corrected to Rel.Cand.15_03

    * Corrected version & date in About dialog and Assembly version.


## Commit [3451731](https://github.com/davispuh/gear-emu/commit/34517317d4301f2ba04811e42f0ac9305ee07416) - Version reference corrected to Rel.Cand.15_03

* Correction on all version references to points to V15.03.31.
* Some spellings corrected in coments.


## Commit [ca1a8cc](https://github.com/davispuh/gear-emu/commit/ca1a8cc78d31b3ce73131df417f88f99e9b9ee48) - Bug corrections of LastPlugin application setting

* Bug Correction on first plugin loading on a fresh instalation of GEAR, related to `LastPlugin` application setting.
* Corrected a bug in references list: it was possible to add a blank line to the list.


## Version 15.03.26

* Corrections on all the effects for PASM hub operations (zero, carry and return): `CLKSET`, COGID, COGINIT, COGSTOP, LOCKNEW, LOCKRET, LOCKSET, LOCKCLR. There was some missing values for carry & zero flags.

* Algorithm optimization for `PropellerCPU.PinChanged()` to determinate the pin state faster for `DIR` and `OUT`.

* Corrected reset events invocations, affecting pins & lock state, and logic view. Now all of them are reset effectively.

* Improvements on LogicView, to show more helpful messages on errors, labels on buttons and text boxes.

* In Plugin Editor now you can start with a default plugin template (new default) or empty window (old default style). The program recovers it from "[`Resource/PluginTemplate.cs`](Gear/bin/Debug/Resources/PluginTemplate.cs)".

* Updated `PluginBase` class structure, so all the old plugins have to be updated:
    - Constructor invocation must call Base constructor.
    - Extra parameter on `OnClock()` method for current clock number in tick clocks.
    - Method `PresentChip()` with no param, beacuse Chip reference now is included in plugin base class definition.
    - New method `OnClose()` is called for every plugin before closing the emulator (to perform cleanup).

* Added program settings to remember them between sessions (stored in "Gear.exe.config" file in this version):
    - `TimeFrame` and `TickMarkGrid` on logic view.
    - `LastBinary` & `LastPlugin` on Emulator, GearDesktop & PluginEditor.
    - `UpdateEachSteps` to enable changes on the screen refresh rate.
    - Added `UseNoTemplate` program setting to enable load plugin editor empty (old default style).

* Changed names and tooltips on buttons "open plugin", "load plugin", "open binary" on GearDesktop & Emulator.

* Memory leaks prevention: Corrections for Disposable members in [`CogView.cs`](Gear/GUI/CogView.cs), [`LogicView.cs`](Gear/GUI/LogicProbe/LogicView.cs), [`MemoryView.cs`](Gear/GUI/MemoryView.cs) & [`PluginEditor.cs`](Gear/GUI/PluginEditor.cs).

* Improved general documentation of source code, including specific pages for sequence of callings for `PropellerCPU.Step()` and loading a plugin in memory after compilation.


## Version 14.07.03

* Faster emulation.
    - In my own testing, GEAR runs now about 30% or 35% faster than Gear V09_10_26, maybe because the executable now use MS .NET framework 4.0 instead of 2.0. The drawback is the need to download the framework, but in windows 7 or 8, probably it is installed already.
    - Also, updated the project files to MS Visual C# 2010 Express (the old proyect was in MS Visual C# 2005). I use that because it was the only I could find.

* Show Cog RAM Special Purpose Registers values.
    - Now the values of special registers in cog memory (between `$1F0` - `PAR` and `$1FF` - `VSCL`) are displayed correctly in memory or PASM view.

* Logic modes in counters.
    - Logic modes (`CTRMODE` between `%10000` and `%11111`) didn't work well in past versions of Gear.

* Correction to enable PAR register changes.
    - As there was some reports of prop forum users that GEAR was of not capable to change the `PAR` cog register (forum thread ["PASM simulator / debugger?"](https://forums.parallax.com/discussion/115909/PASM-simulator-debugger)) then I enabled it. They claims that some parallax video drivers in PASM changes the `PAR` register, and GEAR didn't emulate that. The Propeller Manual V1.2 specify that is a read-only register, but if parallax did that drivers that really run on the real one...

* Fixes in Logic View.
    - When you reload a binary or reset it, and you had run it before using pins, the logic view show a mix between new and old samples.
    - If you are using windows in other languages than english, -with other number formats-, the preset values for time frame and tick mark (with "." decimal separator) will not work if you press the update button.

* Flickering correction on cog view over PASM lines.
    - When you were on cog view (PASM code), the tool tip text sometimes flickers.

* Tooltips over pins and locks in hub view.
    - Added tooltips on hub view on pins and locks views (`IN*`, `DIR*`, Floating, Lock Free, Locks), to identify easily which pin or lock is.

* Plugin editor enhancements.
    - Experimental C# sintax highlighting of code. Now the name of the file is displayed on the title of the window. Added tooltips to be more self explaining.


## Version 09.10.26 26 October 2009

* Many thanks to Bob Anderson for identifying bugs in and subsequently testing improvements to the emulation of `SUBS`, `SUBSX`, `CMPSX` and `REV`.


## Version 09.06.05 (140) 5 June 2009

* Many thanks to Ben Levitt for improving the behaviour of GEAR. (Reload Binary, Open to PlugIn, Close PlugIn)
* Improvements in [`SerialIO`](plug-ins/SerialIO.xml) plugin (Ben Levitt).


## Version 09.05.12 (50) 11 May 2009

* Fixed memory references of the form `LONG[ &MyVar ][ 2 ]`, this case was being emulated as `LONG[ 2 ][ &MyVar ]`.
* Included [`SerialIO`](plug-ins/SerialIO.xml) plugin (with thanks to Ben Levitt) in the distribution.


## Version 08.10.16 (151) 16 October 2008

* An extra function has been added which allows hot keys to be disabled for a plugin.
Simply add the following function to your plugin:
`public override Boolean AllowHotKeys { get { return false; } }`.


## Version 08.04.29 (159) 29 April 2008
* The lastest stimulus.zip is now included in V08_04_29.zip.
* `'R'` and `'S'` keys can now be used to run, stop and step the currently active cog.
* Floating the cursor over an assembly cog page shows the values stored at the source and destination registers for the instruction line that's under the mouse. The values are shown as both hex and decimal.


### On 23 January 2008

* Updated stimulus.zip. Still works with V08_01_18 of GEAR.
* The stimulus plug-in page has a context menu which allows a new stimulus file to be loaded and the current one to be saved.
* Editing of the stimulus file is allowed - previously it was read-only.
* As part of the stimulus file, it is now possible to include the words reset and stop.

    * reset - simulates a reset of the processor.
    * stop - causes a breakpoint - click Run to go to the next stop / breakpoint.

        How's this for something you haven't been able to do til now!!
        ```
        ! Stimulus File breakpoint example
        !  (diagnosing the first microseconds of life)
        10u stop
        +10u stop
        +10u stop
        +10u stop
        +10u stop
        +10u reset
        ```

### On 21 January 2008

* Added stimulus.zip which updates `stimulus.xml` and the example stimulus file. It is now possible to do clock and pwm generation.


## Version 08_01_18 (140)

* Pin toggling by means of a stimulus file.


## Version 08.01.13 (86)

* Better disassembly of PASM.
* Some fixes in assembly language emulation
* An alternative / short instruction set for the spin bytecodes (the original is still available).
* The ability to display numbers as hex or decimal.
* The ability to add a single breakpoint (just click on a line of code).
* Improved simulation of the timing from the output pins.
* Better disassembly of PASM.
* Some fixes in assembly language emulation.
* An alternative / short instruction set for the spin bytecodes (the original is still available).
* The ability to display numbers as hex or decimal.
* The ability to add a single breakpoint (just click on a line of code).


## Version 1.11.0.0

* Miscellaneous bug fixes.
* [`Television`](plug-ins/Television.xml) plugin.


## Version 1.10.0.0

* Fixed `HUBOP` instruction (source and destination fields were switched).
* Fixed `PAR`, now it's the 14 bit value passed, not the data stored at that address.
* Fixed jitter in the analog view.


## Version 1.9.0.0

* Fixed carry in addition / subtraction operations (most of them at least).
* Changed the VGA plug-in to be sensitive on sync edges.
* First official open source release.


## Version 1.8.0.0

* Fixed `MIN/MAX/MINS/MAXS` instructions.
* Improved and Consolidated PLL code.
* General cleanups.
* Fixed broadcast video emulation.


## Version 1.7.0.0

* Now you can add pin ranges in the text box (12..14 is the equivalent to 12,13,14).
* Reduced stack usage for interpreted cogs.
* Fixed `COGINIT/COGNEW` for interpreted calls.
* Fixed nested function calls.
* Added broadcast emulation (possibly incorrect).
* Changed the way PLLs work, allows for multiple aural hooks (since they are used now).


## Version 1.6.0.0

* Implemented proper interpreted random output (thanks Paul and chip!).
* Added analog channels to the logic view.
* Reduced inital pins in logic view to 32 (0 - 31) since those are the only ones anyone should be using anyway.
* Double clicking a channel in the logic view deletes it now.
* Added the ability to add digital channels (incase you deleted it).
* Fixed `CMPSUB`, frequencys are properly calculated in TV based applications.
* Increased the number of samples allowed in the logic view from 256 to 1024.
* Added a good ammount of initial condition checking to the GUI (although there is something wrong if the events occur before the emulator is presented to a plugin).
* Other little fixes here and there.


## Version 1.5.0.0

* Proper handling of condition field on native instructions.
* `VSCL` is now buffered between `WAITVID` frames (`VCFG` is not).
* Changed behavior of Forward and Reverse random (negitive feedback `LSFR`, rather than rotating buffer with `XOR`).
* Timing fixed on instructions.
* Fixed parity and other various instruction flag issues.
* A cog stops rather than reboots the chip if the interpreter returns from it's starting function (stack underflow).
* Increased the size of the virtual VGA monitor plug-in (Bitmap 512x384 VGA runs at 1024x768 with pixel / line doubling).
* Included a [`PinNoise`](plug-ins/PinNoise.xml) plugin, which plays with some pins.
* PLLs dejitter is now more accurate (uses sub-system clock accuracy).


## Version 1.4.0.0

* Fixed a HUGE problem involving inplace effected memory (interpreted)
* x /= y was being processed as x := y / x (Which is bad). Order of operations is now appropriate.
* Changed the name of BusModule to `PluginBase` class (makes more sense to me)
* Sped up the interpreted core a little bit (doesn't use the stack for internal temporary variables).
* Cleaned up the source significantly.
* Fixed a minor bug in the composite video generator phase shifting code (s-video).
* Numerous bug fixes scattered about the native and interpreted cores.


## Version 1.3.0.0

* Support for Frequency Generator modes 1-3 (PLL), and VGA video generators.
* New SPIN memory viewer, but it's incomplete.
