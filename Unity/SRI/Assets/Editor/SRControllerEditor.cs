//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//[CustomEditor(typeof(SRController))]
//public class RoomItemEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();
//        var controller = target as SRController;
//        EditorGUIUtility.LookLikeInspector();
//        SerializedProperty tps = serializedObject.FindProperty("motors");
//        EditorGUI.BeginChangeCheck();
//        EditorGUILayout.PropertyField(tps, true);
//        if (EditorGUI.EndChangeCheck())
//            serializedObject.ApplyModifiedProperties();
//        EditorGUIUtility.LookLikeControls();
//        // ...
//    }
//}
