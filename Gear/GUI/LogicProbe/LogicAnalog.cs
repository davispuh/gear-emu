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
    /// @brief Provides a analog channel for LogicView.
    class LogicAnalog : LogicRow
    {
        /// <summary></summary>
        private readonly LogicDigital[] Channels;

        /// <summary></summary>
        public override string Name
        {
            get
            {
                string s = "[";
                foreach (LogicDigital r in Channels)
                    s += r.Name + ",\n";
                return s.Substring(0, s.Length - 2) + "]";
            }
        }

        /// <summary></summary>
        public override int Height => 64;

        /// <summary></summary>
        public override double MinTime
        {
            get
            {
                double minimum = Channels[0].MinTime;

                for (int i = 1; i < Channels.Length; i++)
                {
                    if (Channels[i].MinTime > minimum)
                        minimum = Channels[i].MinTime;
                }

                return minimum;
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="rows"></param>
        public LogicAnalog(LogicDigital[] rows)
        {
            Channels = rows;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Click() { }

        /// @brief Clear samples when reset is needed.
        public override void Reset()
        {
            foreach (LogicDigital channel in Channels)
                channel.Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="top"></param>
        /// <param name="left"></param>
        /// <param name="width"></param>
        /// <param name="minTime"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        /// @version v22.03.02 - Bugfix on comparisons of
        /// <c>_channels[i].GetTime(index[i]) >= time</c>
        public override int Draw(Graphics graph, int top, float left, float width,
            double minTime, double scale)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            float height = Height * 3f / 4f;
            float bottom = top + Height;

            float previousX = width + left;
            float previousY = bottom;
            float nextX;
            int[] index = new int[Channels.Length];  //automatically set to 0 all elements

            do
            {
                double time = Channels[0].GetTime(index[0]);
                int seekindex = 0;

                for (int i = 1; i < index.Length; i++)
                    if (Channels[i].GetTime(index[i]) >= time)  //Corrected comparison for double
                    {
                        time = Channels[i].GetTime(index[i]);
                        seekindex = i;
                    }

                int outputLevel = 0;

                for (int i = 0, divider = 2; i < Channels.Length; i++, divider *= 2)
                    if (Channels[i].GetState(index[i]) == PinState.OUTPUT_HI)
                        outputLevel += 1 << i;

                float nextY = bottom - height * (outputLevel + 1f) / ((1 << Channels.Length) + 2f);
                nextX = (float)((time - minTime) / scale) * width + left;

                if (nextX < left)
                    nextX = left;

                graph.DrawLine(Pens.Magenta, previousX, nextY, nextX, nextY);
                graph.DrawLine(Pens.Magenta, previousX, previousY, previousX, nextY);

                previousX = nextX;
                previousY = nextY;

                for (int i = 0; i < index.Length; i++)
                    if (Channels[i].GetTime(index[i]) >= time)  //Corrected comparison for double
                        index[i]++;

                if (Channels[seekindex].Overflow(index[seekindex]))
                    break;
            } while (nextX > left);

            return Height;
        }
    }
}
