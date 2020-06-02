using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The bullet pool where Projectiles are drawn from for performance reasons.
/// </summary>
public class BulletPool : MonoBehaviour {
    public static BulletPool instance;
    public static int POOLSIZE = 100;
    private static Queue<Projectile> pool = new Queue<Projectile>();
    private static Projectile bPrefab; // bullet prefab
    //private static int currentProjectile = 0;

    /// <summary>
    /// Initialize the pool with POOLSIZE Projectiles ready to go.
    /// </summary>
    private void Start() {
        instance = this;
        bPrefab = Resources.Load<LuaProjectile>("Prefabs/LUAProjectile 1");

        pool.Clear();
        for (int i = 0; i < POOLSIZE; i++)
            createPooledBullet();
    }

    /// <summary>
    /// Creates a new Projectile and adds it to the pool. Used during instantion and when the pool is empty.
    /// </summary>
    private void createPooledBullet() {
        Projectile lp = Instantiate<Projectile>(bPrefab);
        lp.transform.SetParent(transform);
        lp.GetComponent<RectTransform>().position = new Vector2(-999, -999); // Move offscreen to be safe, but shouldn't be necessary.
        pool.Enqueue(lp);
        lp.gameObject.SetActive(false);
    }

    /// <summary>
    /// Retrieve a Projectile from the pool, or create a new one if it's empty.
    /// </summary>
    /// <returns>A Projectile object for further modification.</returns>
    public Projectile Retrieve() {
        if (pool.Count == 0)
            createPooledBullet();
        Projectile dq = pool.Dequeue(); // had some other stuff going on
        dq.renewController();
        return dq;
    }

    /// <summary>
    /// Return a projectile to the pool.
    /// </summary>
    /// <param name="p">Projectile to return</param>
    public void Requeue(Projectile p) {
        if (p.transform.parent != gameObject.transform)
            p.transform.SetParent(gameObject.transform);
        p.GetComponent<RectTransform>().position = new Vector2(-999, -999);
        p.gameObject.SetActive(false);
        pool.Enqueue(p);
    }
}