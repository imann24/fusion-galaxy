using UnityEngine;
using System.Collections;

public class CheatsStatus : MonoBehaviour {
    // Cheats title object
    public GameObject cheatsTitle;
    void Start()
    {
        CheatCheck();
    }
	public void CheatCheck()
    {
        if (!GlobalVars.RELEASE_BUILD)
        {
            cheatsTitle.SetActive(true);
        }
    }
}
