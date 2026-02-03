using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPopUp : MonoBehaviour
{
    public static event Action<PickupLogic> UITextDestroyed;
    [SerializeField] public float fadeDuration = 2f;

    private Coroutine destroyCoroutine;

    
    
    // Start is called before the first frame update
    void Start()
    {
        // Start destroy coroutine
        destroyCoroutine = StartCoroutine(DestroyAfterTime(fadeDuration));
        
    }

    IEnumerator DestroyAfterTime(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(this.gameObject);
        UITextDestroyed?.Invoke(null);
    }
}
