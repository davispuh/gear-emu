/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * RememberRTBoxPosition.cs
 * Remember and restore displayed position and insert point for a RichTextBox.
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
using System.Drawing;
using System.Windows.Forms;

namespace Gear.Utils
{
    /// @brief Remember and restore displayed position and insert 
    /// point (aka caret in MS documentation) for a RichTextBox.
    /// @details Based on code from https://stackoverflow.com/a/20746931/10200101
    class RememberRTBoxPosition
    {
        /// @brief Object reference.
        private readonly RichTextBox obj;

        /// @brief Store first displayed character.
        private int start;

        /// @brief Store last displayed character.
        private int end;

        /// @brief Store cursor position.
        private int cursor_position;

        /// @brief Store cursor lenght
        private int cursor_lenght;

        /// @brief Default constructor, remembers current position of 
        ///  text displayed and selection.
        /// @param textBox RichTextBox object to remember and restore state.
        public RememberRTBoxPosition(RichTextBox textBox)
        {
            if (textBox != null)
            {
                obj = textBox;
                RememberNewPosition();
            }
            else
                throw new ArgumentNullException(nameof(textBox));
        }

        /// @brief Remember position of text displayed and current selection.
        public void RememberNewPosition()
        {
            // Get first and last displayed character
            start = obj.GetCharIndexFromPosition(new Point(0, 0));
            end = obj.GetCharIndexFromPosition(new Point(obj.ClientSize.Width, obj.ClientSize.Height));
            // Save cursor position
            cursor_position = obj.SelectionStart;
            cursor_lenght = obj.SelectionLength;
        }

        /// @brief Restore position of text displayed and last properties
        /// of selection.
        public void RestorePosition()
        {
            // Scroll to the last character and then to the first + line width
            obj.SelectionLength = 0;
            obj.SelectionStart = end;
            obj.ScrollToCaret();
            obj.SelectionStart = 
                start + obj.Lines[obj.GetLineFromCharIndex(start)].Length + 1;
            obj.ScrollToCaret();

            // Finally, set cursor to original position
            obj.SelectionStart = cursor_position;
            obj.SelectionLength = cursor_lenght;
        }
    }
}
