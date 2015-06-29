/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * PLLGroup.cs
 * Provides PLLA and PLLB to a Cog, as well as forwards internal signals to a
 * video generator object.
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

using System.Collections.Generic;

namespace Gear.EmulationCore
{
    public class PLLGroup : ClockSource
    {
        private List<VideoGenerator> AuralHooks;
        private VideoGenerator Partner;

        private PLL PllA;
        private PLL PllB;

        public ulong Pins
        {
            get
            {
                return PllA.Pins | PllB.Pins;
            }
        }

        public override double TimeUntilClock
        {
            get
            {
                double a = PllA.TimeUntilClock;
                double b = PllB.TimeUntilClock;

                return a < b ? a : b;
            }
        }

        public PLLGroup()
        {
            PllA = new PLL();
            PllB = new PLL();

            AuralHooks = new List<VideoGenerator>();
            Disable(true);
            Disable(false);
        }

        public void SetupPLL(VideoGenerator partner)
        {
            Partner = partner;

            AuralHooks.Clear();
        }

        public void Destroy()
        {
            Partner = null;
            AuralHooks.Clear();
        }

        public void AttachHook(VideoGenerator vgn)
        {
            if (!AuralHooks.Contains(vgn))
                AuralHooks.Add(vgn);
            return;
        }

        public void RemoveHook(VideoGenerator vgn)
        {
            if (AuralHooks.Contains(vgn))
                AuralHooks.Remove(vgn);
            return;
        }

        public void SetBaseFrequency(uint cps)
        {
            PllA.SetBaseFrequency(cps);
            PllB.SetBaseFrequency(cps);
        }

        public void Feed(bool pllA, double multiplier)
        {
            if (pllA)
                PllA.Feed(multiplier);
            else
                PllB.Feed(multiplier);
        }

        public void DrivePins(bool pllA, ulong pinA, ulong pinB)
        {
            if (pllA)
                PllA.DrivePins(pinA, pinB);
            else
                PllB.DrivePins(pinA, pinB);
        }

        public void Disable(bool pllA)
        {
            if (pllA)
                PllA.Disable();
            else
                PllB.Disable();
        }

        public void SetFrequency(bool pllA, double frequency)
        {
            if (pllA)
                PllA.SetFrequency(frequency);
            else
                PllB.SetFrequency(frequency);
        }

        public override void AdvanceClock(double time)
        {
            if (PllA.AdvanceClock(time))
            {
                bool output = PllA.Output;

                foreach (VideoGenerator vgn in AuralHooks)
                    vgn.AuralTick(output);

                Partner.ColorTick(output);
            }

            if (PllB.AdvanceClock(time))
            {
                Partner.CarrierTick(PllB.Output);
            }
        }
    }
}
