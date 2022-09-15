/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
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

// ReSharper disable InconsistentNaming

namespace Gear.EmulationCore
{
    /// <summary>Provides PLLA and PLLB to a Cog, as well as forwards internal
    /// signals to a video generator object.</summary>
    public class PLLGroup : ClockSource
    {
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private readonly List<VideoGenerator> _auralHooks;
        /// <summary></summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private VideoGenerator _partner;

        /// <summary>First clock generator: A.</summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private readonly PLL _pllA;
        /// <summary>Second clock generator: B.</summary>
        /// @version v22.05.04 - Changed name to follow naming conventions.
        private readonly PLL _pllB;

        /// <summary></summary>
        public ulong Pins => _pllA.Pins | _pllB.Pins;

        /// <summary>How much time (in seconds) left until to tick.</summary>
        public override double TimeUntilClock
        {
            get
            {
                double a = _pllA.TimeUntilClock;
                double b = _pllB.TimeUntilClock;
                return a < b ?
                    a :
                    b;
            }
        }

        /// <summary>Default constructor.</summary>
        public PLLGroup()
        {
            _pllA = new PLL();
            _pllB = new PLL();
            _auralHooks = new List<VideoGenerator>();
            Disable(true);  //PLL A
            Disable(false); //PLL B
        }

        /// <summary></summary>
        /// <param name="partner"></param>
        public void SetupPLL(VideoGenerator partner)
        {
            _partner = partner;
            _auralHooks.Clear();
        }

        /// <summary></summary>
        public void Destroy()
        {
            _partner = null;
            _auralHooks.Clear();
        }

        /// <summary></summary>
        /// <param name="videoGenerator"></param>
        /// @version v22.05.04 - Parameter name changed to clarify meaning of it.
        public void AttachHook(VideoGenerator videoGenerator)
        {
            if (!_auralHooks.Contains(videoGenerator))
                _auralHooks.Add(videoGenerator);
        }

        /// <summary></summary>
        /// <param name="videoGenerator"></param>
        /// @version v22.05.04 - Parameter name changed to clarify meaning of it.
        public void RemoveHook(VideoGenerator videoGenerator)
        {
            if (_auralHooks.Contains(videoGenerator))
                _auralHooks.Remove(videoGenerator);
        }

        /// <summary></summary>
        /// <param name="cyclesPerSecond"></param>
        /// @version v22.05.04 - Parameter name changed to clarify meaning of it.
        public void SetBaseFrequency(uint cyclesPerSecond)
        {
            _pllA.SetBaseFrequency(cyclesPerSecond);
            _pllB.SetBaseFrequency(cyclesPerSecond);
        }

        /// <summary></summary>
        /// <param name="isFreqGeneratorA"></param>
        /// <param name="multiplier"></param>
        /// @version v22.05.04 - Parameter name changed to clarify meaning of it.
        public void Feed(bool isFreqGeneratorA, double multiplier)
        {
            if (isFreqGeneratorA)
                _pllA.Feed(multiplier);
            else
                _pllB.Feed(multiplier);
        }

        /// <summary></summary>
        /// <param name="isFreqGeneratorA"></param>
        /// <param name="pinA"></param>
        /// <param name="pinB"></param>
        /// @version v22.05.04 - Parameter name changed to clarify meaning of it.
        public void DrivePins(bool isFreqGeneratorA, ulong pinA, ulong pinB)
        {
            if (isFreqGeneratorA)
                _pllA.DrivePins(pinA, pinB);
            else
                _pllB.DrivePins(pinA, pinB);
        }

        /// <summary></summary>
        /// <param name="isFreqGeneratorA"></param>
        /// @version v22.05.04 - Parameter name changed to clarify meaning of it.
        public void Disable(bool isFreqGeneratorA)
        {
            if (isFreqGeneratorA)
                _pllA.Disable();
            else
                _pllB.Disable();
        }

        /// <summary></summary>
        /// <param name="isFreqGeneratorA"></param>
        /// <param name="frequency"></param>
        /// @version v22.05.04 - Parameter name changed to clarify meaning of it.
        public void SetFrequency(bool isFreqGeneratorA, double frequency)
        {
            if (isFreqGeneratorA)
                _pllA.SetFrequency(frequency);
            else
                _pllB.SetFrequency(frequency);
        }

        /// <summary></summary>
        /// <param name="time"></param>
        /// @version v22.05.04 - Local variable name changed to clarify meaning of it.
        public override void AdvanceClock(double time)
        {
            if (_pllA.AdvanceClock(time))
            {
                bool output = _pllA.Output;
                foreach (VideoGenerator videoGenerator in _auralHooks)
                    videoGenerator.AuralTick(output);
                _partner.ColorTick(output);
            }
            if (_pllB.AdvanceClock(time))
                _partner.CarrierTick(_pllB.Output);
        }
    }
}
