using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private List<ShopCard> shopCards = new List<ShopCard>();
    private List<ShopManequin> unitPool;
    [SerializeField] private TextMeshProUGUI playerGold;
    private void Awake()
    {
        unitPool = GameManager.Instance.GetPlayerUnitPool();
        GameManager.Instance.preparationStart.AddListener(RefreshShop);
        GameManager.Instance.client.goldChange.AddListener(UpdatePlayerGold);
    }

    private void OnEnable()
    {
        unitPool = GameManager.Instance.GetPlayerUnitPool();
    }

    public void RefreshShop()
    {
        unitPool = GameManager.Instance.GetPlayerUnitPool();

        Debug.Log("unitPool");
        foreach (ShopCard card in shopCards)
        {
            ShopManequin manequin = unitPool[Random.Range(0, unitPool.Count)];
            card.unitPrefab = manequin.unit.gameObject;
            card.unitName.text = manequin.unit.unitName;
            card.unitCost.text = manequin.unit.cost.ToString();
            card.manequinCamera.transform.position = manequin.CameraPivot.position;
            card.manequinCamera.transform.rotation = manequin.CameraPivot.rotation;
            card.manequinCamera.orthographicSize = manequin.cameraSize;
            card.manequinCamera.nearClipPlane = manequin.nearPlane;
            card.gameObject.SetActive(true);
        }        
    }

    public void OnButtonRefresh()
    {
        if (GameManager.Instance.client.gold < 2) return;

        GameManager.Instance.client.AddGold(-2);
        RefreshShop();
    }

    public void OnButtonBuyXP()
    {
        if (GameManager.Instance.client.gold < 4) return;

        GameManager.Instance.client.AddGold(-4);
        GameManager.Instance.client.AddXP(4);
    }

    public void UpdatePlayerGold(int _gold)
    {
        playerGold.text = _gold.ToString();
    }
}
