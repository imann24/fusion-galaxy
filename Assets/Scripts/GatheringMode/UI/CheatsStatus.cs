using UnityEngine;
using System.Collections;

public class CheatsStatus : MonoBehaviour {
    // Cheats title object
    public GameObject cheatsTitle;

	public void Start()
    {
        if (!GlobalVars.RELEASE_BUILD)
        {
            cheatsTitle.SetActive(true);
        }
    }
}
