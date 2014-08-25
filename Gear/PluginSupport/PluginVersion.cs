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
using System.Collections.Generic;
using System.Reflection;

using Gear.EmulationCore; 

namespace Gear.PluginSupport
{
    /// @brief Versioning support for members of PluginBase.
    /// @details This class have the definitions of which methods of PluginBase will be 
    /// versionated, e.g. that have different parameters by the same method name and are 
    /// controlled by a version number (ex. 0.0, 1.0, 1.1, etc.).
    /// @remark Each time a variant is generated, the developer have to modify this class:
    /// @par
    /// <b>Case 1 - new method to versionate not managed before :</b> add a 
    /// PluginVersioning.memberType enumeration value for a new method of PluginBase to versionate,
    /// and a PluginVersioning.versionatedMember enumeration value for each version added. Then 
    /// apply the VersionAttribute attribute to the method with the new 
    /// PluginVersioning.memberType enumeration value added and appropriate version number 
    /// (ex. 0.0 if is a new method).
    /// @par
    /// <b> Case 2 - new version of a method already managed :</b> add only a new 
    /// PluginVersioning.versionatedMember enumeration value based on the corresponding 
    /// PluginVersioning.memberType enumeration value that exists. Then apply the VersionAttribute 
    /// attribute to the method with the same PluginVersioning.memberType enumeration value and 
    /// appropriate version number.
    /// @version 14.8.7 - Added.
    public class PluginVersioning
    {
        /// @brief Type of member for version management on menbers of Plugins.
        /// @details They must be equal to the name of the method to work, because the retrieving
        /// mechanism use reflection based on names of PluginBase methods to versioning.
        public enum memberType
        {
            none    = -1,   //!< None
            OnClock = 0,    //!< Run on clock ticks.
            OnPinChange,    //!< Run on pin changes.
            PresentChip     //!< Prepare the notifiers.
        }

        static public Dictionary<memberType, VersionatedContainer> ManagedVersions;

        /// @brief Static default constructor
        static PluginVersioning()
        {
            //TODO [ASB] : change this declaration for a dynamic evaluation with reflexion class
            ManagedVersions = new Dictionary<memberType, VersionatedContainer> ();
            ManagedVersions.Add(memberType.none, null);
            ManagedVersions.Add(memberType.OnClock,
                new VersionatedContainer(memberType.OnClock, 0.0f, typeof(OnClockV0_0)));
            ManagedVersions.Add(memberType.OnClock,
                new VersionatedContainer(memberType.OnClock, 1.0f, typeof(OnClockV1_0)));
            ManagedVersions.Add(memberType.OnPinChange,
                new VersionatedContainer(memberType.OnPinChange, 0.0f, typeof(OnPinChangeV0_0)));
            ManagedVersions.Add(memberType.PresentChip,
                new VersionatedContainer(memberType.PresentChip, 0.0f, typeof(PresentChipV0_0)));
            ManagedVersions.Add(memberType.PresentChip,
                new VersionatedContainer(memberType.PresentChip, 1.0f, typeof(PresentChipV1_0)));
        }

        //=======================================================================
        //Declare in this section delegates for each version of members to manage.
        #region Delegates for each versionated member of PluginBase.

        /// @brief Delegate for version 0.0 for OnClock.
        [VersionAttribute(0.0f, 1.0f, PluginVersioning.memberType.OnClock)]
        public delegate void OnClockV0_0(double time);

        /// @brief Delegate for version 1.0 for OnClock.
        [Version(1.0f, PluginVersioning.memberType.OnClock)]
        public delegate void OnClockV1_0(double time, uint sysCounter);

        /// @brief Delegate for version 0.0 for OnPinChange.
        [VersionAttribute(0.0f,PluginVersioning.memberType.OnPinChange)]
        public delegate void OnPinChangeV0_0(double time);

        /// @brief Delegate for version 0.0 for PresentChip.
        [VersionAttribute(0.0f, 1.0f, PluginVersioning.memberType.PresentChip)]
        public delegate void PresentChipV0_0(PropellerCPU host);

        /// @brief Delegate for version 1.0 for PresentChip.
        [Version(1.0f, PluginVersioning.memberType.PresentChip)]
        public delegate void PresentChipV1_0();

        //=======================================================================
        #endregion
        


    }

    /// @brief Versioning range.
    public class VersRange
    {
        /// @brief Lower limit to validity.
        private float _verFrom;
        /// @brief Upper limit to validity. Maximun limit could be +infinity.
        private float _verTo;
        /// @brief Include lower value (=true) or excludes (=false).
        private bool _includeLower;
        /// @brief Include upper value (=true) or excludes (=false).
        private bool _includeUpper;

