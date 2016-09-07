/*********************************************************************************
 *Copyright(C) 2015 by LiYupeng
 *All rights reserved.
 *FileName:     SerializableDictionary.cs
 *Author:       LiYupeng
 *Version:      1.0
 *UnityVersion：5.4.0f3
 *Date:         2016-09-07
 *Description:   
 *History:  
**********************************************************************************/
using UnityEngine;
using System.Collections;

using System.Collections.Generic;

/// Usage:
/// 
/// [System.Serializable]
/// class MyDictionary : SerializableDictionary<int, GameObject> {}
/// public MyDictionary dic;
///
[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    // We save the keys and values in two lists because Unity does understand those.
    [SerializeField]
    private List<TKey> _keys;
    [SerializeField]
    private List<TValue> _values;

    // Before the serialization we fill these lists
    public void OnBeforeSerialize()
    {
        _keys = new List<TKey>(this.Count);
        _values = new List<TValue>(this.Count);
        foreach (var kvp in this)
        {
            _keys.Add(kvp.Key);
            _values.Add(kvp.Value);
        }
    }

    // After the serialization we create the dictionary from the two lists
    public void OnAfterDeserialize()
    {
        this.Clear();
        int count = Mathf.Min(_keys.Count, _values.Count);
        for (int i = 0; i < count; ++i)
        {
            this.Add(_keys[i], _values[i]);
        }
    }
}
