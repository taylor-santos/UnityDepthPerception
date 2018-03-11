using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureProjection : MonoBehaviour {
	public RenderTexture RT;
	public RenderTexture result;
	public Transform objectTransform;
	public Matrix4x4 projection;
	private Material shader;
	// Use this for initialization
	void Start () {
		result = new RenderTexture(RT.width, RT.height, RT.depth);
		shader = new Material(Shader.Find("Custom/TextureProjection"));
	}
	
	// Update is called once per frame
	void Update () {
		shader.SetMatrix("_Projection", projection);
		Graphics.Blit(RT, result, shader);
	}

	void OnGUI () {
		GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), result);
	}
}
