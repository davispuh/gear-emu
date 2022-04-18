/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * ModuleCompiler.cs
 * Provides the reflection base and compiler components for plugins
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

using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace Gear.PluginSupport
{
    public delegate void ErrorEnumProc(CompilerError e);

    /// @brief Compile a PluginBase Module, keeping the errors if they appears.
    static class ModuleCompiler
    {
        /// @brief Collection for error list on compile a dynamic plugin.
        static private CompilerErrorCollection m_Errors = null;

        /// @brief Enumerate errors from the compiling process.
        /// @param proc Method to invoke for each error.
        static public void EnumerateErrors(ErrorEnumProc proc)
        {
            if (m_Errors == null)
                return;

            foreach (CompilerError e in m_Errors)
                proc(e);
        }

        /// @brief Dynamic compiling & loading for a plugin.
        /// @details Try to dynamically compile a className for the plugin, based on supplied C# code
        /// and other C# modules referenced. If the compiling fails, it gives a list of errors,
        /// intended to be showed in the plugin view.
        /// @param code C# Source code based on PluginBase class, to implement your plugin.
        /// @param className Class name of the plugin.
        /// @param references String array with auxiliary references used by your plugin.
        /// See notes for defaults used.
        /// @param obj Reference to a PropellerCPU of this instance, to be passed as a
        /// parameter to the constructor of the new plugin class instance.
        /// @returns New Plugin class instance compiled (on success), or NULL (on fail).
        /// @note There are some references already added, so you don't need to include on your plugins:
        /// @li `using System;` @li `using System.Data;` @li `using System.Drawing;`
        /// @li `using System.Windows.Forms;` @li `using System.Xml;`
        /// @version v20.08.01 - Added compiler version definition.
        static public PluginBase LoadModule(string code, string className,
            string[] references, object obj)
        {
            //set compiler version
            Dictionary<string, string> provOptions =
                new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } };
            //create new compiler version
            CodeDomProvider provider = new CSharpCodeProvider(provOptions);
            CompilerParameters cp = new CompilerParameters
            {
#if DEBUG
                IncludeDebugInformation = true,
#else
                IncludeDebugInformation = false,
#endif
                GenerateExecutable = false,
                GenerateInMemory = true,
                CompilerOptions = "/optimize"
            };

            cp.ReferencedAssemblies.Add(Application.ExecutablePath);
            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add("System.Data.dll");
            cp.ReferencedAssemblies.Add("System.Drawing.dll");
            cp.ReferencedAssemblies.Add("System.Xml.dll");
            cp.ReferencedAssemblies.Add("System.Windows.Forms.dll");

            foreach (string s in references)
                cp.ReferencedAssemblies.Add(s);

            CompilerResults results = provider.CompileAssemblyFromSource(cp, code);

            if (results.Errors.HasErrors | results.Errors.HasWarnings)
            {
                m_Errors = results.Errors;
                return null;
            }

            //compile plugin with parameters
            object target = results.CompiledAssembly.CreateInstance(
                className,                                    //name of class
                false,                                        //=false: case sensitive
                BindingFlags.Public | BindingFlags.Instance,  //flags to delimit the candidates
                null,                                         //default binder object
                new object[] { obj },                         //parameter lists
                null,                                         //default culture
                null                                          //default activation object
            );

            if (target == null)
            {
                CompilerError c = new CompilerError(string.Empty, 0, 0, "CS0103",
                    "The name '" + className + "' does not exist in the current context." +
                    " Does the class name is the same that is declared in c# code?");
                m_Errors = new CompilerErrorCollection(new CompilerError[] { c });
                return null;
            }
            else if (!(target is PluginBase))
            {
                CompilerError c = new CompilerError(string.Empty, 0, 0, "CS0029",
                    "Cannot implicitly convert type '" + target.GetType().FullName +
                    "' to 'Gear.PluginSupport.PluginBase'");
                m_Errors = new CompilerErrorCollection(new CompilerError[] { c });
                return null;
            }

            m_Errors = null;
            return (PluginBase)target;
        }
    }
}
