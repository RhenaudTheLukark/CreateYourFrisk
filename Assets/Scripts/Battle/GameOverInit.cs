using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Initiates the death sequence. Used in the Game Over scene to make sure the player doesn't go looking for objects before the Game Over scene has loaded.
/// </summary>
public static class GameOverInit {
	public static void Launch() {
        //GameObject.Find("player").GetComponent<Image>().sprite = null;
        Camera.main.GetComponent<AudioSource>().clip = AudioClipRegistry.GetMusic("mus_gameover");
        GameObject.Find("GameOver").GetComponent<Image>().sprite = SpriteRegistry.Get("UI/spr_gameoverbg_0");
        GameObject.FindObjectOfType<GameOverBehavior>().StartDeath(null, null);
	}
}
