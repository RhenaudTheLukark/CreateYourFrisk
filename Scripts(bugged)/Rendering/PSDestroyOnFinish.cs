using UnityEngine;
using System.Collections;

public class PSDestroyOnFinish : MonoBehaviour {
    ParticleSystem ps;
    void Start() { ps = GetComponent<ParticleSystem>(); }
	void Update () {
        if (ps.isStopped) { Destroy(this.gameObject); }
	}
}
