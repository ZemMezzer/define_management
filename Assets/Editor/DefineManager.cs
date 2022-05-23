using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DefineManagement.Editor
{
    [CreateAssetMenu(menuName = "Editor/Define Manager")]
    public class DefineManager : ScriptableObject
    {
        [SerializeField] private List<StringKeyValuePair> definitions;
        private readonly HashSet<string> defines = new HashSet<string>();

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnCompile()
        {
            string[] guids = AssetDatabase.FindAssets ("t:DefineManager", null);
            if(guids.Length<=0)
                return;
            
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            DefineManager manager = AssetDatabase.LoadAssetAtPath<DefineManager>(path);
            manager.GetType()?.GetMethod("GenerateDefinitions", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(manager, null);
        }
        
        private void GenerateDefinitions()
        {
            defines.Clear();

            foreach (var definition in PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';'))
            {
                defines.Add(definition);
            }
            CheckDefines();
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", defines));
        }
        
        private void OnValidate()
        {
            foreach (var define in definitions)
            {
                string currentKey = define.Type;

                for (int i = 0; i < definitions.Count; i++)
                {
                    var checkDefine = definitions[i];
                    
                    if(define == checkDefine)
                        continue;

                    if (currentKey == checkDefine.Type)
                    {
                        checkDefine.Type = $"{checkDefine.Type}(Copy)";
                        definitions[i] = checkDefine;
                        OnValidate();
                        return;
                    }
                }
            }
        }

        private void CheckDefines()
        {
            foreach (var definition in definitions)
            {
                if (defines.Contains(definition.Define))
                    defines.Remove(definition.Define);

                string assembly = "Assembly-CSharp";
                if (definition.OverrideAssembly)
                    assembly = definition.Assembly;
                
                Type type = Type.GetType($"{definition.Type}, {assembly}");
                if (type == null)
                {
                    Debug.LogError($"Type [{definition.Type}] in [{assembly}] is not exists");
                    continue;
                }
                
                defines.Add(definition.Define);
            }
        }
        
        [Serializable]
        private struct StringKeyValuePair
        {
            public string Type;
            public string Define;
            public bool OverrideAssembly;
            public string Assembly;

            public static bool operator ==(StringKeyValuePair f, StringKeyValuePair s)
            {
                return f.Type == s.Type && f.Define == s.Define && f.Assembly == s.Assembly && f.OverrideAssembly == s.OverrideAssembly;
            }

            public static bool operator !=(StringKeyValuePair f, StringKeyValuePair s)
            {
                return !(f == s);
            }
        }
    }
}
