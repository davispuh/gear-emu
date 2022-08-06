/* --------------------------------------------------------------------------------
 * Gear: Parallax Inc. Propeller P1 Emulator
 * Copyright 2007-2022 - Gear Developers
 * --------------------------------------------------------------------------------
 * TabManager.cs
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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Gear.PluginSupport;

namespace Gear.GUI
{
    /// <summary>Manage document tabs of a %Emulator.</summary>
    /// @version v22.06.01 - Class created to maintain tabs ordered
    /// on a %TabControl.
    public class TabManager
    {
        /// <summary>Tab Control to be managed.</summary>
        private readonly TabControl _tabControlManaged;

        /// <summary>Ordered list of tabbed documents, containing cogs or
        /// plugins of emulator.</summary>
        private readonly List<string> _tabOrderedList = new List<string>();
        /// <summary>List for counting instances of each plugin type.</summary>
        private readonly SortedList<string, uint> _tabsInstancesQtyList =
            new SortedList<string, uint>();

        /// <summary>Pattern for parse name of a tab.</summary>
        private const string PatternParseNameTab = @"^(?<base>[a-zA-Z _-]+)\d*.*";

        /// <summary>Regex to parse names.</summary>
        private readonly Regex _parseNameTabRegex =
            new Regex(PatternParseNameTab, RegexOptions.Compiled |
                                           RegexOptions.CultureInvariant);

        /// <summary>Counts how many tabs are contained in floating windows.</summary>
        /// @version v22.07.xx - Changed name from former
        /// `FloatingTabsQuantity`, to shorter name.
        public int FloatingTabsQty { get; set; }

        /// <summary>Counts how many tabs are pinned in another object and not
        /// into document tab control.</summary>
        /// @version v22.07.xx - Changed type and name of property from former
        /// `TabPinnedExist`, to correct error on restore in wrong position
        /// a unpinned tab when other where selected to pinned.
        public int TabPinnedQty { get; set; }

        /// <summary>Default constructor.</summary>
        /// <param name="tabControlToManage">Tab Control to be managed.</param>
        public TabManager(TabControl tabControlToManage)
        {
            _tabControlManaged = tabControlToManage;
            FloatingTabsQty = 0;
            TabPinnedQty = 0;
        }

        /// <summary>Extract the base name of a tab text.</summary>
        /// <param name="fullName">Name of a tab text.</param>
        /// <returns>Base name.</returns>
        /// <exception cref="ArgumentException">If full name does not follow
        /// the pattern: to start with letters.</exception>
        private string GetBaseNameTabInstance(string fullName)
        {
            Match firstMatch = _parseNameTabRegex.Match(fullName);
            if (!firstMatch.Success)
                throw new ArgumentException(
                    $"Name '{fullName}' is not valid. It does not start with letters.");
            return firstMatch.Groups.Count > 1 ?
                firstMatch.Groups[1].Value :
                firstMatch.Groups[0].Value;
        }

        /// <summary>Insert tab name to Instance List, changing
        /// name appropriately to a specified numbering generator.</summary>
        /// <param name="name">Name of the tab.</param>
        /// <param name="generator">Delegate for generate tab names on
        /// repetition of a plugin name.</param>
        /// <returns>Complete name for the tab, considering repetitions.</returns>
        private string InsertTabToInstanceList(string name,
            QuantityPostFixGenerator generator)
        {
            try
            {
                string baseName = GetBaseNameTabInstance(name);
                string postFixQuantity;
                if (_tabsInstancesQtyList.ContainsKey(baseName))
                    //increment quantity of instances of this plugin
                    postFixQuantity = generator(_tabsInstancesQtyList[baseName]++);
                else
                {
                    //if not exist, add to List
                    _tabsInstancesQtyList.Add(baseName, 1);
                    postFixQuantity = generator(0);
                }
                return $"{baseName}{postFixQuantity}";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,
                    $@"Technical Error on {e.Source}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return string.Empty;
        }

        /// <summary>Remove tab name from Instance List or decrement
        /// its number of instances on the List.</summary>
        /// <param name="name">Complete name of the tab.</param>
        private void RemoveTabFromInstanceList(string name)
        {
            try
            {
                string baseName = GetBaseNameTabInstance(name);
                if (!_tabsInstancesQtyList.ContainsKey(baseName))
                    return;
                if (_tabsInstancesQtyList[baseName] > 1)
                    --_tabsInstancesQtyList[baseName];
                else
                    _tabsInstancesQtyList.Remove(baseName);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,
                    $@"Technical Error on {e.Source}",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>Add a plugin to a managed Tab Control,
        /// remembering its position.</summary>
        /// <param name="newPlugin">Plugin to add into a Tab Control.</param>
        /// <param name="generator">Delegate for generate tab names on
        /// repetition of a plugin name.</param>
        public void AddToTabControl(PluginBase newPlugin,
            QuantityPostFixGenerator generator)
        {
            //get new name of tab
            string newTitle = InsertTabToInstanceList(newPlugin.Title, generator);
            if (string.IsNullOrWhiteSpace(newTitle))
                newTitle = "Nameless Plugin";
            TabPage newTabPage = new TabPage(newTitle);
            //maintain tabs order
            _tabOrderedList.Add(newTitle);
            //add tab to the end
            _tabControlManaged.TabPages.Add(newTabPage);
            newPlugin.Text = newTitle;
            newPlugin.Dock = DockStyle.Fill;
            newPlugin.Parent = newTabPage;
            _tabControlManaged.SelectedTab = newTabPage;
        }

        /// <summary>Remove plugin and its related tab page from a managed
        /// Tab Control.</summary>
        /// <param name="tabPage">Document tab to remove from a Tab Control.</param>
        /// <param name="plugin">Plugin to remove from a Tab Control.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void RemoveFromTabControl(TabPage tabPage, PluginBase plugin)
        {
            if (tabPage == null)
                throw new ArgumentNullException(nameof(tabPage));
            if (plugin == null)
                throw new ArgumentNullException(nameof(plugin));
            RemoveTabFromInstanceList(tabPage.Text);
            if (_tabOrderedList.Contains(tabPage.Text))
            {
                int idxToRemove = _tabOrderedList.IndexOf(tabPage.Text);
                _tabOrderedList.RemoveAt(idxToRemove);
            }
            plugin.Parent = null;
            _tabControlManaged.TabPages.Remove(tabPage);
        }

        /// <summary>Restore a control for a plugin on its last position
        /// of the managed Tab Control.</summary>
        /// <param name="control"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// @version v22.07.xx - Corrected error on restore in wrong position
        /// a unpinned tab when other where selected to pinned.
        public void RestoreToTabControl(Control control)
        {
            int idxFound;
            if (control == null)
                throw new ArgumentNullException(nameof(control));
            if (_tabOrderedList.Contains(control.Text))
                idxFound = _tabOrderedList.IndexOf(control.Text);
            else
                throw new ArgumentException(
                    $@"Control Text '{control.Text}' do not exists in Tab Names List!",
                    nameof(control));
            TabPage newTabPage = new TabPage(control.Text);
            control.Dock = DockStyle.Fill;
            control.Parent = newTabPage;
            //calculate the position to reinsert the tab
            int pos = Math.Min(idxFound,
                _tabOrderedList.Count - 1 - FloatingTabsQty - TabPinnedQty);
            //insert tab in saved position
            _tabControlManaged.TabPages.Insert(pos, newTabPage);
            _tabControlManaged.SelectedTab = newTabPage;
        }

        /// <summary>Definition of signature of generators for name
        /// tabs repetitions.</summary>
        /// <param name="quantity">Number of instance.</param>
        /// <returns>Text to append on tab name.</returns>
        public delegate string QuantityPostFixGenerator(uint quantity);

        /// <summary> Generator that always appends a number,
        /// starting at zero.</summary>
        /// <param name="quantity">Number of instance.</param>
        /// <returns>Text to append on tab name.</returns>
        public static string NumericZeroBased(uint quantity)
        {
            return $"{quantity}";
        }

        /// <summary>
        /// Generator that append numbers only after second instance, starting
        /// at 1 on repetition.
        /// </summary>
        /// <param name="quantity">Number of instance.</param>
        /// <returns>Text to append on tab name.</returns>
        public static string OnlyRepetitionNumberedFromOne(uint quantity)
        {
            return quantity == 0 ?
                string.Empty :
                $"{quantity}";
        }
    }
}
