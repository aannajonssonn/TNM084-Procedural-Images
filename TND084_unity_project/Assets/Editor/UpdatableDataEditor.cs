using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UpdatableData), true)]
public class UpdatableDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // reference to updatable data
        UpdatableData data = (UpdatableData)target;

        if (GUILayout.Button("Update"))
        {
            data.NotifyofUpdatedValues();
            EditorUtility.SetDirty(target);
        }
    }
}
