using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// interface for make task(sensor) interactable
/// </summary>
public interface IInteractable<T> {
    void CheckValue(T value);
}