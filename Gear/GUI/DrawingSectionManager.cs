/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * DrawingSectionManager.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Gear.Utils;

namespace Gear.GUI
{
    /// <summary>Manage drawing sections areas based on a Enum list of
    /// values.</summary>
    /// <remarks>Requirements for Enum values:<br/>
    /// - A sub range of values of the enum can be used for first and last
    /// values.<br/>
    /// - Duplicated values should be avoided.</remarks>
    /// <typeparam name="TEnum">Enum type associated to drawing
    /// sections.</typeparam>
    /// @version v22.09.02 - Added.
    [DebuggerDisplay("{TextForDebugger,nq}")]
    public class DrawingSectionManager<TEnum> where TEnum : Enum
    {
        /// <summary>Delegate pattern for method that calculate rectangle
        /// area for each section.</summary>
        /// <param name="drawingSection">Enum value to calculate area.</param>
        /// <returns>Coordinates of rectangle area.</returns>
        public delegate Rectangle RectangleFromSectionDelegate(TEnum drawingSection);

        /// <summary>Reference to method following delegate pattern.</summary>
        private readonly RectangleFromSectionDelegate _rectangleGenerator;

        /// <summary>Dictionary between drawing section and corresponding
        /// node.</summary>
        private readonly ConcurrentDictionary<TEnum, SectionNode<TEnum> > _sectionDictionary =
            new ConcurrentDictionary<TEnum, SectionNode<TEnum>>();

        /// <summary>Returns a summary text of this class, to be used in
        /// debugger view.</summary>
        private string TextForDebugger =>
            $"{{{nameof(DrawingSectionManager<TEnum>)}, Dictionary count: {_sectionDictionary.Count}.}}";

        /// <summary>Default constructor.</summary>
        /// <remarks>IComparable interface Reference :
        /// https://learn.microsoft.com/en-us/dotnet/api/system.icomparable?view=netframework-4.7.2 </remarks>
        /// <param name="rectangleGenerator">Delegate Method to calculate rectangle
        /// area for each drawing section.</param>
        /// <param name="firstSection">First value of included drawing section.</param>
        /// <param name="lastSection">Last value of included drawing section.</param>
        /// <exception cref="ArgumentException">Value given as parameter
        /// <paramref name="firstSection"/> or <paramref name="lastSection"/>
        /// is not valid on enum, or <paramref name="firstSection"/> value is
        /// greater than <paramref name="lastSection"/> value in enum order.</exception>
        public DrawingSectionManager(
            RectangleFromSectionDelegate rectangleGenerator,
            TEnum firstSection, TEnum lastSection)
        {
            _rectangleGenerator = rectangleGenerator;
            //check valid start and end values
            if (!Enum.IsDefined(typeof(TEnum), firstSection))
                throw new ArgumentException(
                    GetValueNotValidErrorText(firstSection, nameof(TEnum)),
                    nameof(firstSection));
            if (!Enum.IsDefined(typeof(TEnum), lastSection))
                throw new ArgumentException(
                    GetValueNotValidErrorText(lastSection, nameof(TEnum)),
                    nameof(firstSection));
            //check relative order of firstSection < lastSection
            if (firstSection.CompareTo(lastSection) >= 0)
            {
                string msg =
                    $"Value '{firstSection}' of parameter {nameof(firstSection)} is greater or equal than value '{lastSection}' of parameter {nameof(lastSection)}. Must be smaller.";
                throw new ArgumentException(msg, nameof(lastSection));
            }
            //create items for every enum value on range
            bool rollOver = false;
            for (TEnum section = firstSection;
                 section.CompareTo(lastSection) <= 0 && !rollOver; //section must precede or be equal to lastSection
                 section = section.EnumNext(out rollOver))
                //don't insert duplicate values
                if (!_sectionDictionary.ContainsKey(section))
                    _sectionDictionary[section] = new SectionNode<TEnum>(section);
        }

