/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * AboutGear.cs
 * About box for Gear Emulator.
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

using System.Reflection;
using System.Windows.Forms;

namespace Gear.GUI
{
    /// @brief About form for %Gear %Emulator.
    public partial class AboutGear : Form
    {
        /// <summary>Property to get/set the Form text.</summary>
        /// @version v22.06.01 - Added.
        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        /// <summary>Default constructor.</summary>
        /// <remarks>Initialize the AboutBox to display the product information
        /// from the assembly information.</remarks>
        /// @version v22.06.01 - Changed to string interpolation.
        public AboutGear()
        {
            InitializeComponent();

            //  Initialize the AboutBox to display the product information from the assembly information.
            //  Change assembly information settings for your application through either:
            //  - Project->Properties->Application->Assembly Information
            //  - AssemblyInfo.cs
            Text = $@"About {AssemblyTitle}";
            labelProductName.Text = AssemblyProduct;
            labelVersion.Text = $@"Version: {AssemblyVersion}";
            labelCopyright.Text = AssemblyCopyright;
            labelCompanyName.Text = $@"Project: {AssemblyCompany}";
            textBoxDescription.Text = AssemblyDescription;
        }

        #region Assembly Attribute Accessors

        /// <summary>Returns the Assembly title.</summary>
        /// @version v22.06.01 - Added management for an empty Title on return value.
        public static string AssemblyTitle
        {
            get
            {
                // Get all Title attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                // If there is at least one Title attribute
                if (attributes.Length <= 0)
                    return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
                // Select the first one
                AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                // If it is not an empty string, return it
                // If there was no Title attribute, or if the Title attribute was the empty string, return the .exe name
                return string.IsNullOrEmpty(titleAttribute.Title) ?
                    System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase) :
                    titleAttribute.Title;
            }
        }

        /// <summary>Returns the assembly Version.</summary>
        /// @version v22.06.01 - Added static access to Property.
        public static string AssemblyVersion =>
            Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>Returns the assembly Description.</summary>
        /// @version v22.06.01 - Added static access to Property.
        public static string AssemblyDescription
        {
            get
            {
                // Get all Description attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                return attributes.Length == 0 ?
                    string.Empty :
                    ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        /// <summary>Returns the assembly Product name.</summary>
        public static string AssemblyProduct
        {
            get
            {
                // Get all Product attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                return attributes.Length == 0 ?
                    string.Empty :
                    ((AssemblyProductAttribute)attributes[0]).Product;
           }
        }

        /// <summary>Returns the assembly Copyright text.</summary>
        public static string AssemblyCopyright
        {
            get
            {
                // Get all Copyright attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                return attributes.Length == 0 ?
                    string.Empty :
                    ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        /// <summary>Returns the assembly Company text.</summary>
        public static string AssemblyCompany
        {
            get
            {
                // Get all Company attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                return attributes.Length == 0 ?
                    string.Empty :
                    ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
        #endregion

    }
}
