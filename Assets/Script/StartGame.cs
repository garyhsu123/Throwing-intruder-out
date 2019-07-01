using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour {

    public void Awake()
    {
        Cursor.visible = true;
    }
	public void changemenuscenen(string scenename)
    {
        Application.LoadLevel (scenename);
    }
    public void stopGame()
    {
        Application.Quit();
    }
}
