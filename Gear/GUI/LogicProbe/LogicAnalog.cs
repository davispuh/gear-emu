/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * LogicAnalog.cs
 * Provides a analog channel for LogicView
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
    /// <summary>Provides a analog channel for LogicView.</summary>
    public class LogicAnalog : LogicRow
    {
        /// <summary></summary>
        private readonly LogicDigital[] _channels;

        /// <summary>Name of channel.</summary>
        public override string Name
        {
            get
            {
                string name = "[";
                foreach (LogicDigital channel in _channels)
                    name += channel.Name + ",\n";
                return name.Substring(0, name.Length - 2) + "]";
            }
        }

        /// <summary>Height this channel on screen.</summary>
        public override int Height => 64;

        /// <summary></summary>
        /// @todo Parallelism [complex:low, cycles:_channels.Length ~low] point in loop of LogicAnalog._channels[]
        public override double MinTime
        {
            get
            {
                double minimum = _channels[0].MinTime;
                for (int i = 1; i < _channels.Length; i++) //TODO Parallelism [complex:low, cycles:_channels.Length ~low] point in loop _channels[]
                    if (_channels[i].MinTime > minimum)
                        minimum = _channels[i].MinTime;
                return minimum;
            }
        }

        /// <summary>Default constructor.</summary>
        /// <param name="rows">Number of channels.</param>
        public LogicAnalog(LogicDigital[] rows)
        {
            _channels = rows;
        }

        /// <summary></summary>
        public override void Click() { }

        /// <summary>Clear samples when reset is needed.</summary>
        /// @todo Parallelism [complex:low, cycles:_channels.Length ~low] point in loop of LogicAnalog._channels[]
        public override void Reset()
        {
            foreach (LogicDigital channel in _channels)  //TODO Parallelism [complex:low, cycles:_channels.Length ~low] point in loop _channels[]
                channel.Reset();
        }

        /// <summary></summary>
        /// <param name="graph"></param>
        /// <param name="top"></param>
        /// <param name="left"></param>
        /// <param name="width"></param>
        /// <param name="minTime"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// @version v22.03.02 - Bugfix on comparisons of
        /// <c>_channels[i].GetTime(index[i]) >= time</c>
        /// @todo Parallelism [complex:low, cycles:_channels.Length ~low] point in 3 loops of index[i]
        public override int Draw(Graphics graph, int top, float left, float width,
            double minTime, double scale)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            float height = Height * 3.0f / 4.0f;
            float bottom = top + Height;
            float previousX = width + left;
            float previousY = bottom;
            float nextX;
            int[] index = new int[_channels.Length];  //automatically set to 0 all elements

            do
            {
                double time = _channels[0].GetTime(index[0]);
                int seekIndex = 0;
                for (int i = 1; i < index.Length; i++)  //TODO Parallelism [complex:low, cycles:_channels.Length ~low] point in loop _channels[]
                    if (_channels[i].GetTime(index[i]) >= time)  //Corrected comparison for double
                    {
                        time = _channels[i].GetTime(index[i]);
                        seekIndex = i;
                    }
                int outputLevel = 0;
                for (int i = 0; i < _channels.Length; i++)  //TODO Parallelism [complex:low, cycles:_channels.Length ~low] point in loop _channels[]
                    if (_channels[i].GetState(index[i]) == PinState.OUTPUT_HI)
                        outputLevel += 1 << i;
                float nextY = bottom - height * (outputLevel + 1f) / ((1 << _channels.Length) + 2f);
                nextX = (float)((time - minTime) / scale) * width + left;
                if (nextX < left)
                    nextX = left;
                graph.DrawLine(Pens.Magenta, previousX, nextY, nextX, nextY);
                graph.DrawLine(Pens.Magenta, previousX, previousY, previousX, nextY);
                previousX = nextX;
                previousY = nextY;
                for (int i = 0; i < index.Length; i++)  //TODO Parallelism [complex:low, cycles:_channels.Length ~low] point in loop _channels[]
                    if (_channels[i].GetTime(index[i]) >= time)  //Corrected comparison for double
                        index[i]++;
                if (_channels[seekIndex].Overflow(index[seekIndex]))
                    break;
            } while (nextX > left);
            return Height;
        }
    }
}
