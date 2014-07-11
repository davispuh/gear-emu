/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Gear.EmulationCore;

namespace Gear.GUI.LogicProbe
{
    class LogicAnalog : LogicRow
    {
        private LogicDigital[] Channels;

        public override string Name
        {
            get
            {
                string s = "[";
                foreach (LogicDigital r in Channels)
                {
                    s += r.Name + ",\n";
                }
                return s.Substring(0, s.Length - 2) + "]";
            }
        }

        public override int Height
        {
            get { return 64; }
        }

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

        public LogicAnalog(LogicDigital[] rows)
        {
            Channels = rows;
        }

        public override void Click()
        {
        }

        // ASB: new method to clear samples when reset is needed
        public override void Reset()
        {
            foreach (LogicDigital r in Channels)
            {
                r.Reset();
            }
        }

        public override int Draw(System.Drawing.Graphics g, int top, float left, float width, double minTime, double scale)
        {
            float height = Height * 3 / 4;
            float bottom = top + Height;

            float previousX = width + left;
            float previousY = bottom;
            float nextX;
            float nextY;
            int[] index = new int[Channels.Length];

            for (int i = 0; i < Channels.Length; i++)
                index[i] = 0;

            do
            {
                double time = Channels[0].GetTime(index[0]);
                int seekindex = 0;

                for (int i = 1; i < index.Length; i++)
                {
                    if (Channels[i].GetTime(index[i]) > time)
                    {
                        time = Channels[i].GetTime(index[i]);
                        seekindex = i;
                    }
                }

                int outputLevel = 0;

                for (int i = 0, divider = 2; i < Channels.Length; i++, divider *= 2)
                    if (Channels[i].GetState(index[i]) == PinState.OUTPUT_HI)
                        outputLevel += 1 << i;

                nextY = bottom - height * (outputLevel + 1) / (float)((1 << Channels.Length) + 2);
                nextX = (float)((time - minTime) / scale) * width + left;

                if (nextX < left)
                    nextX = left;

                g.DrawLine(Pens.Magenta, previousX, nextY, nextX, nextY);
                g.DrawLine(Pens.Magenta, previousX, previousY, previousX, nextY);

                previousX = nextX;
                previousY = nextY;

                for (int i = 0; i < index.Length; i++)
                {
                    if (Channels[i].GetTime(index[i]) == time)
                        index[i]++;
                }

                if (Channels[seekindex].Overflow(index[seekindex]))
                    break;
            } while (nextX > left);

            return Height;
        }
    }
}
