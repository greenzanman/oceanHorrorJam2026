using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
    * Interface for pickups and interactables. Both have a short and long description, and can be interacted with.
    * The actionable controller listens for interact input and fires the OnInteractInput event with the actionable in view as a parameter.
    * Interactables and pickups subscribe to this event and check if they are in view before firing their own events.
*/
public interface Actionable
{

    public static event Action<MonoBehaviour> OnInteract;
    // This class is a marker
    void Fire();
    void SafeFire();
}