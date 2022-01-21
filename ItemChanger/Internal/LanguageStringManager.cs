﻿using System.Xml;
using System.Reflection;
using Modding;

namespace ItemChanger.Internal
{
    internal static class LanguageStringManager
    {
        private static readonly Dictionary<string, Dictionary<string, string>> LanguageStrings = new();
        private static bool loaded;

        internal static void Load()
        {
            if (!loaded)
            {
                Assembly a = typeof(ItemChangerMod).Assembly;
                Stream xmlStream = a.GetManifestResourceStream("ItemChanger.Resources.language.xml");

                // Load XmlDocument from resource stream
                XmlDocument xml = new();
                xml.Load(xmlStream);
                xmlStream.Dispose();

                XmlNodeList nodes = xml.SelectNodes("Language/entry");
                if (nodes == null)
                {
                    LogWarn("Malformatted language xml, no nodes that match Language/entry");
                    return;
                }

                foreach (XmlNode node in nodes)
                {
                    string sheet = node.Attributes?["sheet"]?.Value;
                    string key = node.Attributes?["key"]?.Value;

                    if (sheet == null || key == null)
                    {
                        LogWarn("Malformatted language xml, missing sheet or key on node");
                        continue;
                    }

                    SetString(sheet, key, node.InnerText.Replace("\\n", "\n"));
                }
            }
            loaded = true;
        }

        internal static void Hook()
        {
            ModHooks.LanguageGetHook += GetLanguageString;
        }

        internal static void Unhook()
        {
            ModHooks.LanguageGetHook -= GetLanguageString;
        }

        private static void SetString(string sheetName, string key, string text)
        {
            if (string.IsNullOrEmpty(sheetName) || string.IsNullOrEmpty(key) || text == null)
            {
                return;
            }

            if (!LanguageStrings.TryGetValue(sheetName, out Dictionary<string, string> sheet))
            {
                sheet = new Dictionary<string, string>();
                LanguageStrings.Add(sheetName, sheet);
            }

            sheet[key] = text;
        }

        private static void ResetString(string sheetName, string key)
        {
            if (string.IsNullOrEmpty(sheetName) || string.IsNullOrEmpty(key))
            {
                return;
            }

            if (LanguageStrings.TryGetValue(sheetName, out Dictionary<string, string> sheet) && sheet.ContainsKey(key))
            {
                sheet.Remove(key);
            }
        }

        // keep this private -- the api hook does weird stuff with GetInternal
        private static string GetLanguageString(string key, string sheetTitle, string orig)
        {
            if (sheetTitle.StartsWith("Internal"))
            {
                return Language.Language.GetInternal(key, sheetTitle[8..]);
            }

            if (sheetTitle == "Exact")
            {
                return key;
            }

            if (key.StartsWith("ITEMCHANGER_NAME_ESSENCE_"))
            {
                return key["ITEMCHANGER_NAME_ESSENCE_".Length..] + " 精华";
            }

            if (key.StartsWith("ITEMCHANGER_NAME_GEO_"))
            {
                return key["ITEMCHANGER_NAME_GEO_".Length..] + " 吉欧";
            }

            if (key == "ITEMCHANGER_POSTVIEW_GRUB")
            {
                return $"一只幼虫! ({PlayerData.instance.GetInt(nameof(PlayerData.grubsCollected))}/46)";
            }

            if (key == "ITEMCHANGER_POSTVIEW_GRIMMKIN_FLAME")
            {
                int flames = PlayerData.instance.GetInt("cumulativeFlamesCollected");
                if (flames <= 0) flames = PlayerData.instance.GetInt(nameof(PlayerData.flamesCollected));
                return $"格林之火 ({flames}/10)";
            }


            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(sheetTitle))
            {
                return string.Empty;
            }

            if (LanguageStrings.ContainsKey(sheetTitle) && LanguageStrings[sheetTitle].ContainsKey(key))
            {
                return LanguageStrings[sheetTitle][key];
            }

            return orig;
        }
    }
}
