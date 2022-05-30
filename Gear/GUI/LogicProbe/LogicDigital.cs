/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * LogicDigital.cs
 * Provides a Digital logic channel for LogicView
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
using System;
using System.Drawing;

namespace Gear.GUI.LogicProbe
{
    /// @brief Provides a Digital logic channel for LogicView.
    public class LogicDigital : LogicRow
    {
        /// <summary>Maximum samples to be saved in each channel.</summary>
        private const int MaximumSamples = 1024;
        /// <summary>Mask of maximum samples.</summary>
        /// @version v22.06.01 - Added.
        private const int SamplesMask = MaximumSamples - 1;

        /// <summary>Array for time value of a sample.</summary>
        private readonly double[] _time;
        /// <summary>Array for pin state of a sample.</summary>
        private readonly PinState[] _pins;
        /// <summary>Pin number witch this channel will be associated.</summary>
        private readonly int _pinNumber;

        /// <summary>Index position of write pointer, one based.</summary>
        private int _writePointer;
        /// <summary>Indicator of wrapped state for samples.</summary>
        private bool _wrapped;

        /// <summary></summary>
        public override double MinTime => 
            _wrapped ?
                _time[_writePointer] :
                _time[0];

        /// <summary>Height this channel on screen.</summary>
        public override int Height => 16;

        /// <summary>Name of channel, associated to a pin number.</summary>
        /// @version v22.06.01 - Changed to use interpolated string text.
        public override string Name =>
            $"P{_pinNumber}";

        /// @brief Default Constructor.
        /// @param pin Pin number to represent.
        public LogicDigital(int pin)
        {
            _pins = new PinState[MaximumSamples];
            _time = new double[MaximumSamples];
            _wrapped = false;
            _pinNumber = pin;
            Reset();
        }

        /// <summary></summary>
        public override void Click() { }

        /// @brief Clear samples when reset is needed.
        public sealed override void Reset()
        {
            _time[0] = 0.0;
            _pins[0] = PinState.FLOATING;
            _writePointer = 1;
        }

        /// <summary></summary>
        /// <param name="pin"></param>
        /// <param name="time"></param>
        public void Update(PinState pin, double time)
        {
            if (_pins[(_writePointer - 1) & SamplesMask] == pin)
                return;
            _pins[_writePointer] = pin;
            _time[_writePointer] = time;
            _writePointer = (_writePointer + 1) & SamplesMask;
            if (_writePointer == 0)
                _wrapped = true;
        }

        /// <summary></summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public double GetTime(int entry) => 
            _time[(_writePointer - 1 - entry) & SamplesMask];

        /// <summary></summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public PinState GetState(int entry) =>
            _pins[(_writePointer - 1 - entry) & SamplesMask];

        /// <summary></summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool Overflow(int entry) =>
            entry >= MaximumSamples || entry >= _writePointer && !_wrapped;

        /// <summary></summary>
        /// <param name="graph"></param>
        /// <param name="top"></param>
        /// <param name="left"></param>
        /// <param name="width"></param>
        /// <param name="minTime"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// @version v.22.06.01 - Added exception and changed local variable
        /// name to clarify it.
        public override int Draw(Graphics graph, int top, float left, float width, double minTime, double scale)
        {
            float height = Height * 2.0f / 6.0f;
            float center = top + Height / 2.0f;
            float previousX = width + left;
            float previousY = center;
            float nextX;
            int entry = 0;
            do
            {
                nextX = (float)((GetTime(entry) - minTime) / scale);
                Pen color;
                float nextY;
                switch (GetState(entry))
                {
                    case PinState.INPUT_HI:
                        nextY = center - height;
                        color = Pens.LightBlue;
                        break;
                    case PinState.INPUT_LO:
                        nextY = center + height;
                        color = Pens.LightBlue;
                        break;
                    case PinState.OUTPUT_HI:
                        nextY = center - height;
                        color = Pens.Red;
                        break;
                    case PinState.OUTPUT_LO:
                        nextY = center + height;
                        color = Pens.Red;
                        break;
                    case PinState.FLOATING:
                    default:
                        nextY = center;
                        color = Pens.Yellow;
                        break;
                }
                nextX = nextX * width + left;
                if (nextX < left)
                    nextX = left;
                if (graph == null)
                    throw new ArgumentNullException(nameof(graph));
                graph.DrawLine(color, previousX, nextY, nextX, nextY);
                graph.DrawLine(color, previousX, previousY, previousX, nextY);
                previousX = nextX;
                previousY = nextY;
                entry++;
            } while (!Overflow(entry) && nextX > left);

            return Height;
        }
    }
}
