using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Xml
{
    public static class UIExtensions
    {
        /// <summary>
        /// Set the selected value of this Dropdown to 'value'.
        /// </summary>
        /// <param name="dropdown"></param>
        /// <param name="value"></param>
        public static void SetSelectedValue(this Dropdown dropdown, string value, int attempt = 0)
        {
            var option = dropdown.options.FirstOrDefault(o => o.text.Equals(value, StringComparison.OrdinalIgnoreCase));

            // If the user sets the options and the value in one frame,
            // the options property may not yet be populated correctly
            // so if this happens, we try wait a few frames
            if (option == null && attempt < 3)
            {
                XmlLayoutTimer.AtEndOfFrame(() => dropdown.SetSelectedValue(value, ++attempt), dropdown);
                return;
            }

            if (option != null)
            {
                dropdown.value = dropdown.options.IndexOf(option);
                dropdown.RefreshShownValue();
            }
            else
            {
                Debug.Log("Dropdown.SetSelectedValue :: Value '" + value + "' was not found in dropdown '" + dropdown.name + "'.");
            }
        }

        public static void SetSelectedValue(this Dropdown dropdown, int value)
        {
            dropdown.value = value;
            dropdown.RefreshShownValue();
        }

        public static void SetOptions(this Dropdown dropdown, IEnumerable<string> options)
        {
            dropdown.options = options.Select(s => new Dropdown.OptionData(s)).ToList();
            dropdown.RefreshShownValue();
        }

        public static void SetOptions(this Dropdown dropdown, params string[] options)
        {
            dropdown.options = options.Select(s => new Dropdown.OptionData(s)).ToList();
            dropdown.RefreshShownValue();
        }

        public static string GetSelectedValue(this Dropdown dropdown)
        {
            return dropdown.options[dropdown.value].text;
        }

        /// <summary>
        /// This is primarily used by ToggleButton to change the color when the element is on/off
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="normalColor"></param>
        /// <returns></returns>
        public static ColorBlock SetNormalColor(this ColorBlock colors, Color normalColor)
        {
            ColorBlock c = colors;
            c.normalColor = normalColor;

            return c;
        }
    }
}
