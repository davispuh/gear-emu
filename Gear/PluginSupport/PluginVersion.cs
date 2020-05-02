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
    public class PluginVersioning : System.IDisposable 
    {
        /// @brief Type of member for version management on menbers of Plugins.
        /// @details They must be equal to the name of the method to work, because the retrieving
        /// mechanism use reflection based on names of PluginBase methods to versioning.
        public enum memberType
        {
            OnClock = 0,    //!< Run on clock ticks.
            OnPinChange,    //!< Run on pin changes.
            PresentChip     //!< Prepare the notifiers.
        }

        /// @brief Structure to hold the data about a versionated method.
        public struct VersionMemberInfo
        {
            /// @brief Hold validity of the information of the struct.
            /// @details When created by default constructor,this will be =false; when in parametered 
            /// constructor, this will be =true.
            public bool Valid;
            /// @brief Indicate if the method was instanciated on the derived class (=true) 
            /// or in base class (=false) that means in PluginBase class.
            public bool IsDeclaredInDerived;
            /// @brief Indicate if if the method is considered mandatory and must to be defined
            /// in the derived class (not only in Pluginbase).
            public bool IsMandatory;
            /// @brief Indicate the version of the member.
            public float VersionLow;
            /// @brief Information of the method, from Reflexion.
            public MethodInfo MInfo;
            /// @brief Array of parameters of the method.
            public SortedList<int,ParamMemberInfo> ParamListOfMethod;

            /// @brief Constructor with parameters.
            public VersionMemberInfo(float versionLow, MethodInfo mInfo, bool isInherited,
                bool isMandatory, ParameterInfo[] paramArr)
            {
                Valid = true;
                VersionLow = versionLow;
                MInfo = mInfo;
                IsDeclaredInDerived = isInherited;
                IsMandatory = isMandatory;
                //build the list of parameters for the method
                ParamListOfMethod = new SortedList<int,ParamMemberInfo>();
                foreach(ParameterInfo param in paramArr)
                {
                    //get attributes that decorates the parameter (if any)
                    Object[] attr = param.GetCustomAttributes(typeof(ParamOrderAttribute), true);
                    //if there are custom attributes for it
                    if (attr.Length > 0)
                    {   //browse the attribute
                        foreach (Object obj in attr)
                        {
                            ParamOrderAttribute ord = obj as ParamOrderAttribute;    //cast 
                            //if it is a ParamOrderAttribute type
                            if (ord != null)
                            {
                                //TODO [ASB] : determinar si el orden en que ser entregan los parametros de un metodo es confiable o no. Si es confiable, 1) dejar a ParamListOfMethod como un array, 2) volar todo el código de atributos en el constructor, 3) eliminar la clase ParamOrderAttribute desde PluginVersion.cs, 4) eliminar la decoración con atributo ParamOrder en PluginBase.
                            }
                        }
                    }

                }
            }
        }

        /// @brief Structure to hold parameter information to versionated members.
        public struct ParamMemberInfo
        {
            /// @brief Holds the name of the target parameter.
            public string Name;
            /// @brief Holds the value (and type) of the parameter passed.
            /// @note If this have value, ParamType must be null.
            public Object ParameterObj;
            /// @brief Hold the type of the parameter.
            /// @note If this have value, ParameterObj must be null.
            public Type ParameterType;
            /// @brief Hold the order of the parameter.
            public int Order;

            /// @brief Constructor with a Object parameter: to be used with given parameters (with values).
            /// @param[in] name Name of the given parameter.
            /// @param[in] parameter Object reference to the value given as parameter.
            public ParamMemberInfo(string name, Object parameter)
            {
                Name = name;
                ParameterObj = parameter;
                ParameterType = null;
                Order = 0;
            }

            /// @brief Constructor with a Type parameter: to be used with method's parameters (definitions).
            /// @param[in] name Name of the parameter to be supplied.
            /// @param[in] type Type of object required.
            /// @param[in] order Order in the invocation.
            public ParamMemberInfo(string name, Type type, int order)
            {
                Name = name;
                ParameterType = type;
                ParameterObj = null;
                Order = order;
            }

            /// @brief Determine if are compatible with the name and Type given
            /// @param[in] name Name of the parameter to compare to.
            /// @param[in] typ Type of the parameter to compare to.
            /// @returns If they are compatible parameters (=true), else (=false).
            public bool IsCompatible(string name, Type typ)
            {
                return ((this.Name == name) && (this.ParameterObj.GetType() == typ));
            }
        }

        /// @brief List of possible parameters for each memberType subject to versioning.
        /// @details This list is filled with all the possibles parameters when the constructor is
        /// called, because a plugin instance is needed (no static call).
        private SortedList<memberType, SortedList<string, Type>> ParamsByType;
        /// @brief Reference to plugin type
        private Type _pluginType;
        /// @brief Property for type of plugin
        public Type PluginType { 
            get { return _pluginType; }
            private set { _pluginType = value; }
        }

        /// @brief Default constructor given a plugin class type.
        public PluginVersioning(PluginBase plugin) 
        {
            // todo [ASB] : add validation of methods of pluginBase having same memberType and version number, and throw an exception in runtime (as I don't know how to do that in compile time).

            if (plugin == null)
                throw new VersioningPluginException(
                    "Plugin reference is NULL, for new PluginVersioning(null)");
            else
            {
                PluginType = plugin.GetType();

                ParamsByType = new SortedList<memberType, SortedList<string, Type>>();
                //traverse across the member types to build the list of ParamsByType
                foreach (memberType memTyp in (memberType[])Enum.GetValues(typeof(memberType)))
                {
                    //start with a empty list of parameters each time
                    var paramsFound = new SortedList<string, Type>();
                    //traverse across each versionated method
                    foreach (Tuple<MethodInfo, ParameterInfo[]> eachMeth in GetVersionatedMethods(memTyp))
                    {
                        foreach (ParameterInfo paramInfo in eachMeth.Item2)
                        {
                            //if the list not contains the name & type of the parameter already...
                            if (!paramsFound.ContainsKey(paramInfo.Name) &
                                 !paramsFound.ContainsValue(paramInfo.ParameterType))
                            {
                                //...add it to the list
                                paramsFound.Add(paramInfo.Name, paramInfo.ParameterType);
                            }
                        }
                    }
                    //if any parameter found...
                    if (paramsFound.Count > 0)
                        this.ParamsByType.Add(memTyp, paramsFound);    //... add to the list of parameters
                }
            }
        }

        /// @brief Get the maximum quantity of paramers for the versionated methods of the supplied 
        /// member type.
        /// @param memberType Type of versionated member.
        /// @returns Quantity of parameters for the member type.
        public int ParametersQty(PluginVersioning.memberType memberType)
        {
            if ((ParamsByType.Count == 0) | (!ParamsByType.ContainsKey(memberType)))
                return 0;
            else
            {
                SortedList<string, Type> paramList;
                ParamsByType.TryGetValue(memberType, out paramList);
                return paramList.Count;
            }
        }

        /// @brief Retrieve a list with all the possible parameters for the versionated methods of 
        /// the supplied member type.
        /// @param[in] memberType Type of versionated member.
        /// @returns List of tuples name & type of the parameters.
        public SortedList<string, Type> GetPossibleParams(PluginVersioning.memberType memberType)
        {
            var list = new SortedList<string, Type>();
            ParamsByType.TryGetValue(memberType, out list);
            return list;
        }

        /// @brief Auxiliary method to retrieve a list of versionated methods of a PluginBase instance
        /// of the specefied member type.
        /// @details It use Reflexion to obtain the information dinamically.
        /// @param[in] memberType Type of versionated member.
        /// @returns List of tuples containing information of the method and its parameters, 
        /// (internal format to this class).
        private List<Tuple<MethodInfo, ParameterInfo[]>> 
            GetVersionatedMethods(PluginVersioning.memberType memberType)
        {
            var result = new List<Tuple<MethodInfo,ParameterInfo[]>>();
            //get the methods list of the plugin
            MethodInfo[] meth = _pluginType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
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
                            {
                                result.Add(
                                    new Tuple<MethodInfo, ParameterInfo[]>(mInfo,mInfo.GetParameters())
                                );
                            }
                    }
                }
            }
            return result;
        }

        /// @brief Obtain versionated list of members of the type, using reflexion on plugin type.
        /// @param memberType Type of versionated member.
        /// @returns Sorted list of VersionMemberInfo by version of members of memberType type.
        /// @note @internal Example to obtain attributes from Reflexion:
        /// http://stackoverflow.com/questions/6637679/reflection-get-attribute-name-and-value-on-property
        private SortedList<float, VersionMemberInfo>
            VersionatedMemberCandidates(PluginVersioning.memberType memberType)
        {
            //prepare the sorted list of candidates to output
            var selMeth = new SortedList<float, VersionMemberInfo>();
            //get the versionated methods of the plugin
            List<Tuple<MethodInfo, ParameterInfo[]>> meth = GetVersionatedMethods(memberType);
            //browse the method list
            foreach (Tuple<MethodInfo, ParameterInfo[]> tupleInfo in meth)
            {
                //get the custom attributes for the method
                Object[] attr = tupleInfo.Item1.GetCustomAttributes(typeof(VersionAttribute), true);
                //if there are custom attributes for it
                if (attr.Length > 0)
                {   //browse the attribute
                    foreach (Object obj in attr)
                    {
                        VersionAttribute vers = obj as VersionAttribute;    //cast as VersionAttribute
                        //if it is a VersionAttribute type
                        if (vers != null)
                        {
                            //create a entry on the sorted list
                            selMeth.Add(
                                vers.VersionFrom,
                                new VersionMemberInfo(
                                    vers.VersionFrom,               //VersionLow
                                    tupleInfo.Item1,                //MethodInfo
                                    //does it is instanciated here, so not in base class?
                                    (tupleInfo.Item1.DeclaringType == _pluginType), //IsInherited
                                    vers.IsMandatory,               //IsMandatory
                                    tupleInfo.Item2                 //parameter list of the method
                                )
                            );
                         }
                    }
                }
            }
            return selMeth;
        }

        /// @brief Get the high version implemented method for the given member type of the 
        /// Plugin instance.
        /// @details As theorically a PluginBase descendent can have instanciated more than one 
        /// version of each memberType, this method detects and returns the higher version available.
        /// @pre It assumes the version number of methods for member type are unique: a validation of that
        /// is needed in the begining of the program or complile time.
        /// @param[in] memberType Type of versionated member to obtain its version.
        /// @param[out] mInfo Information about the upper version instanciated method to use. If exist 
        /// it, return the only value; otherwise, a empty structure (filled with 0's and null).
        /// @returns True if there is at least one versionated member of the type supplied as 
        /// parameter, or False if there is not.
        /// @note @internal reference on article "How to loop through a SortedList, getting both the key and the value"  http://stackoverflow.com/questions/14013261/how-to-loop-through-a-sortedlist-getting-both-the-key-and-the-value
        public bool GetImplementedMethod(
            PluginVersioning.memberType memberType, 
            out PluginVersioning.VersionMemberInfo mInfo)
        {
            mInfo = new PluginVersioning.VersionMemberInfo();
            if (_pluginType == null)
                return false;
            else
            {
                //Get the list of avalaible versions of this member type.
                var candidates = VersionatedMemberCandidates(memberType);
                if (candidates.Count == 0)    //if there is no method of this type implemented on plugin
                    return false;
                else    //if there at least one method of this type implemented on plugin
                {
                    float ver = -1.0f;
                    bool exists = false;
                    //browse the candidates list looking for a method instanciated in derived plugin class
                    foreach (KeyValuePair<float, PluginVersioning.VersionMemberInfo> pair in candidates)
                    {
                        if (pair.Value.IsDeclaredInDerived)
                        {
                            //remember higher value of key, to update the out parameter with the 
                            //corresponding struct later.
                            ver = ((pair.Key >= ver) ? pair.Key : ver);
                            exists = true;
                        }
                    }
                    //if exists, and assuming the precondition of the method was validated, next call 
                    //must retrieve exactly one object.
                    if (exists)
                        candidates.TryGetValue(ver, out mInfo);  //update mInfo parameter.
                    return exists;
                }
            }
        }

        /// @brief Determine if exist into plugin, a implemented member of the type given as 
        /// parameter.
        /// @details This method is designed to be used after the plugin is loaded in memory, to 
        /// check if it is consistent: ex. when the derived plugin declare on PresentChip() that 
        /// it use NotifyOnPins() method, there must exist a definition for OnPinChange() method 
        /// correspondly.
        /// @param[in] memberType Type of versionated member to check.
        /// @returns True if there is an implemented member, false if not.
        public bool IsMemberTypeImplemented(PluginVersioning.memberType memberType)
        {
            var temp = GetVersionatedMethods(memberType);
            return (temp.Count > 0);
        }

        /// @brief Check if the mandatory methods for each type are defined. If not, 
        /// also returns an error list.
        /// @returns True if all the mandatory members are defined, of false if not.
        public bool IsMandatoryMembersDefined() 
        {
            if (_pluginType == null)
                return false;
            else
            {
                /// todo [ASB] : agregar lógica y devolver una lista de errores compatible con el compilador de plugins.
                return true;
            }
        }
    
        /// @brief Member of IDisposable interface
        public void Dispose()
        {
            ParamsByType.Clear();
        }

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
        AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class VersionAttribute : System.Attribute
    {
        
        /// @brief Range of versioning.
        private VersRange _range;
        /// @brief Type of member to versioning.
        private PluginVersioning.memberType _memberType;
        /// @brief States if it is Mandatory (=true) or optional (=false) to declare in the plugin.
        private bool isMandatory;

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

    ///@brief Attribute class to decorate the parameters of a versionated member of PluginBase.
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class ParamOrderAttribute : System.Attribute
    {
        /// @brief Name of the parameter to associate 
        private int _order;

        /// @brief Constructor
        public ParamOrderAttribute(int order)
        {
            _order = order;
        }

        /// @brief Property for order.
        public int Order { get { return _order; } }

    }

    /// @brief Exception class for versioning problems of members or null plugin reference.
    [Serializable]
    public class VersioningPluginException : System.Exception
    {
        private PluginBase _plugin;
        public VersioningPluginException() { }
        public VersioningPluginException(string message) : base(message) { }
        public VersioningPluginException(string message, PluginBase plugin)
            : base(message)
        {
            _plugin = plugin;
        }
        public VersioningPluginException(string message, Exception inner) : base(message, inner) { }
        protected VersioningPluginException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
        public bool IsNullPluginBase() { return (_plugin == null);}
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
        /// @brief Information of versionated member to use for the member type.
        private PluginVersioning.VersionMemberInfo _member;

        /// @brief Default constructor.
        /// @details The constructor determines also the version of the member, from the type class
        /// and the member type parameter, but it is possible that it doesn't have one, as the case 
        /// of system plugins (logic probe, cogs windows, etc).
        /// @param[in] plugin Reference to plugin.
        /// @param[in] MemType Type of member to select.
        /// @exception VersioningPluginException Null plugin encountered.
        public VersionatedContainer(PluginBase plugin, PluginVersioning.memberType MemType)
        {
            if (plugin == null)
            {
                throw new VersioningPluginException(
                    "Plugin reference is NULL, for new VersionatedContainer() of \"" +
                     MemType.ToString() + "\" member type.");
            };
            if (!plugin.IsUserPlugin)
            {
                throw new VersioningPluginException(
                    "Plugin \"" +
                     plugin.Name + "\" is system plugin. Versioning is only allowed to user plugins.");
            }
            else
            {
                _plugin = plugin;
                _membType = MemType;
                //As theorically a PluginBase descendent can have instanciated more than one version
                //of each memberType, below code detects and returns the higher version available.
                //if there is none avalaible, the return value is false.
                if (_plugin.Versioning.GetImplementedMethod(MemType, out _member))
                    _version = _member.VersionLow;
                else
                    _version = -1.0f;
            }
        }

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

        /// @brief  Property for access target version.
        public PluginVersioning.memberType memberType
        {
            get { return _membType; }
            set { _membType = value; }
        }

        /// @brief Determine if the container have a valid versionated member.
        /// @details To determine this, this method take in consideration if the version of and the 
        /// versionated method itself could be determined, expecting to be zero or higher to be 
        /// considered valid.
        /// @returns True if the versionated member is valid, or false if not.
        public bool IsValidMember()
        {
            return ( (_version >= 0.0f) & (_member.Valid) );
        }

        /// @brief Indicates if the current object is equal to another object of the same type,
        /// to implement System.IEquatable<> interface.
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
        /// to implement System.IEquatable<> interface, overriding the implementation for Object.
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

        /// @brief Serves as the default hash function for Object, to implement 
        /// System.IEquatable<> interface.
        public override int GetHashCode()
        {
            return this._version.GetHashCode();
        }

        /// @brief Equal operator, required to implement System.IEquatable<> interface.
        /// @param cont1 First container to compare.
        /// @param cont2 Second container to compare.
        /// @returns True if they are the same, or False if are different.
        public static bool operator ==(VersionatedContainer cont1, VersionatedContainer cont2)
        {
            if ((object)cont1 == null || ((object)cont2 == null))
                return Object.Equals(cont1, cont2);
            return cont1.Equals(cont2);
        }

        /// @brief Different operator, required to implement System.IEquatable<> interface.
        /// @param cont1 First container to compare.
        /// @param cont2 Second container to compare.
        /// @returns True if they are different, or False if are the same.
        public static bool operator !=(VersionatedContainer cont1, VersionatedContainer cont2)
        {
            if (cont1 == null || cont2 == null)
                return !Object.Equals(cont1, cont2);
            return !(cont1.Equals(cont2));
        }

        /// @brief Get member code by type and version using Reflexion, then and run it with the 
        /// supplied parameter list.
        /// @param[in] parms Parameter list to perform the invocation of a versionated member
        public bool Invoke(params PluginVersioning.ParamMemberInfo[] parms)
        {
            if (!this.IsValidMember())
                return false;
            else
            {
                bool state = false;
                //validate the given parameter list is not empty
                if (parms.Length == 0)
                    return false;
                else
                {
                    //validate if there are not enough parameters according to memberType
                    if (_plugin.Versioning.ParametersQty(_membType) > parms.Length)
                    {
                        throw new VersioningPluginException(
                            "There are not enough parameters supplied in plugin \"" + _plugin.Name + 
                            "\" when invoking for \"" +  _membType.ToString() + "\" member type. Supplied " +
                            parms.Length + ", but " + _plugin.Versioning.ParametersQty(_membType) +
                            "required.");
                    }
                    else
                    {
                        //get the possible parameters for this member type
                        var possibleParamList = _plugin.Versioning.GetPossibleParams(_membType);
                        //traverse the given parameters
                        foreach (PluginVersioning.ParamMemberInfo givenParam in parms)
                        {
                            Type correspType;
                            //check if this given parameter exists in the possibles parameter list
                            if (! possibleParamList.TryGetValue(givenParam.Name, out correspType))
                                throw new VersioningPluginException(
                                    "Given parameter \"" + givenParam.ParameterObj.GetType().ToString() + 
                                    " " + givenParam.Name + 
                                    "\" not found in the possibles parameter list for \"" +
                                    _membType.ToString() + "\" member type in plugin \"" + _plugin.Name + 
                                    "\".");
                            //check if the given parameter is compatible (name & type) with the possible one
                            if (! givenParam.IsCompatible(givenParam.Name, correspType))
                                throw new VersioningPluginException(
                                    "Given parameter \"" + givenParam.ParameterObj.GetType().ToString() + 
                                    " " + givenParam.Name + 
                                    "\" not matched with the possibles parameter list for \"" + 
                                    _membType.ToString() + "\" member type in plugin \"" + _plugin.Name + 
                                    "\".");
                            //
                            //TODO [ASB] : determinar el orden de los parametros a entregar a la llamada
                            _member.ParamListOfMethod

                        }
                        
                        // TODO [ASB] : agregar lógica para invocar el método (nombre: _member.MInfo.Name)
                        


                        state = true;
                    }
                    return state;
                }
            }
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
        /// @param[in] index Index to choose an container element.
        public VersionatedContainer this[int index]
        {
            get { return (VersionatedContainer)_list[index]; }
            set { _list[index] = value; }
        }

        /// @brief Determine if the Container have one reference of the plugin inside.
        /// @note This method is thought to be used in the program.
        /// @param[in] plugin Plugin reference to check its precesence in the collection.
        /// @param[in] MemType Member type to check.
        /// @returns True if exist one instance of the plugin, or False if not.
        public bool Contains(PluginBase plugin, PluginVersioning.memberType MemType)
        {
            if (plugin == null)
                return false;
            else
            {
                bool exist = false;
                foreach (VersionatedContainer vc in _list)
                {
                    exist |= ( (vc.Plugin == plugin) & (vc.memberType == MemType) );
                    if (exist) break;
                }
                return exist;
            }
        }

        /// @brief Determine if the Container have one reference of the plugin inside.
        /// @note This method is required by System.Collections.Generic.ICollection<> Interface, 
        /// but not thought to be used widely.
        /// @param[in] verCont Container reference to check its precesence in the collection.
        /// @returns True if exist one instance of the plugin, or False if not.
        public bool Contains(VersionatedContainer verCont)
        {
            return _list.Contains(verCont);
        }

        /// @brief Add a plugin of the container, given a plugin & versionated member type.
        /// @pre The plugin was revised on requirement for mandatory members (using
        /// PluginVersioning.MandatoryMembersDefined() static method) before call this method.
        /// @param[in] plugin Plugin to reference.
        /// @param[in] MemType Type of member.
        /// @exception VersioningPluginException Problems encountered as versionated member not 
        /// valid for the member type, or null plugin.
        public void Add(PluginBase plugin, PluginVersioning.memberType MemType)
        {
            try {
                VersionatedContainer cont = new VersionatedContainer(plugin, MemType);
                if (!cont.IsValidMember())
                    throw new VersioningPluginException(
                        "There is no '" + MemType.ToString() + "' defined member in '" +
                        plugin.Name + "' plugin.", plugin);
                else
                    this.Add(cont);
            }
            catch (VersioningPluginException e)
            {
                if (e.IsNullPluginBase())
                {
                    throw new VersioningPluginException("Plugin reference is NULL, for " + MemType.ToString() + "member type.", e);
                }
                else {
                    throw e;
                }
            }
        }

        /// @brief Add a plugin of the container, given a VersionatedContainer object reference.
        /// @note This method is required by System.Collections.Generic.ICollection<> Interface, 
        /// but used by VersionatedContainerCollection.Add(VersionatedContainer verCont) too.
        /// @param[in] verCont Container reference to add to collection.
        public void Add(VersionatedContainer verCont)
        {
            _list.Add(verCont);
        }

        /// @brief Remove a plugin of the container, given plugin reference.
        /// @note This method is thought to be used widely in the program.
        /// @param[in] plugin Plugin reference to remove.
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
        /// @note This method is required by System.Collections.Generic.ICollection<> Interface, 
        /// but not thought to be used widely.
        /// @param[in] verCont Container reference to remove.
        /// @returns True if item is successfully removed; otherwise, false. Also returns false if 
        /// item was not found.
        public bool Remove(VersionatedContainer verCont)
        {
            return _list.Remove(verCont);
        }

        /// @brief Clear all the contents of the container.
        /// @note This method is required by System.Collections.Generic.ICollection<> Interface, 
        /// but not thought to be used widely.
        public void Clear()
        {
            _list.Clear();
        }

        /// @brief Copies elements from container to an Array, starting at a particular Array index.
        /// @param[in] array Array reference to copy at.
        /// @param[in] arrayIndex Start index to start copying.
        /// @note This method is required by System.Collections.Generic.ICollection<> Interface, 
        /// but not thought to be used widely.
        public void CopyTo(VersionatedContainer[] array, int arrayIndex)
        {
            for (int i = 0; i < _list.Count; i++)
            {

                array[i] = (VersionatedContainer)_list[i];
            }
        }

        /// @todo Document VersionatedContainerCollection.GetEnumerator()
        /// @note This method is required by System.Collections.Generic.ICollection<> Interface.
        public IEnumerator<VersionatedContainer> GetEnumerator()
        {
            return new VersionatedContainerEnumerator(this);
        }

        /// @todo  Document VersionatedContainerCollection.GetEnumerator()
        /// @note This method is required by System.Collections.Generic.ICollection<> Interface.
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