using UnityEngine;
using System.Collections;

/// <summary>
/// Utility behaviour for gyrating any dogs that appear onscreen. Can be used with non-dog objects, but not recommended.
/// </summary>
public class DogGyrator : MonoBehaviour {
    RectTransform dog;

    /// <summary>
    /// Initialize dog.
    /// </summary>
    void Start()
    {
        dog = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Gyrate dog.
    /// </summary>
	void Update () {
        dog.Rotate(Vector3.forward * 90 * Time.deltaTime);
	}
}