        /// <summary>Error text for an invalid enum <paramref name="value"/>.</summary>
        /// <param name="value">Invalid enum <paramref name="value"/>.</param>
        /// <param name="enumName">Name of enum class.</param>
        /// <returns>Error text.</returns>
        private static string GetValueNotValidErrorText(TEnum value, string enumName) =>
            $"Value '{value}' is not a defined value of {enumName} enumeration. Check values passed to {nameof(DrawingSectionManager<TEnum>)}() constructor.";

        /// <summary>Error text for an invalid enum <paramref name="value"/>.</summary>
        /// <param name="value">Enum <paramref name="value"/> not contained in range.</param>
        /// <returns>Error text.</returns>
        private static string GetValueNotContainedErrorText(TEnum value) =>
            $"Error: Value '{value}' is not contained on {nameof(_sectionDictionary)}. Check values passed to {nameof(DrawingSectionManager<TEnum>)}() constructor.";

        /// <summary>Retrieve rectangle area associated to section.</summary>
        /// <param name="section">Drawing section enum value.</param>
        /// <returns>Rectangle with coordinates delimiting the related
        /// drawing area.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="section"/>
        /// is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Value given as parameter
        /// <paramref name="section"/> is not valid on Enum, or
        /// <paramref name="section"/> is not found in internal dictionary.</exception>
        public Rectangle GetRectangle(TEnum section)
        {
            if (section == null)
                throw new ArgumentNullException(nameof(section));
            //check valid value of parameter
            if (!Enum.IsDefined(typeof(TEnum), section))
                throw new ArgumentException(
                    GetValueNotValidErrorText(section, nameof(TEnum)),
                    nameof(section));
            //get value if exists or throw exception
            if (_sectionDictionary.ContainsKey(section))
                return _sectionDictionary[section].DrawingRectangle;
            throw new ArgumentException(
                GetValueNotContainedErrorText(section),
                nameof(section));
        }

        /// <summary>Re-calculate areas for all registered drawing sections.</summary>
        public void Reset()
        {
            foreach (KeyValuePair<TEnum, SectionNode<TEnum>> valuePair in _sectionDictionary)
            {
                try
                {
                    SectionNode<TEnum> sectionNode = valuePair.Value;
                    //invoke delegate to recalculate rectangle area
                    Rectangle rectangle = _rectangleGenerator(valuePair.Key);
                    if (rectangle == sectionNode.DrawingRectangle)
                        continue;
                    //assign new rectangle area only in changes
                    valuePair.Value.DrawingRectangle = rectangle;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    MessageBox.Show(
                        e.Message,
                        @"Error Resetting Drawing Sections",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                // ReSharper disable once CatchAllClause
                catch (Exception e)
                {
                    MessageBox.Show(
                        e.Message,
                        $@"Error invoking '{nameof(_rectangleGenerator)}' delegate to recalculate Drawing Sections",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>Node with data associated to a section enum value.</summary>
        /// <typeparam name="TEnum1">Enum type associated to drawing
        /// sections.</typeparam>
        /// @version v22.09.02 - Added.
        [DebuggerDisplay("{TextForDebugger,nq}")]
        private class SectionNode<TEnum1> where TEnum1 : Enum
        {
            /// <summary>Enum value for drawing section to use.</summary>
            private TEnum1 DrawingSection { get; }

            /// <summary>Rectangle area associated to section.</summary>
            public Rectangle DrawingRectangle { get; set; }

            /// <summary>Returns a summary text of this class, to be used in
            /// debugger view.</summary>
            private string TextForDebugger =>
                $"{{{nameof(SectionNode<TEnum1>)}, Enum value: '{DrawingSection}', Rectangle: {DrawingRectangle}.}}";

            /// <summary>Default constructor.</summary>
            /// <param name="drawingSection">Enum value for drawing section
            /// to use.</param>
            public SectionNode(TEnum1 drawingSection)
            {
                DrawingSection = drawingSection;
                DrawingRectangle = Rectangle.Empty;
            }
        }
    }
}
