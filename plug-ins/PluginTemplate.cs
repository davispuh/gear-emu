//Plugin template for plugin system v1.0
//
//Add here extra assemblies as needed in your plugin.
using System;
using Gear.EmulationCore;
using Gear.PluginSupport;

//Class name declared below must match class name on plugin editor text box
class <YourClassName> : PluginBase
{
    //Constructor for the initialization
    public <YourClassName>(PropellerCPU chip) : base(chip)
    {
        //include here your initialization code as interface objects
        //for initial setup you can use here method DrivePin() but it is a 
        //better practice to put that in method OnReset(), and call it here:
        //  OnReset();
        //
        //Put your code here.
        
    }

    //Title to be shown in tab, change it as you need.
    public override string Title
    { 
        get { return "PluginBase"; }   
    }

    //Set the reference to the emulated chip. Occurs once the plugin is loaded.
    //Also, if you need the plugin be notified on pin or clock changes, you 
    // need to mantain the calls to NotifyOnPins or NotifyOnClock. To improve 
    // performance, use only the necesaries.
    public override void PresentChip()
    {
        //Note: use "Chip" member to access properties and methods.
        //to be notified on changed pins (ex. DIRx, OUTx, INx) use:
        //  NotifyOnPins();
        //to be notified on each clock tick, use:
        //  NotifyOnClock();
        //
        //Put your code here.
    }

    //Called every time a pin changes, if called Chip.NotifyOnPins() in 
    // method PresentChip(.) above. If not used, remove to improve performance.
    public override void OnPinChange(double time, PinState[] pins)
    {
        //You could set or test pin states like "pins[0] == PinState.FLOATING;" 
        //or "if (pins[1] == PinState.FLOATING)..." 
        //Possible values for PinState enum are: 
        //  FLOATING, OUTPUT_LO, OUTPUT_HI, INPUT_LO, INPUT_HI.
        //
        //Put your code here.
        
    }
    
    //Called every clock tick, if called Chip.NotifyOnClock() in method 
    // PresentChip() above. If not used, remove to improve performance.
    public override void OnClock(double time) 
    { 
        //Put your code here.
    }

    public override void OnReset()
    {
        //for initial setup you can use here method DrivePin(), and call it on the constructor.
        //  DrivePin(int pin_number, bool Floating, bool Hi);
        //
        //Put your code here.
        
    }

}