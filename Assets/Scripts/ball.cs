using UnityEngine;
using System.Collections;

public class ball : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		//ゲームオブジェクトを5秒後にDestroy
       	Destroy(gameObject, 3);
		// スコアコンポーネントを取得してポイントを追加
        FindObjectOfType<score>().AddPoint(100);

	}
}
