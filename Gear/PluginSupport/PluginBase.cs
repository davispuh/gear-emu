/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * PluginBase.cs
 * Abstract superclass for emulator plugins
 * --------------------------------------------------------------------------------
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 * --------------------------------------------------------------------------------
 */

using Gear.EmulationCore;
using System.Windows.Forms;

// ReSharper disable InvalidXmlDocComment

/// <summary>Name space for Plugin support.</summary>
/// <remarks>Contains the classes that defines the plugin system: the plugin
/// class structure itself, the loading of plugins from XML files,
/// the compiling and instantiation of a plugin class.</remarks>
namespace Gear.PluginSupport
{
    /// <summary>Base class for plugin support.</summary>
    /// <remarks>Define basic methods and attributes for plugins in GEAR.
    /// Almost every window on GEAR is based on it.
    /// To see examples of how to use it, see the directory 'plugins'
    /// included with the source code.</remarks>
    /// @note See Asterisk's comments:
    /// Source: <a href="https://forums.parallax.com/discussion/comment/636953/#Comment_636953">
    /// Original thread on GEAR with explanation of plugin class</a>
    public partial class PluginBase : UserControl
    {
        /// <summary>Reference to PropellerCPU for the plugin.</summary>
        /// @version v15.03.26 - Added reference to keep PropellerCPU internal
        /// to class.
        protected PropellerCPU Chip;

        /// <summary>Title of the tab window.</summary>
        /// @note Changed default value , based on a comment from Asterisk
        /// from propeller forum:
        /// Source: <a href="https://forums.parallax.com/discussion/comment/627190/#Comment_627190">
        /// Post #32 from original GEAR post</a>. It shows that the original
        /// name of the class was "BusModule". Changed to the new name of
        /// the class.
        /// @version v15.03.26 - change the default name.
        public virtual string Title => "Plugin Base";

        /// <summary>Attribute to allow key press detecting on the plugin.</summary>
        /// @note Mirror's: allows hot keys to be disabled for a plugin.
        /// @note Source: <a href="https://forums.parallax.com/discussion/100380/more-gear-improved-emulation-of-the-propeller">
        /// Mirror Post for Version V08_10_16 in propeller forums</a>
        public virtual bool AllowHotKeys => true;

        /// <summary>Attribute to allow the window to be closed (default) or
        /// not (like cog windows).</summary>
        /// <remarks>Not to be used in Plugin Editor by user plugins.</remarks>
        public virtual bool IsClosable => true;

        /// <summary>Identify a plugin as user (=true) or system (=false).</summary>
        /// @version v15.03.26 - Added.
        public virtual bool IsUserPlugin => true;

        /// <summary>Default constructor.</summary>
        /// @note Not to be used by plugin derived class, only by the
        /// Designer in Visual Studio.
        /// <remarks>Not to be used in Plugin Editor by user plugins.</remarks>
        protected PluginBase()
        {
            InitializeComponent();
        }

        /// <summary>Constructor to initialize with the PropellerCPU reference.</summary>
        /// This avoid to declare in each plugin the following example code:
        /// @code{.cs}
        /// class PinNoise : PluginBase
        /// {
        ///     private PropellerCPU Chip;  /*&lt;== this line will not be necessary to declare in every plugin anymore.*/
        /// ...
        /// @endcode
        /// <param name="chip">Propeller CPU reference.</param>
        /// @version v15.03.26 - Modified to add %PropellerCPU parameter.
        public PluginBase(PropellerCPU chip)
        {
            Chip = chip;
            InitializeComponent();
        }

        /// <summary>Register the events to be notified to this plugin.</summary>
        /// <remarks>Set the reference to the emulated chip. Occurs once
        /// the plugin is loaded.
        ///
        /// Also, if you need the plugin be notified on pin or clock changes, you
        /// need to add inside this method calls to NotifyOnPins or NotifyOnClock.
        /// To keep good performance, use only the essentials ones.</remarks>
        /// @note Original documentation: <a href="https://forums.parallax.com/discussion/comment/625629/#Comment_625629">
        /// API GEAR described on GEAR original Post</a>
        /// @version v15.03.26 - Changed to method without parameters.
        public virtual void PresentChip() { }

