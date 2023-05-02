using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class s_Manager : MonoBehaviour
{
 
    public bool b_enableFpsDebug;


    void Update()
    {
        if (b_enableFpsDebug)
        {
            //FPS Debug
            Debug.Log(1 / Time.deltaTime);
        }
    }
    
}
