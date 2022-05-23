using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DefineManagement.Editor
{
    [CustomEditor(typeof(DefineManager))]
    public class DefineManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            DefineManager manager = (DefineManager)target;

            if (GUILayout.Button("Generate Definitions"))
            {
                manager.GetType()?.GetMethod("GenerateDefinitions", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(manager, null);
            }
        }
    }
}