        /// @brief Constructor with lower limit from a value (included).
        /// Assumed upper or equal from this value up to +infinity.
        /// @param versionFrom Lower limit for valid version.
        public VersRange(float versionFrom)
        {
            ///TODO [ASB] : throw exception if versionXXXX is out of range, ex. lower than 0.0
            _verFrom = versionFrom;
            _verTo = Single.MaxValue;
            _includeLower = true;
            _includeUpper = true;
        }
        /// @brief Constructor for validity between two values.
        /// Assumed upper or equal from lower limit and lesser than upper limit.
        /// @param[in] versionFrom Lower limit for valid version (included).
        /// @param[in] versionTo Upper limit for valid version (not included).
        public VersRange(float versionFrom, float versionTo)
        {
            ///TODO [ASB] : throw exception if versionXXXX is out of range, ex. lower than 0.0
            _verFrom = versionFrom;
            _verTo = versionTo;
            _includeLower = true;
            _includeUpper = false;
        }

        public float LowerLimit
        {
            get { return _verFrom; }    
        }   

        // @brief Getter to include lower limit o not.
        //public float VersionFrom { get { return _verFrom; } }
        // @brief Getter to include upper limit o not.
        //public float VersionTo { get { return _verTo; } }
        /// @brief Property to include or not the lower limit on validity.
        public bool IncludeLower
        {
            get { return _includeLower; }
            set { _includeLower = value; }
        }
        /// @brief Property to include or not the upper limit on validity.
        public bool IncludeUpper
        {
            get { return _includeUpper; }
            set { _includeUpper = value; }
        }

        /// @brief Validate if atributte is valid beetween lower and upper limits.
        /// @param[in] version Version number to test validity.
        /// @returns True if between limits, false if not.
        public bool ValidOn(float version)
        {
            return (
                (_includeLower ? (_verFrom <= version) : (_verFrom < version)) &
                (_includeUpper ? (version <= _verTo) : (version < _verTo))
            );
        }

    }

