using UnityEngine;
using UnityEditor;

/// <summary>
/// Adds a menu option in GameObject -> Render to Cubemap, which creates a Cubemap from the position of an object in space
/// Each FPS Character Scene should have a "CubemapPosition" object, which needs to be selected when running "Render to Cubemap" - temporary cameras are added to this object in order to generate the Cubemap
/// </summary>

public class RenderCubemapWizard : ScriptableWizard
{
	public Transform renderFromPosition;
	public Cubemap cubemap;

	void OnWizardUpdate()
	{
		bool isValid = (renderFromPosition != null) && (cubemap != null);
	}

	void OnWizardCreate()
	{
		// create temporary camera for rendering
		GameObject go = new GameObject("CubemapCamera");
		go.AddComponent<Camera>();
		// place it on the object
		go.transform.position = renderFromPosition.position;
		go.transform.rotation = Quaternion.identity;
		// render into cubemap
		go.GetComponent<Camera>().RenderToCubemap(cubemap);

		// destroy temporary camera
		DestroyImmediate(go);
	}

	[MenuItem("GameObject/Render into Cubemap")]
	static void RenderCubemap()
	{
		ScriptableWizard.DisplayWizard<RenderCubemapWizard>(
			"Render cubemap", "Render!");
	}
}