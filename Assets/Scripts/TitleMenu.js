#pragma strict

function OnGUI()
{
	if (GUI.Button (Rect (Screen.width / 2 - 100, Screen.height / 2 , 200, 50),"START"))
	{
		Application.LoadLevel("UnityKinect2Miku");
	}
}