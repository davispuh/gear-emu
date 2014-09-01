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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Gear.EmulationCore; 

/// @copydoc Gear.PluginSupport
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

        //static public Dictionary<memberType, VersionatedContainer> ManagedVersions;

        /// @brief Document Gear.PluginSupport.PluginVersioning.VersionMemberInfo struct.
        private struct VersionMemberInfo
        {
            public bool IsDeclaredInDerived;
            public bool IsMandatory;
            public float VersionLow;
            public MethodInfo MInfo;

            public VersionMemberInfo(float versionLow, MethodInfo mInfo, bool isInherited, bool isMandatory)
            {
                VersionLow = versionLow;
                MInfo = mInfo;
                IsDeclaredInDerived = isInherited;
                IsMandatory = isMandatory;
            }
        }

        /// @brief Static default constructor
        static PluginVersioning()
        {
            ////TODO [ASB] : change this declaration for a dynamic evaluation with reflexion class
            //ManagedVersions = new Dictionary<memberType, VersionatedContainer> ();
            //ManagedVersions.Add(memberType.none, null);
            //ManagedVersions.Add(memberType.OnClock,
            //    new VersionatedContainer(memberType.OnClock, 0.0f, typeof(OnClockV0_0)));
            //ManagedVersions.Add(memberType.OnClock,
            //    new VersionatedContainer(memberType.OnClock, 1.0f, typeof(OnClockV1_0)));
            //ManagedVersions.Add(memberType.OnPinChange,
            //    new VersionatedContainer(memberType.OnPinChange, 0.0f, typeof(OnPinChangeV0_0)));
            //ManagedVersions.Add(memberType.PresentChip,
            //    new VersionatedContainer(memberType.PresentChip, 0.0f, typeof(PresentChipV0_0)));
            //ManagedVersions.Add(memberType.PresentChip,
            //    new VersionatedContainer(memberType.PresentChip, 1.0f, typeof(PresentChipV1_0)));
        }

        /// @brief Obtain versionated list of members of the type, using reflexion on plugin type.
        /// @param tPlugin  Plugin Type to obtain versionated methods implementation details.
        /// @param memberType Type of versionated member.
        /// @returns Sorted list of VersionMemberInfo by version of members of memberType type.
        /// @note @internal Example to obtain attributes from Reflexion:
        /// http://stackoverflow.com/questions/6637679/reflection-get-attribute-name-and-value-on-property
        static private SortedList<float, VersionMemberInfo>
            GetVersionatedCandidates(Type tPlugin, PluginVersioning.memberType memberType)
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
                            if (vers.MemberType == memberType)  //if type is the same of given parameter
                            {   //create a entry on the sorted list
                                selMeth.Add(
                                    vers.VersionFrom,
                                    new VersionMemberInfo(
                                        vers.VersionFrom,
                                        mInfo,
                                        (mInfo.DeclaringType == tPlugin) ? true : false,
                                        vers.IsMandatory
                                    )
                                );
                            }
                    }
                }
            }
            return selMeth;
        }

        /// @brief Get Version for the given member type of the Plugin instance.
        /// @details As theorically a PluginBase descendent can have instanciated more than one 
        /// version of each memberType, this method detects and returns the higher version available.
        /// @param[in] plugin Plugin instance to obtain its version number.
        /// @param[in] memberType Type of versionated member to obtain its version.
        /// @param[out] ver Upper version instanciated of the plugin supplied as parameter.
        /// @returns True if there is at least one versionated member of the type supplied as 
        /// parameter, or False if there is not.
        /// @note @internal reference on article "How to loop through a SortedList, getting both the key and the value"  http://stackoverflow.com/questions/14013261/how-to-loop-through-a-sortedlist-getting-both-the-key-and-the-value
        static public bool GetImplementedVersion(PluginBase plugin,
            PluginVersioning.memberType memberType, out float ver)
        {
            ver = -1.0f;
            if (plugin == null)
                return false;
            else
            {
                //Get the list of avalaible versions of this member type.
                SortedList<float, VersionMemberInfo> selected =
                    GetVersionatedCandidates(plugin.GetType(), memberType);
                if (selected.Count == 0)    //if there is no method of this type implemented on plugin
                    return false;
                else    //if there at least one method of this type implemented on plugin
                {
                    bool exists = false;
                    //browse the candidates list looking for a method instanciated in derived plugin class
                    foreach (KeyValuePair<float, VersionMemberInfo> pair in selected)
                    {
                        if (pair.Value.IsDeclaredInDerived)
                        {
                            //remember higher value
                            ver = ((pair.Key >= ver) ? pair.Key : ver);
                            exists = true;
                        }
                    }
                    return exists;
                }
            }
        }

        /// @brief Determine if exist in the plugin a implemented member of the type given as parameter.
        /// @details This method is used after the plugin is loaded in memory, to check if it is 
        /// consistent: ex. when the derived plugin declare on PresentChip() that it use NotifyOnPins()
        /// method, there must exist a definition for OnPinChange() method correspondly.
        /// @param[in] plugin Plugin instance to check.
        /// @param[in] memberType Type of versionated member to check.
        /// @returns True if there is an implemented member, false if not.
        static public bool IsMemberTypeImplemented(PluginBase plugin,
            PluginVersioning.memberType memberType)
        {
            float temp;
            return GetImplementedVersion(plugin, memberType, out temp);
        }

        /// @brief Check if the mandatory methods for each type are defined. If not, 
        /// also returns an error list.
        /// @todo agregar parametro para devolver una lista de errores compatible con el compilador de plugins.
        /// 
        /// @param plugin  Plugin instance to be checked.
        /// @returns True if all the mandatory members are defined, of false if not.
        static public bool IsMandatoryMembersDefined(PluginBase plugin) 
        {
            if (plugin == null)
                return false;
            else
            {
                /// @todo agregar lógica y devolver una lista de errores compatible con el compilador de plugins.
                return true;
            }
        }

        //=======================================================================
        //Declare in this section delegates for each version of members to manage.
        #region Delegates for each versionated member of PluginBase.

        ///// @brief Delegate for version 0.0 for OnClock.
        //[VersionAttribute(0.0f, 1.0f, PluginVersioning.memberType.OnClock)]
        //public delegate void OnClockV0_0(double time);

        ///// @brief Delegate for version 1.0 for OnClock.
        //[Version(1.0f, PluginVersioning.memberType.OnClock)]
        //public delegate void OnClockV1_0(double time, uint sysCounter);

        ///// @brief Delegate for version 0.0 for OnPinChange.
        //[VersionAttribute(0.0f,PluginVersioning.memberType.OnPinChange)]
        //public delegate void OnPinChangeV0_0(double time);

        ///// @brief Delegate for version 0.0 for PresentChip.
        //[VersionAttribute(0.0f, 1.0f, PluginVersioning.memberType.PresentChip)]
        //public delegate void PresentChipV0_0(PropellerCPU host);

        ///// @brief Delegate for version 1.0 for PresentChip.
        //[Version(1.0f, PluginVersioning.memberType.PresentChip)]
        //public delegate void PresentChipV1_0();

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
    public class VersionAttribute : System.Attribute
    {
        
        /// @brief Range of versioning.
        private VersRange _range;
        /// @brief Type of member to versioning.
        private PluginVersioning.memberType _memberType;
        /// @brief States if it is Mandatory (=true) or optional (=false) to declare in the plugin.
        private bool isMandatory;
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
            isMandatory = false;
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
            isMandatory = false;
            //_versionatedMember = PluginVersioning.versionatedMember.none;
        }
        #endregion

        /// @brief Property for member type.
        public PluginVersioning.memberType MemberType
        {
            get { return _memberType; }
        }

        /// @brief Property for mandatory or optional state of the member.
        public bool IsMandatory
        {
            get { return isMandatory; }
            set { isMandatory = value; }
        }

        /// @brief Get the version value witch the member is valid.
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
    /// @note @internal MSDN reference on IEquatable<T> Interface:
    /// http://msdn.microsoft.com/es-es/library/ms131190%28v=vs.110%29.aspx
    public class VersionatedContainer : System.IEquatable<VersionatedContainer>
    {
        /// @brief Pointer to plugin.
        private PluginBase _plugin;
        /// @brief Version of plugin system to use
        private float _version;
        /// @brief Type of member to select.
        private PluginVersioning.memberType _membType;
        ///// @brief Delegate type assigned to this container.
        //// todo [ASB] : revisar si se requerirán los delegados o no.
        //private System.Type _assignedTypeDel;
        ///// @brief Delegate assigned to this container
        //// todo [ASB] : revisar si se requerirán los delegados o no.
        //private object _assignedDel;

        /// @brief Constructor with PluginBase specification.
        public VersionatedContainer(PluginBase plugin, PluginVersioning.memberType MemType)
        {
            _plugin = plugin;
            _membType = MemType;
            //As theorically a PluginBase descendent can have instanciated more than one version of
            //each memberType, below code detects and returns the higher version available.
            _version = ((PluginVersioning.GetImplementedVersion(_plugin, _membType, out _version)) 
                ? _version 
                : -1.0f );
            //_assignedTypeDel = null;
        }

        ///// @brief Constructor with member type specification and delegated.
        //// todo [ASB] : revisar si se requerirán los delegados o no; sino eliminar constructor.
        //public VersionatedContainer(
        //    PluginVersioning.memberType MemType, 
        //    float Version, 
        //    System.Type asignatedDelegate)
        //{
        //    _plugin = null;
        //    _version = Version;
        //    _membType = MemType;
        //    _assignedTypeDel = asignatedDelegate;
        //}

        /// @brief Property for PluginBase.
        public PluginBase Plugin
        {
            get { return _plugin; }
            set 
            {
                if (value != null)
                    _plugin = value;
            }
        }

        /// @brief Get property for Version.
        public float Version
        {
            get { return _version; }
        }

        /// @brief  Property for hold target version.
        public PluginVersioning.memberType memberType
        {
            get { return _membType; }
            set { _membType = value; }
        }

        /// @brief Determine if plugin is a valid reference (=true) or null (=false).
        public bool IsValidPlugin()
        {
            return (_plugin != null);
        }

        /// @brief Determine if the container have a valid versionated member.
        /// @returns True if the versionated member is valid, or false if not.
        public bool IsValidMember()
        {
            return ((IsValidPlugin())&&(_version >= 0.0f) ? true : false);
        }

        /// @brief Indicates if the current object is equal to another object of the same type,
        /// to implement IEquatable<> interface.
        public bool Equals(VersionatedContainer other)
        {
            if (other == null)
                return false;
            else
            {
                if ((this._plugin == other._plugin) &&
                    (this._version == other._version) &&
                    (this._membType == other._membType))
                    return true;
                else
                    return false;
            }
        }

        /// @brief Indicates if the current object is equal to another object of the same type,
        /// to implement IEquatable<> interface, overriding the implementation for Object.
        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            VersionatedContainer ContObj = obj as VersionatedContainer;
            if (ContObj == null)
                return false;
            else
                return Equals(ContObj);
        }

        /// @brief Serves as the default hash function for Object.
        public override int GetHashCode()
        {
            return this._version.GetHashCode();
        }

        public static bool operator ==(VersionatedContainer cont1, VersionatedContainer cont2)
        {
            if ((object)cont1 == null || ((object)cont2 == null))
                return Object.Equals(cont1, cont2);
            return cont1.Equals(cont2);
        }

        public static bool operator !=(VersionatedContainer cont1, VersionatedContainer cont2)
        {
            if (cont1 == null || cont2 == null)
                return !Object.Equals(cont1, cont2);
            return !(cont1.Equals(cont2));
        }
        
        /// @brief Get member code by type and version.
        private bool Invoke(PluginVersioning.memberType member)
        {
            bool success = false;
            // TODO [ASB] : agregar lógica para determinar el tipo de miembro según versión, y 
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

    }

    /// @brief List of VersionatedContainer to handle plugins to call on clock tick or pin change
    /// @version 14.8.7 Added.
    /// @note @internal MSDN reference on ICollection Interface:
    /// http://msdn.microsoft.com/es-es/library/92t2ye13%28v=vs.100%29.aspx
    /// @par
    /// MSDN reference on IEnumerator<T> Interface:
    /// http://msdn.microsoft.com/es-es/library/78dfe2yb%28v=vs.110%29.aspx
    /// @par
    /// Example to implement generic collection con ICollect interface:
    /// http://www.codeproject.com/Articles/21241/Implementing-C-Generic-Collections-using-ICollecti
    public class VersionatedContainerCollection : 
        System.Collections.Generic.ICollection<VersionatedContainer>
    {
        /// @brief Internal list of containers of plugins and versionated members.
        private List<VersionatedContainer> _list;
        /// @brief Collection for error list on loading of a plugin.
        private CompilerErrorCollection m_Errors;    

        /// @brief Exception class for versioning problems of members.
        [Serializable]
        public class VersMembPluginException : Exception
        {
            PluginBase _plugin;
            public VersMembPluginException() { }
            public VersMembPluginException(string message) : base(message) { }
            public VersMembPluginException(string message, PluginBase plugin) : base(message) 
            {
                _plugin = plugin;
            }
            public VersMembPluginException(string message, Exception inner) : base(message, inner) { }
            protected VersMembPluginException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
        }

        /// @brief Default constructor
        public VersionatedContainerCollection()
        {
            _list = new List<VersionatedContainer>();
        }

        /// @brief Only allowed read/write container.
        public bool IsReadOnly { get { return false; } }

        /// @brief Get number of elements in the container.
        public int Count { get { return _list.Count; } }

        /// @brief Reference an element of the collection at index position.
        public VersionatedContainer this[int index]
        {
            get { return (VersionatedContainer)_list[index]; }
            set { _list[index] = value; }
        }

        /// @brief Determine if the Container have one reference of the plugin inside.
        /// @note This method is thought to be used in the program.
        /// @returns True if exist one instance of the plugin, or False if not.
        public bool Contains(PluginBase plugin)
        {
            if (plugin == null)
                return false;
            else
            {
                bool exist = false;
                foreach (VersionatedContainer vc in _list)
                {
                    exist |= (vc.Plugin == plugin);
                    if (exist) break;
                }
                return exist;
            }
        }

        /// @brief Determine if the Container have one reference of the plugin inside.
        /// @note This method is required by ICollection<> Interface, but not thought to be used.
        /// @param verCont Container reference to check its precesence in the collection.
        /// @returns True if exist one instance of the plugin, or False if not.
        public bool Contains(VersionatedContainer verCont)
        {
            return _list.Contains(verCont);
        }

        /// @brief Add a plugin of the container, given a plugin & versionated member type.
        /// @pre The plugin was revised on requirement for mandatory members (using
        /// PluginVersioning.MandatoryMembersDefined() static method) before call this method.
        /// @param plugin Plugin to reference.
        /// @param MemType Type of member.
        /// @exception VersMembPluginException Problems encountered as versionated member not 
        /// valid for the member type, or null plugin.
        public void Add(PluginBase plugin, PluginVersioning.memberType MemType)
        {
            VersionatedContainer cont = new VersionatedContainer(plugin, MemType);
            if (!cont.IsValidPlugin())
                throw new VersMembPluginException(
                    "Plugin reference is NULL, for " + MemType.ToString() + "member type.");
            else if (!cont.IsValidMember())
                throw new VersMembPluginException(
                    "There is no '" + MemType.ToString() + "' defined member in '" + 
                    plugin.Name + "' plugin.", plugin);
            else
                this.Add(cont);
        }

        /// @brief Add a plugin of the container, given a VersionatedContainer object reference.
        /// @note This method is required by ICollection<> Interface, but used too 
        /// by VersionatedContainerCollection.Add(VersionatedContainer verCont).
        /// @param verCont Container reference to add to collection.
        public void Add(VersionatedContainer verCont)
        {
            _list.Add(verCont);
        }

        /// @brief Remove a plugin of the container, given plugin reference.
        /// @note This method is thought to be used in the program.
        /// @param plugin Plugin reference to remove.
        /// @returns True if item is successfully removed; otherwise, false. Also returns false if 
        /// item was not found.
        public bool Remove(PluginBase plugin)
        {
            bool removed = false;
            foreach (VersionatedContainer vc in _list)
            {
                if (vc.Plugin == plugin)
                {
                    this.Remove(vc);
                    removed = true;
                    break;  //if found, exit the iteration
                }
            }
            return removed;
        }

        /// @brief Remove a plugin of the container.
        /// @note This method is required by ICollection<> Interface, but not thought to be used.
        /// @param verCont Container reference to remove.
        /// @returns True if item is successfully removed; otherwise, false. Also returns false if 
        /// item was not found.
        public bool Remove(VersionatedContainer verCont)
        {
            return _list.Remove(verCont);
        }

        /// @brief Clear all the contents of the container.
        /// @note This method is required by ICollection<> Interface, but not thought to be used.
        public void Clear()
        {
            _list.Clear();
        }

        /// @brief Copies elements from container to an Array, starting at a particular Array index.
        /// @note This method is required by ICollection<> Interface, but not thought to be used.
        public void CopyTo(VersionatedContainer[] array, int arrayIndex)
        {
            for (int i = 0; i < _list.Count; i++)
            {

                array[i] = (VersionatedContainer)_list[i];
            }
        }

        /// @todo Document VersionatedContainerCollection.GetEnumerator()
        /// @note This method is required by ICollection<> Interface.
        public IEnumerator<VersionatedContainer> GetEnumerator()
        {
            return new VersionatedContainerEnumerator(this);
        }

        /// @todo  Document VersionatedContainerCollection.GetEnumerator()
        /// @note This method is required by ICollection<> Interface.
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new VersionatedContainerEnumerator(this);
        }

        /// @brief Class to implement enumeration of VersionatedContainer objects.
        /// @note This class is required by ICollection<> Interface of VersionatedContainerCollection,
        /// and also it requires that implements IEnumerator<> Interface here too.
        class VersionatedContainerEnumerator : 
            System.Collections.Generic.IEnumerator<VersionatedContainer>
        {
            private VersionatedContainerCollection _collection;
            private int contIdx;
            private VersionatedContainer contRef;

            public VersionatedContainerEnumerator(VersionatedContainerCollection coll)
            {
                _collection = coll;
                contIdx = -1;
            }

            public bool MoveNext()
            {
                if (++contIdx >= _collection.Count) //Avoids going beyond the end of the collection.
                    return false;
                else
                    contRef = _collection[contIdx]; // Set current box to next item in collection.
                return true;
            }

            public void Reset() { contIdx = -1; }

            void IDisposable.Dispose() { }

            public VersionatedContainer Current { get { return contRef; } }

            object IEnumerator.Current { get { return Current; } }

        }

    }


}