using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobileParadise.Core.Localization
{
    /// <summary>
    /// Tiny two-language text table (Portuguese / English).
    /// Each game registers its own keys with Set(), so Core stays game-agnostic.
    /// Until the player picks a language, the phone's language is used.
    /// </summary>
    public static class Texts
    {
        public const string Portuguese = "pt";
        public const string English = "en";

        private static readonly Dictionary<string, string> PortugueseTable = new Dictionary<string, string>();
        private static readonly Dictionary<string, string> EnglishTable = new Dictionary<string, string>();
        private static string _language;

        /// <summary>Raised after the language changes, so visible texts can refresh.</summary>
        public static event Action LanguageChanged;

        public static string Language
        {
            get
            {
                if (_language == null) _language = ResolveSystemLanguage();
                return _language;
            }
        }

        public static bool IsPortuguese
        {
            get { return Language == Portuguese; }
        }

        public static string ResolveSystemLanguage()
        {
            return Application.systemLanguage == SystemLanguage.Portuguese ? Portuguese : English;
        }

        public static void SetLanguage(string code)
        {
            if (code != Portuguese && code != English)
            {
                code = ResolveSystemLanguage();
            }
            if (code == Language) return;

            _language = code;
            if (LanguageChanged != null)
            {
                LanguageChanged();
            }
        }

        public static void Set(string key, string portuguese, string english)
        {
            PortugueseTable[key] = portuguese;
            EnglishTable[key] = english;
        }

        public static bool Has(string key)
        {
            return PortugueseTable.ContainsKey(key);
        }

        public static string Get(string key)
        {
            Dictionary<string, string> table = IsPortuguese ? PortugueseTable : EnglishTable;

            string value;
            return table.TryGetValue(key, out value) ? value : key;
        }
    }
}
