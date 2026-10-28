using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

namespace ProjectStarwatch.Editor.DebugTools
{
    /// <summary>
    /// Scripting-define helpers for Starwatch debug/prod compile modes.
    /// </summary>
    internal static class StarwatchDebugDefines
    {
        /// <summary>
        /// Project debug gate. Prefer <c>#if STARWATCH_DEBUG</c> over <c>#if DEBUG</c>:
        /// Unity / the IDE already manage <c>DEBUG</c> for Editor and Development builds.
        /// </summary>
        public const string DebugSymbol = "STARWATCH_DEBUG";

        public static bool IsDebugModeEnabled
        {
            get => HasDefine(DebugSymbol);
            set
            {
                if (value)
                {
                    AddDefine(DebugSymbol);
                }
                else
                {
                    RemoveDefine(DebugSymbol);
                }
            }
        }

        public static bool HasDefine(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return false;
            }

            return GetDefines().Contains(symbol);
        }

        public static void AddDefine(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return;
            }

            var defines = GetDefines();
            if (defines.Contains(symbol))
            {
                return;
            }

            defines.Add(symbol);
            SetDefines(defines);
        }

        public static void RemoveDefine(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return;
            }

            var defines = GetDefines();
            if (!defines.Remove(symbol))
            {
                return;
            }

            SetDefines(defines);
        }

        private static List<string> GetDefines()
        {
            var raw = PlayerSettings.GetScriptingDefineSymbols(ActiveNamedBuildTarget());
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new List<string>();
            }

            var parts = raw.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var defines = new List<string>(parts.Length);
            for (var i = 0; i < parts.Length; i++)
            {
                var symbol = parts[i].Trim();
                if (symbol.Length > 0 && !defines.Contains(symbol))
                {
                    defines.Add(symbol);
                }
            }

            return defines;
        }

        private static void SetDefines(List<string> defines)
        {
            defines.Sort(StringComparer.Ordinal);
            PlayerSettings.SetScriptingDefineSymbols(ActiveNamedBuildTarget(), string.Join(";", defines));
        }

        private static NamedBuildTarget ActiveNamedBuildTarget()
        {
            return NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        }
    }
}
