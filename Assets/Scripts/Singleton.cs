using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton pattern.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : Component
{
    protected static T _instance;

    /// <summary>
    /// Singleton design pattern
    /// </summary>
    /// <value>The instance.</value>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
            }
            return _instance;
        }
    }

    /// <summary>
    /// Returns whether an Instance exists or not.
    /// </summary>
    public static bool InstanceExists
    {
        get
        {
            return _instance != null;
        }
    }

    /// <summary>
    /// On awake, we initialize our instance. Make sure to call base.Awake() in override if you need awake.
    /// </summary>
    protected virtual void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        _instance = this as T;
    }
}
