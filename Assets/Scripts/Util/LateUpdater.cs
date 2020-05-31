using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Stupid utility class that waits a frame before running whatever's inside because RectTransform's inherited positions aren't accurate on startup. Nice.
/// </summary>
public class LateUpdater : MonoBehaviour {
    public static List<Action> lateInit = new List<Action>();
    public static List<Action> lateActions = new List<Action>();
    int frametimer = 0;

    public static void Init() { InvokeList(lateInit); }

    void Update () {
        if (frametimer > 0) {
            InvokeList(lateActions);
            Destroy(this);
        }
        frametimer++;
    }

    private static void InvokeList(List<Action> l){
        foreach (Action a in l)
            a.Invoke();
        l.Clear();
    }
}
