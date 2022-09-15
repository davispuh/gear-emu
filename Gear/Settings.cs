﻿/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * Settings.cs
 * --------------------------------------------------------------------------------
 *
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

// ReSharper disable CommentTypo
// ReSharper disable InvalidXmlDocComment

/// <summary>Transversal %Properties for the program.</summary>
namespace Gear.Properties
{
    /// <summary>Controls events for configuration class.</summary>

    // Esta clase le permite controlar eventos específicos en la clase de configuración:
    //  El evento SettingChanging se desencadena antes de cambiar un valor de configuración.
    //  El evento PropertyChanged se desencadena después de cambiar el valor de configuración.
    //  El evento SettingsLoaded se desencadena después de cargar los valores de configuración.
    //  El evento SettingsSaving se desencadena antes de guardar los valores de configuración.
    public sealed partial class Settings {

        /// <summary>Default constructor.</summary>
        public Settings() {
            //Para agregar los controladores de eventos para guardar y cambiar la configuración, quite la marca de comentario de las líneas:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Agregar código para administrar aquí el evento SettingChangingEvent.
        }

        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Agregar código para administrar aquí el evento SettingsSaving.
        }

        //This DOXYGEN documentation goes here, because Settings.Designer.cs
        //is auto generated by MSVS and the documentation will be lost if
        //put there.

        /// @property LastTickMarkGrid
        /// <summary>Last value used as grid separation in the logic view
        /// plugin.</summary>
        /// @version v15.03.26 - Added as a User property of the program.

        /// @property LastTimeFrame
        /// <summary>Last value used as a width in the logic view plugin.</summary>
        /// @version v15.03.26 - Added as a User property of the program.

        /// @property LogicViewTimeUnit
        /// <summary>Unit of measure for elapsed time label in logic probe.</summary>
        /// @version v20.09.01 - Added as a User property of the program.

        /// @property EmbeddedCode
        /// <summary>Determine if the code for the plugin is embedded in
        /// the XML file or resides on a separate file, on Plugin Editor.</summary>
        /// @version v15.03.26 - Added as a User property of the program.

        /// @property LastPlugin
        /// <summary>Last plugin successfully opened or saved on Plugin
        /// Editor.</summary>
        /// <remarks>Include complete path and name.</remarks>
        /// @version v15.03.26 - Added as a User property of the program.

        /// @property TabSize
        /// <summary>Tabulator size in characters, from Plugin Editor.</summary>
        /// @version v20.09.01 - Added as a User property of the program.

        /// @property FreqFormat
        /// <summary>Format of frequency labels in %GUI.</summary>
        /// @version v20.06.01 - Added as a User property of the program.

        /// @property HubTimeUnit
        /// <summary>Unit of measure for elapsed time label in Hub view.</summary>
        /// @version v20.06.01 - Added as a User property of the program.

        /// @property LastBinary
        /// <summary>Last binary file successfully opened.</summary>
        /// <remarks>Include complete path and name.</remarks>
        /// @version v15.03.26 - Added as a User property of the program.

        /// @property UpdateEachSteps
        /// <summary>Number of steps before update the windows and tabs.</summary>
        /// @version v15.03.26 - Added as a User property of the program.

        /// @property TabSize
        /// <summary>Tabulator size in characters.</summary>
        /// @version v20.08.01 - Added as a User property of the program.

        /// @property UseAnimations
        /// <summary>Use animations on splitters.</summary>
        /// @version v22.06.01 - Added as a User property of the program.

        /// @property ValuesShownAsHex
        /// <summary>Values shown as hexadecimals as default initial setting
        /// on Cog View.</summary>
        /// @version v22.09.01 - Added as a User property of the program.

        /// @propert PCHighlightedType
        /// <summary>Type of Highlight for line now executing.</summary>
        /// @version v22.09.01 - Added as a User property of the program.
    }
}
