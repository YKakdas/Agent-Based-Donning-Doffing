using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuManager : MonoBehaviour {
	public GameObject startMenuPanel;
	public GameObject timer;

	// Start is called before the first frame update
	void Start() {
			
	}

	// Update is called once per frame
	void Update() {

	}

	public void OnTrainingButtonClicked() {
		ProgramManager.enableInteractions = false;
		SceneManager.LoadScene("TeachMeIfYouCan");
	}

	public void OnInteractiveButtonClicked() {
		ProgramManager.enableInteractions = true;
		SceneManager.LoadScene("TeachMeIfYouCan");
	}
}
