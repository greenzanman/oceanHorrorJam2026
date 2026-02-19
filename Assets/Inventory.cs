using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private List<GameObject> items = new List<GameObject>();

    void Awake()
    {
        Pickup.OnInteract += HandlePickup;
    }

    public void AddItem(GameObject item)
    {
        items.Add(item);
    }

    public void RemoveItem(GameObject item)
    {
        items.Remove(item);
    }

    public List<GameObject> GetItems()
    {
        return items;
    }

    void HandlePickup(Pickup pickup)
    {
        GameObject item = pickup.gameObject;
        AddItem(item);
        Debug.Log("Added item to inventory: " + item.name);
    }

    public GameObject Contains(string itemName, bool startsWith)
    {
        foreach (GameObject item in items)
        {
            if (startsWith && item.name.StartsWith(itemName))
            {
                Debug.Log("Inventory contains: " + itemName);
                return item;
            }
            else if (!startsWith && item.name == itemName)
            {
                Debug.Log("Inventory contains: " + itemName);
                return item;
            }
        }
        Debug.Log("Inventory does not contain: " + itemName);
        return null;
    }
}
