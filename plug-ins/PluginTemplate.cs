//Plugin template for plugin system v1.0
//
//Add here extra assemblies needed in your plugin
using System;
using Gear.EmulationCore;
using Gear.PluginSupport;

//Class name declared below must match class name on plugin editor text box
class YourClassName : PluginBase
{
    private PropellerCPU Chip;   //Reference to the emulating chip.

    //Title to be shown in tab, change it as you need.
    public override string Title
    { 
        get { return "PluginBase"; }   
    }

    //Set the reference to the emulated chip. Occurs once the plugin is loaded.
    //Also, if you need the plugin be notified on pin or clock changes, you 
    // need to mantain the calls to NotifyOnPins or NotifyOnClock. To improve 
    // performance, remove the unnecesary one.
    public override void PresentChip(PropellerCPU host)
    {
        Chip = host;
        //use to be notified on changed pins (DIRx, OUTx, INx).
        Chip.NotifyOnPins(this);
        //use to be notified on each clock tick.
        Chip.NotifyOnClock(this);
        //for initial setup you can use here method: 
        //  Chip.DrivePin(int pin_number, bool Floating, bool Hi)
        //  but it is a good practice do the same on method OnReset()
    }

    //Called every time a pin changes, if called Chip.NotifyOnPins() in 
    // method PresentChip(.) above. If not used, remove to improve performance.
    public override void OnPinChange(double time, PinState[] pins)
    {
        //Put your code here.
        //You could set or test pin states like "pins[0] == PinState.FLOATING;" 
        // or "if (pins[1] == PinState.FLOATING)..." 
        //Possible values for PinState enum are: 
        // FLOATING, OUTPUT_LO, OUTPUT_HI, INPUT_LO, INPUT_HI.
        //
    }
    
    //Called every clock tick, if called Chip.NotifyOnClock() in method 
    // PresentChip(.) above. If not used, remove to improve performance.
    public override void OnClock(double time) 
    { 
        //Put your code here.
    }

    public override void OnReset()
    {
        //Put your code here.
        //for initial setup you can use here method: 
        //  Chip.DrivePin(int pin_number, bool Floating, bool Hi)
        //  and it is a good practice do the same on method PresentChip().
    }

}