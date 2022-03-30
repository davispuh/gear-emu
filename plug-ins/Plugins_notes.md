# Plugins Contents:

## Index

### A) Included Plugins
* **PinNoise** - stimulus on some pins at a fixed clock rate.
* **SerialIO** - terminal emulation.
* **Stimulus** - read stimulus from file, and set propeller pins according by time specified on it.
* **Television** - emulation of a screen TV signal reception from propeller.
* **vgamonitor** - emulation of a VGA signal reception from propeller.

### B) Plugin API

---

# Plugins Details:

## **1) PinNoise** - stimulus on some pins at a fixed clock rate.
*Included in original GEAR.*

Demo to use stimulus. By default it changes pin `p2` in every clock, and pin `p3` in every two clocks. To change that, you need to edit it in the plugin editor, at your will.

---
## **2) SerialIO** - terminal emulation.
*Included in mirror's GEAR version, with help of Ben Levitt.*

Lets you communicate interactively with the simulated Propeller. It talks to a FullDuplexSerial object running on the simulated chip (or anything pin-compatible).

Then just type to send, and stare at the screen to receive. Copy and Paste work too. It has user controls in tab view to change tx & rx pins, and baud rate.

---
## **3) Stimulus** - read stimulus from file, and set propeller pins according by time specified on it.
*Included in original GEAR.*

It read `*.stm` files. See included `test.stm` to hints to formats and usage.

---
## **4) Television** - emulation of a screen TV signal reception from propeller.
*Included in original GEAR.*

By default it uses pins `p12`, `p13` and `p14`. To change that, you need to edit it in the plugin editor, at your will.

---
## **5) vgamonitor** - emulation of a VGA signal reception from propeller.
*Included in original GEAR.*

By default it uses pins `p16` and `p17` as the Propeller board has. To change that, you need to edit it in the plugin editor, at your will.

---

# Plugin API

## Useful methods

The full class is defined in [PluginBase.cs](../Gear/PluginSupport/PluginBase.cs), but a scheme of the useful methods are this:

```csharp
namespace Gear.PluginSupport
{
    public class PluginBase : UserControl
    {
        /// Reference to PropellerCPU for the plugin.
        protected PropellerCPU Chip;

        /// @brief Title of the tab window.
        public virtual string Title { }

        /// Called once when the plugin is loaded. It gives you a reference
        /// to the propeller chip (so you can drive the pins).
        public virtual void PresentChip() { }

        /// Event when the chip is reset. Useful to reset plugin's components
        /// or data, to their initial states.
        public virtual void OnReset() { }

        /// Event when the plugin is closing. Useful to reset pins states or
        /// direction to initial state before loading the plugin, or to release
        /// pins driven by the plugin.
        public virtual void OnClose() { }

        /// Event when a clock tick is informed to the plugin, in clock units.
        /// @param time Time in seconds of the emulation.
        /// @param sysCounter Present system clock in ticks unit.
        /// @warning If sysCounter is used only, the plugin designer have to take
        ///    measures to detect and manage system counter rollover.
        public virtual void OnClock(double time, uint sysCounter) { }

        /// Event when some pin changed and is informed to the plugin.
        /// Occurs every time a pin has changed states. PinState tells you if
        /// either the propeller or another component has set the pin Hi or Lo,
        /// or if the pin is floating.
        /// @param time Time in seconds.
        /// @param pins Array of pins with the current state.
        public virtual void OnPinChange(double time, PinState[] pins) { }

        /// Event to repaint the plugin screen (if used).
        /// Occurs when the GUI has finished executing a emulation 'frame'
        /// (variable number of clocks).
        /// @param force Flag to indicate the intention to force the repaint.
        public virtual void Repaint(bool force) { }
    }
}
```

By default, the following references will be applied:
```
System.Windows.Forms.dll
System.dll
System.Data.dll
System.Drawing.dll
System.Xml.dll
```

## **Important Notes and limitations**

* The emulator emulates all 64 possible pins (rather than the current cap of 32). All pins default as floating.

* Other components do not cause events to occur. Also, when a component changes a pin, that value is committed (no ORing or anything like that) So, no bus sharing.

* The Propeller's output takes priority over the components (so, if DIR* is set HI, the pin state's will be set to the output of the PROPELLER).

---
## Typical Plugin template

The initial template is full detailled in [PluginTemplate.cs](Gear/Resources/PluginTemplate.cs), but the outline is:
```csharp
//Plugin template for plugin system v1.0

//Assemblies needed for GEAR plugin system (do not delete them).
using System;
using Gear.EmulationCore;
using Gear.PluginSupport;
//Add here extra assemblies as needed in your plugin.


//Class name of your plugin. You must change it to avoid conflics with others.
class YourPluginClassName : PluginBase
{
	//Constructor for the initialization of your plugin. Must be the same
    //  name of above.
    //Include here your initialization code only for attributes you add to
    //  your class or interface objects (example create user controls for
    //  the tab window). If you don't use any, don't add code here, but you
    //  must not delete this method.
    //If you wish to use DrivePin() for initial setup of pins, call it
    //  inside of OnReset() method, instead of here.
	public YourPluginClassName(PropellerCPU chip) : base(chip)
	{
		OnReset();  // <= don't delete this
		// Put your code here:
	}

	//Title to be shown in tab. You must change to your plugin name.
	public override string Title
	{
		get { return "My Plugin"; }  //change to your plugin name
	}

	//Set the reference to the emulated chip. Occurs once the plugin is loaded.
	//Also, if you need the plugin be notified on pin or clock changes, you
	//  need to add inside this method calls to NotifyOnPins or NotifyOnClock.
	//To keep good performance, use only the essentials ones.
    //To be notified on changed pins (ex. DIRx, OUTx, INx) use here:
    //  NotifyOnPins();
    //To be notified on each clock tick, use here:
    //  NotifyOnClock();
	public override void PresentChip()
	{
		//Put your code here:
	}

	//Called every time a pin changes, if called Chip.NotifyOnPins() in
	//  method PresentChip(.) above.
    //You could set or test pin states like "pins[0] == PinState.FLOATING;"
    //  or "if (pins[1] == PinState.FLOATING)..."
    //Possible values for PinState enum are:
    //  FLOATING, OUTPUT_LO, OUTPUT_HI, INPUT_LO, INPUT_HI.
	public override void OnPinChange(double time, PinState[] pins)
	{
		//Put your code here.
	}

	//Called every clock tick, if called Chip.NotifyOnClock() in method
	//  PresentChip() above.
	public override void OnClock(double time, uint sysCounter)
	{
		//Put your code here.
	}

	//Called when you press the Reset button, to reset the plugin to a
	//  initial state.
    //For initial setup of the plugin (but not the creation of interface
    //  objects), you can add your code here. The idea if you have to reset
    //  the pins state, you can use here the DrivePin() method. Example:
    //  DrivePin(int pin_number, bool Floating, bool Hi);
	public override void OnReset()
	{
		//Put your code here.
	}
}
```
**Warning:**
To assure the correct correct operation of the plugin system, you can't change the parameters of any predefined method: neither the names or the types. Also you must not add other parameters.
