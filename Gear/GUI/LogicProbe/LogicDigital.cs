/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
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

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Gear.EmulationCore;

namespace Gear.GUI.LogicProbe
{
    public class LogicDigital : LogicRow
    {
        const int MAXIMUM_SAMPLES = 1024;

        private double[] Time;
        private PinState[] Pins;
        private int PinNumber;

        private int WritePointer;
        private bool Wrapped;

        public override double MinTime
        {
            get
            {
                if (Wrapped)
                    return Time[WritePointer];
                else
                    return Time[0];
            }
        }

        public override int Height
        {
            get { return 16; }
        }

        public override string Name
        {
            get { return "P" + PinNumber.ToString(); }
        }

        public LogicDigital(int pin)
        {
            Pins = new PinState[MAXIMUM_SAMPLES];
            Time = new double[MAXIMUM_SAMPLES];
            Wrapped = false;
            PinNumber = pin;

            // ASB: use the new method Reset()
            Reset();
        }

        public override void Click()
        {
        }

        // ASB: new method to clear samples when reset is needed
        public override void Reset()
        {
            Time[0] = 0.0;
            Pins[0] = PinState.FLOATING;
            WritePointer = 1;
        }

        public void Update(PinState pin, double time)
        {
            if (Pins[(WritePointer - 1) & (MAXIMUM_SAMPLES - 1)] == pin)
                return;

            Pins[WritePointer] = pin;
            Time[WritePointer] = time;

            WritePointer = (WritePointer + 1) & (MAXIMUM_SAMPLES - 1);

            if (WritePointer == 0)
                Wrapped = true;
        }

        public double GetTime(int entry)
        {
            return Time[(WritePointer - 1 - entry) & (MAXIMUM_SAMPLES - 1)];
        }

        public PinState GetState(int entry)
        {
            return Pins[(WritePointer - 1 - entry) & (MAXIMUM_SAMPLES - 1)];
        }

        public bool Overflow(int entry)
        {
            if (entry >= MAXIMUM_SAMPLES)
                return true;

            else if (entry >= WritePointer && !Wrapped)
                return true;

            return false;
        }

        public override int Draw(Graphics g, int top, float left, float width, double minTime, double scale)
        {
            float height = Height * 2 / 6;
            float center = top + Height / 2;

            Pen color;
            float previousX = width + left;
            float previousY = center;
            float nextX;
            float nextY;
            int s = 0;

            do
            {
                nextX = (float)((GetTime(s) - minTime) / scale);
                nextY = center;

                switch (GetState(s))
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
                    default:
                        nextY = center;
                        color = Pens.Yellow;
                        break;
                }

                nextX = nextX * width + left;

                if (nextX < left)
                    nextX = left;

                g.DrawLine(color, previousX, nextY, nextX, nextY);
                g.DrawLine(color, previousX, previousY, previousX, nextY);

                previousX = nextX;
                previousY = nextY;

                s++;
            } while (!Overflow(s) && nextX > left);

            return Height;
        }
    }
}
