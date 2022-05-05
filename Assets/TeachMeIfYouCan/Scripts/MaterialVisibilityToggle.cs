using UnityEngine;

/* To fix gown skinning issues */
public class MaterialVisibilityToggle : MonoBehaviour {
	public new Renderer renderer;
	public Material material;

	void Start() {
		Hide();
	}

	private void Hide() {
		Material[] newMaterials = new Material[renderer.materials.Length];

		for (int i = 0; i < renderer.materials.Length; i++) {
			if (i == 2)
				newMaterials[i] = material;
			else
				newMaterials[i] = renderer.materials[i];
		}
		renderer.materials = newMaterials;
	}

}
