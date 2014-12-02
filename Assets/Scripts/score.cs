using UnityEngine;
using System.Collections;



public class score : MonoBehaviour {

public GUIText scoreGUIText;

// スコア
private int s;

	// Use this for initialization
	void Start () {
	
		s = 0;
	}
	
	// Update is called once per frame
	void Update () {

		scoreGUIText.text = "Score : " + s.ToString();

	
	}

	public void AddPoint (int point)
    {
    	s= s + point;
    }
}