        /// <summary>Event when the chip is reset.</summary>
        /// Useful to reset plugin's components or data, to their initial states.
        public virtual void OnReset() { }

        /// <summary>Event when the plugin is closing.</summary>
        /// <remarks>Useful to reset pins states or direction to initial state
        /// before loading the plugin, or to release pins driven by the plugin.</remarks>
        /// @version v15.03.26 - Added.
        public virtual void OnClose() { }

        /// <summary>Event when a clock tick is informed to the plugin, in clock
        /// units.</summary>
        /// @param time Time in seconds of the emulation.
        /// @param sysCounter Present system clock in ticks unit.
        /// @warning If sysCounter is used only, the plugin designer have
        /// to take measures to detect and manage system counter rollover.
        /// @version v15.03.26 - Modified to have two parameters.
        public virtual void OnClock(double time, uint sysCounter) { }

        /// <summary>Event when some pin changed and is informed to the plugin.</summary>
        /// @note Asterisk's: occurs every time a pin has changed states.
        /// PinState tells you if either the propeller or another component
        /// has set the pin Hi or Lo, or if the pin is floating.
        /// @note Source: <a href="https://forums.parallax.com/discussion/comment/625629/#Comment_625629">
        /// API GEAR described on GEAR original Post</a>
        /// @param time Time in seconds.
        /// @param pinStates Array of pins with the current state.
        /// @version v22.05.02 - Changed parameter name to clarify meaning of it.
        public virtual void OnPinChange(double time, PinState[] pinStates) { }

        /// <summary>Event to repaint the plugin screen (if used).</summary>
        /// @note Asterisk's: occurs when the GUI has finished executing
        /// a emulation 'frame' (variable number of clocks). Force is always
        /// true (this means that the call wants to 'force' an update, this is
        /// provided so you can pass a false for non-forced repaints).
        /// @note Source: <a href="https://forums.parallax.com/discussion/comment/625629/#Comment_625629">
        /// API GEAR described on GEAR original Post</a>
        /// @param force Flag to indicate the intention to force the repaint.
        public virtual void Repaint(bool force) { }

        /// <summary>Notifies that this plugin must be notified on pin changes.
        /// This method is to isolate the access to the underline Chip.</summary>
        /// @version v15.03.26 - Added.
        public void NotifyOnPins()
        {
            Chip.NotifyOnPins(this);
        }

        /// <summary>Notifies that this plugin must be notified on clock ticks.
        /// This method is for isolate the access to the underline Chip.</summary>
        /// @version v15.03.26 - Added.
        public void NotifyOnClock()
        {
            Chip.NotifyOnClock(this);
        }

        /// <summary>Drive a pin of PropellerCPU.</summary>
        /// <remarks>The purpose of this method is to isolate the access to
        /// the underline Chip.</remarks>
        /// @param pin Pin number to drive.
        /// @param isFloating Boolean to left the pin floating (=true) or to
        /// set on input/output mode (=false).
        /// @param isHigh Boolean to set on High state (=true) or to set
        /// on Low (=false).
        /// @version v22.05.02 - Parameter names changed, to clarify purpose
        /// of each one.
        public void DrivePin(int pin, bool isFloating, bool isHigh)
        {
            Chip.DrivePin(pin, isFloating, isHigh);
        }

        /// <summary>Set an immediate breakpoint.</summary>
        /// <remarks>The purpose of this method is to isolate the access to
        /// the underline Chip.</remarks>
        /// @version v15.03.26 - Added.
        public void BreakPoint()
        {
            Chip.BreakPoint();
        }
    }
}