    /// @brief Attribute class for manage versioning for members of PluginBase.
    /// @details To be applied as attribute to members to have dinamic manage of versions.
    /// @version 14.8.5 - Added.
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Delegate, 
        AllowMultiple = false, Inherited = true)]
    public class VersionAttribute : Attribute
    {
        
        /// @brief Range of versioning.
        private VersRange _range;
        /// @brief Type of member to versioning.
        private PluginVersioning.memberType _memberType;
        /// @brief Code of a versionated member.
        //private PluginVersioning.versionatedMember _versionatedMember;

        #region Constructor for class VersionAttribute
        /// @brief Constructor with lower limit of validity.
        /// @details Typically used by a new version of a method (upper range of validity).
        /// Assumed upper or equal from this value up to +infinity.
        /// @param[in] versionFrom Lower limit for valid version.
        /// @param[in] memberType Type of member to versioning.
        public VersionAttribute(float versionFrom, PluginVersioning.memberType memberType)
        {
            //TODO [ASB] : add support for exceptions trowed by VersRange
            _range = new VersRange(versionFrom);
            _memberType = memberType;
            //_versionatedMember = PluginVersioning.versionatedMember.none;
        }

        /// @brief Constructor with both limits for validity.
        /// @details Implies upper or equal from `lowerLimit` and less than `upperLimit`.
        /// @param[in] lowerLimit Lower limit for valid version.
        /// @param[in] upperLimit Upper limit for valid version.
        /// @param[in] memberType Type of member to versioning.
        public VersionAttribute(float lowerLimit, float upperLimit, PluginVersioning.memberType memberType)
        {
            ///TODO [ASB] : add support for exceptions trowed by VersRange
            _range = new VersRange(lowerLimit, upperLimit);
            _memberType = memberType;
            //_versionatedMember = PluginVersioning.versionatedMember.none;
        }
        #endregion

        public PluginVersioning.memberType MemberType
        {
            get { return _memberType; }
        }

        public float VersionFrom {
            get { return _range.LowerLimit; } 
        }

        /// @brief Validate if atributte is valid beetween lower and upper limits of permitted range.
        /// @param[in] version Version number to test validity.
        /// @returns True if between limits, false if not.
        // TODO [ASB] : define if it would be used only inside (=> change to private) or not.
        public bool ValidOnVersion(float version) 
        {
            return _range.ValidOn(version);
        }
    }

    /// @brief Manages Versions of plugins.
    /// @details Manages versions of plugins, to choose correct member signatures for each version
    /// of plugin system. 
    /// It is used on the definition of avalaible versions into PluginVersioning class, and also
    /// to contain an instance of a compiled plugin with the reference to method to call.
    public class VersionatedContainer
    {
        /// @brief Pointer to plugin.
        private PluginBase _plugin;
        /// @brief version of plugin system to use
        private float _version;
        /// @brief Type of member to select.
        private PluginVersioning.memberType _memType;
        /// @brief Delegate type assigned to this container.
        private System.Type _assignedTypeDel;
        /// @brief Delegate assigned to this container
        private object _assignedDel;

        /// @brief Constructor with PluginBase specification.
        public VersionatedContainer(PluginBase plugin, PluginVersioning.memberType MemType)
        {
            _plugin = plugin;
            _memType = MemType;
            _version = GetVersion(plugin, MemType);
            _assignedTypeDel = null;
        }

        /// @brief Constructor with member type specification and delegated.
        public VersionatedContainer(
            PluginVersioning.memberType MemType, 
            float Version, 
            System.Type asignatedDelegate)
        {
            _plugin = null;
            _version = Version;
            _memType = MemType;
            _assignedTypeDel = asignatedDelegate;
        }

        /// @brief Attribute to PluginBase
        public PluginBase Plugin
        {
            get { return _plugin; }
            set 
            {
                if (value != null)
                    _plugin = value;
            }
        }

        /// @brief Determine if plugin is a valid reference (=true) or null (=false).
        public bool IsValidPlugin()
        {
            return (_plugin != null);
        }

        /// @brief Attribute to hold target version
        public PluginVersioning.memberType memberType
        {
            get { return _memType; }
            set { _memType = value; }
        }

        /// @brief Get Version for the member type given of the Plugin instance.
        /// @param plugin Plugin instance to obtain its version number.
        /// @param memberType Type of versionated member to obtain its version.
        /// @returns Version of plugin to declare
        private float GetVersion(PluginBase plugin, PluginVersioning.memberType memberType)
        {
            float ver = 0.0f;
            if (IsValidPlugin())
            {
                SortedList<float, VersionMemberInfo> selected = GetVersionatedCandidates(plugin.GetType(), memberType);
                //TODO [ASB] : seleccionar con cual version me quedo
            }
            return ver;
        }

        //TODO[ASB] : cambiar en SortedList<float, MethodInfo>, MethodInfo por un struct conteniendo MethodInfo y un atributo para cachar si es de clase base o derivada.
        //ejemplo referencia para obtener atributos desde reflexion:
        //http://stackoverflow.com/questions/6637679/reflection-get-attribute-name-and-value-on-property
        //
        private SortedList<float, VersionMemberInfo> 
            GetVersionatedCandidates( Type tPlugin, PluginVersioning.memberType memberType)
        {
            //prepare the sorted list of candidates to output
            SortedList<float, VersionMemberInfo> selMeth = new SortedList<float, VersionMemberInfo>();
            //get the methods list of the plugin
            MethodInfo[] meth = tPlugin.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            //browse the method list
            foreach (MethodInfo mInfo in meth)
            {
                //get the custom attributes for the method
                Object[] attr = mInfo.GetCustomAttributes(typeof(VersionAttribute), true);
                //if there are custom attributes for it
                if (attr.Length > 0)
                {   //browse the attribute
                    foreach (Object obj in attr)
                    {   
                        VersionAttribute vers = obj as VersionAttribute;    //cast as VersionAttribute
                        //if it is a VersionAttribute type
                        if (vers != null) 
                            if (vers.MemberType == memberType)  //if it is of memberType type
                            {   //create a entry on the sorted list
                                selMeth.Add(vers.VersionFrom, 
                                    new VersionMemberInfo(
                                        vers.VersionFrom,
                                        mInfo,
                                        (mInfo.GetType() == typeof(PluginBase)) ? true : false) );
                            }
                    }
                } 
            }
            return selMeth;
        }

        /// @brief Get member code by type and version.
        private bool Invoke(PluginVersioning.memberType member)
        {
            bool success = false;
            ///TODO [ASB] : agregar lógica para determinar el tipo de miembro según versión, y 
            //  ejecutarlo
            if (PluginVersioning.ManagedVersions.ContainsKey(member))
            {
                switch (member)
                {
                    case PluginVersioning.memberType.OnPinChange:

                        break;
                    case PluginVersioning.memberType.OnClock:

                        break;
                    case PluginVersioning.memberType.PresentChip:

                        break;

                };
            }
            return success;
        }

        /// @TODO [ASB] : leer que interfaz es requerida de implementar en VersionMemberInfo para ser utilizado dentro de container VersionatedContainerCollection (¿ICollection?)
        private struct VersionMemberInfo
        {
            bool IsInherited;
            float VersionLow;
            MethodInfo MInfo;

            public VersionMemberInfo(float versionLow, MethodInfo mInfo, bool isInherited)
            {
                IsInherited = isInherited;
                VersionLow = versionLow;
                MInfo = mInfo;
            }
        }
    }

    public class VersionatedContainerCollection
    {
        private List<VersionatedContainer> _list;

        public VersionatedContainerCollection()
        {
            _list = new List<VersionatedContainer>();
        }

        public bool Contains(PluginBase plugin)
        {
            bool exist = false;
            foreach (VersionatedContainer vc in _list)
            {
                exist |= (vc.Plugin == plugin);
                if (exist) break;
            }
            return exist;
        }

        public void Add(PluginBase plugin)
        {
            //TODO[ASB] : completar metodo Add()
        }

        public void Remove(PluginBase plugin)
        {
            //TODO[ASB] : completar metodo Remove()
        }
    }

    //reference to implement generic collection con ICollect interface:
    //http://www.codeproject.com/Articles/21241/Implementing-C-Generic-Collections-using-ICollecti
    //
    //reference on ICollection:
    //http://msdn.microsoft.com/es-es/library/92t2ye13%28v=vs.100%29.aspx

}