/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2014 - Antonio Sanhueza
 * --------------------------------------------------------------------------------
 * PluginVersion.cs
 * Version attribute class for emulator plugins
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

namespace Gear.PluginSupport
{
    /// @brief Attribute class for validity check for plugin base members.
    /// @version 14.8.5 - Added.
    /// <summary>
    /// Attribute class for validity check for plugin base members.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, 
        AllowMultiple = false, Inherited = true)]
    public class VersionAttribute : Attribute
    {
        /// @brief Type of member for version management
        public enum memberTypeVersion
        {
            TickHandler,    //notifies tick changes 
            PinHandler      //notifies pin changes
        }
        
        /// @brief Lower limit to validity.
        private float _verFrom;
        /// @brief Upper limit to validity. Maximun limit could be +infinity.
        private float _verTo;
        /// @brief Include lower value (=true) or excludes (=false).
        private bool _includeLower;
        /// @brief Include upper value (=true) or excludes (=false).
        private bool _includeUpper;
        /// @brief Type of member to versioning.
        private memberTypeVersion _memberType;

        /// @brief Constructor with lower limit from a value (included).
        /// Assumed upper or equal from this value up to +infinity.
        /// @param[in] versionFrom Lower limit for valid version.
        /// <summary>
        /// Constructor with lower limit from a value (included).
        /// Assumed upper or equal from this value up to +infinity.
        /// </summary>
        /// <param name="versionFrom">Lower limit for valid version.</param>
        public VersionAttribute(float versionFrom)
        {
            _verFrom = versionFrom;
            _verTo = Single.MaxValue;
            _includeLower = true;
            _includeUpper = true;
        }
        /// @brief Constructor for validity between two values.
        /// Assumed upper or equal from lower limit and lesser than upper limit.
        /// @param[in] versionFrom Lower limit for valid version (included).
        /// @param[in] versionTo Upper limit for valid version (not included).
        /// <summary>
        /// Constructor for validity between two values.
        /// Assumed upper or equal from lower limit and lesser than upper limit.
        /// </summary>
        /// <param name="versionFrom">Lower limit for valid version (included).</param>
        /// <param name="versionTo">Upper limit for valid version (not included).</param>
        public VersionAttribute(float versionFrom, float versionTo)
        {
            _verFrom = versionFrom;
            _verTo = versionTo;
            _includeLower = true;
            _includeUpper = false;
        }

        /// @brief Getter to include lower limit o not.
        /// <summary>
        /// Getter to include lower limit o not.
        /// </summary>
        public float VersionFrom { get { return _verFrom; } }
        /// @brief Getter to include upper limit o not.
        /// <summary>
        /// Getter to include upper limit o not.
        /// </summary>
        
        public float VersionTo { get { return _verTo; } }

        /// @brief Property to include or not the lower limit on validity.
        /// <summary>
        /// Property to include or not the lower limit on validity.
        /// </summary>
        public bool IncludeLower
        {
            get { return _includeLower; }
            set { _includeLower = value; }
        }
        
        /// @brief Property to include or not the upper limit on validity.
        /// <summary>
        /// Property to include or not the upper limit on validity.
        /// </summary>
        public bool IncludeUpper
        {
            get { return _includeUpper; }
            set { _includeUpper = value; }
        }

        /// @brief Property to set type of member to versioning.
        /// <summary>
        /// Property to set type of member to versioning.
        /// </summary>
        public memberTypeVersion MemberType
        {
            get { return _memberType; }
            set { _memberType = value; }
        }

        /// @brief Validate if atributte is valid beetween lower and upper limits.
        /// @param[in] version Version number to test validity.
        /// @returns True if between limits, false if not.
        /// <summary>
        /// Validate if atributte is valid beetween lower and upper limits.
        /// </summary>
        /// <param name="version">Version number to test validity.</param>
        /// <returns>True if between limits, false if not.</returns>
        public bool ValidOnVersion(float version)
        {
            return (
                (_includeLower ? (_verFrom <= version) : (_verFrom < version)) &
                (_includeUpper ? (version <= _verTo) : (version < _verTo))
            );
        }
    }

    /// @brief Manages Versions of plugins members.
    /// 
    class Pepito
    {
        /// @brief Pointer to plugin.
        private PluginBase _plugin;
        /// @brief Type of member to select.
        private VersionAttribute.memberTypeVersion _memType;
        ///
        private float _version;

        /// @brief Constructor with member type specification.
        public Pepito(VersionAttribute.memberTypeVersion memberType)
        {
            _plugin = null;
            _memType = memberType;
            _version = 0.0f;
        }

        /// @brief Attribute to hold target version
        public float Version
        {
            get { return _version; }
            set { _version = value; }
        }

        /// @brief Add a plugin to Pepito.
        /// @param plugin Plugin reference to add.
        public void Add(PluginBase plugin) { }

        /// @brief Remove a plugin from Pepito.
        /// @param plugin Plugin reference to remove.
        public void Remove(PluginBase plugin) { }

        /// @brief Get member by type and version.
        public PluginBase GetMember(VersionAttribute.memberTypeVersion member)
        {
            //TODO [ASB] : agregar lógica para determinar el tipo de miembro según versión, y ejecutarlo
            switch (member)
            {
                case VersionAttribute.memberTypeVersion.PinHandler:

                    break;
                case VersionAttribute.memberTypeVersion.TickHandler:

                    break;
            };

            return _plugin;
        }
    }

}