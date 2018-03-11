using UnityEngine;
using System.Collections;

public class lightScaler : MonoBehaviour {
	public float scale;
	private Light l;
	private GameObject obj;
	// Use this for initialization
	void Start () {
		l = GetComponent<Light>();
		obj = gameObject;
		while (obj.transform.parent!=null)
			obj = obj.transform.parent.gameObject;
	}
	
	// Update is called once per frame
	void Update () {


		l.range = Mathf.Max(obj.transform.localScale.x, obj.transform.localScale.y, obj.transform.localScale.z) * scale;
	}
}
