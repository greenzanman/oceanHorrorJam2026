using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextPanels : MonoBehaviour
{
    [SerializeField] private List<GameObject> nextPanels;

    public List<GameObject> GetNextPanels()
    {
        return nextPanels;
    }
}
