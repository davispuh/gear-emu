/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller Debugger
 * Copyright 2007 - Robert Vandiver
 * --------------------------------------------------------------------------------
 * ModuleLoader.cs
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

using System;
using System.Collections.Generic;
using System.Text;

//using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

/// @todo Document Gear.PluginSupport namespace.
/// 
namespace Gear.PluginSupport
{
    public delegate void ErrorEnumProc(System.CodeDom.Compiler.CompilerError e);

    /// @todo Document Gear.PluginSupport.ModuleLoader class.
    /// 
    static class ModuleLoader
    {
        static private CompilerErrorCollection m_Errors;    //!< @brief Collection for error list on compile a dynamic plugin.

        /// @brief ModuleLoader Constructor.
        /// Clear error list by default.
        static ModuleLoader()
        {
            m_Errors = null;
        }

        static public void EnumerateErrors(ErrorEnumProc proc)
        {
            if (m_Errors == null)
                return;

            foreach (CompilerError e in m_Errors)
                proc(e);
        }

        /// @brief Dynamic compiling & loading for a plugin.
        /// Try to dynamically compile a module for the plugin, based on supplied C# code and other 
        /// C# modules referenced. If the compiling fails, it gives a list of errors, intended to be 
        /// showed in the plugin view.
        /// @param[in] code C# Source code based on PluginBase class, to implement your plugin
        /// @param[in] module Class name of the plugin
        /// @param[in] references `string` array with auxiliary references used by your plugin. See notes for defaults used.
        /// @returns New Plugin class instance compiled (on sucess), or NULL (on fail).
        /// @note There are some references already added, so you don't need to include on your plugins: 
        /// @li `using System;` @li `using System.Data;` @li `using System.Drawing;`
        /// @li `using System.Windows.Forms;` @li `using System.Xml;`
        static public PluginBase LoadModule(string code, string module, string[] references)
        {
            CodeDomProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
            CompilerParameters cp = new CompilerParameters();

            cp.IncludeDebugInformation = false;
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = true;
            cp.CompilerOptions = "/optimize";

            cp.ReferencedAssemblies.Add(System.Windows.Forms.Application.ExecutablePath);

            cp.ReferencedAssemblies.Add("System.Windows.Forms.dll");
            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add("System.Data.dll");
            cp.ReferencedAssemblies.Add("System.Drawing.dll");
            cp.ReferencedAssemblies.Add("System.Xml.dll");

            foreach (string s in references)
                cp.ReferencedAssemblies.Add(s);

            CompilerResults results = provider.CompileAssemblyFromSource(cp, code);

            if (results.Errors.HasErrors)
            {
                m_Errors = results.Errors;
                return null;
            }

            object target = results.CompiledAssembly.CreateInstance(module);

            if (target == null)
            {
                CompilerError c = new CompilerError("", 0, 0, "CS0103",
                    "The name '" + module + "' does not exist in the current context." +
                    " Does the class name is the same that is declared in c# code?");
                m_Errors = new CompilerErrorCollection(new CompilerError[] { c });
                return null;
            }
            else if (!(target is PluginBase))
            {
                CompilerError c = new CompilerError("", 0, 0, "CS0029",
                    "Cannot implicitly convert type '" + target.GetType().FullName +
                    "' to 'Gear.PluginSupport.BusModule'");
                m_Errors = new CompilerErrorCollection(new CompilerError[] { c });
                return null;
            }

            m_Errors = null;
            return (PluginBase)target;
        }
    }
}
