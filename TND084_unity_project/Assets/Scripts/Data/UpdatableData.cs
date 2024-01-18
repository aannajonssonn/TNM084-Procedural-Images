using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatableData : ScriptableObject
{
    // event called when values are updated
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

    protected virtual void OnValidate()
    {
        if (autoUpdate)
        {
            UnityEditor.EditorApplication.update += NotifyofUpdatedValues;
        }
    }

    public void NotifyofUpdatedValues()
    {
        UnityEditor.EditorApplication.update -= NotifyofUpdatedValues;
        if (OnValuesUpdated != null) OnValuesUpdated();

    }
}
