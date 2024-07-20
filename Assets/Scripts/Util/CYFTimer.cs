using System;
using UnityEngine;

public class CYFTimer {
    private readonly float triggerTime = 0;
    private float startTime = 0;
    private bool elapsing = false;
    private readonly Action func;

    public CYFTimer(float triggerTime, Action func) {
        this.triggerTime = triggerTime;
        this.func = func;
    }

    public void Start() {
        startTime = Time.time;
        elapsing = true;
    }

    public void Stop() {
        elapsing = false;
    }

    public bool IsElapsing() {
        return elapsing;
    }

    public void Update() {
        if (elapsing)
            if (Time.time - startTime > triggerTime) {
                func();
                Stop();
            }
    }
}
