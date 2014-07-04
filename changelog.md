

Read more in forum threads:
* [More GEAR - Improved Emulation of the Propeller](http://forums.parallax.com/showthread.php/100380-More-GEAR-Improved-Emulation-of-the-Propeller)
* [GEAR: Propeller Debugging Environment](http://forums.parallax.com/showthread.php/91084-GEAR-Propeller-Debugging-Environment)


## V09_10_26 26 October 2009

* Many thanks to Bob Anderson for identifying bugs in and subsequently testing improvements to the emulation of SUBS, SUBSX, CMPSX and REV.


## V09_06_05 (140) 5 June 2009

* Many thanks to Ben Levitt for improving the behaviour of GEAR. (Reload Binary, Open to PlugIn, Close PlugIn)
* Improvements in SerialIO plugin (Ben Levitt).


## V09_05_12 (50) 11 May 2009

* Fixed memory references of the form `LONG[ &MyVar ][ 2 ]`, this case was being emulated as `LONG[ 2 ][ &MyVar ]`.
* Included SerialIO plugin (with thanks to Ben Levitt) in the distribution.


## V08_10_16 (151) 16 October 2008

* An extra function has been added which allows hot keys to be disabled for a plugin.
Simply add the following function to your plugin:
`public override Boolean AllowHotKeys { get { return false; } }`


## V08 04 29 (159) 29 April 2008
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


## V08_01_18 (140)

* Pin toggling by means of a stimulus file.


## V08_01_13 (86)

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

