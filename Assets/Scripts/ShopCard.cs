using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopCard : MonoBehaviour
{
    public GameObject unitPrefab;
    public Camera manequinCamera;
    public Text unitName;
    public Text unitCost;

    public void OnClick()
    {
        if (GameManager.Instance.client.AddUnit(unitPrefab))
            this.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        //Play VFX or animation accordingly
    }
}
