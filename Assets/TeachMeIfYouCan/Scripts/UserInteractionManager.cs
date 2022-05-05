using TMPro;
using UnityEngine;

/* Prompts UI for the player's feedback on agent's action for interactive mode */
public class UserInteractionManager : MonoBehaviour {
	public GameObject userPromptPanel;
	public TextMeshProUGUI descriptionText;
	private Transition transition;

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	public void PromptUserUI(Transition transition) {
		this.transition = transition;
		string text = "NPC did " + transition.action.ToDescriptionString() + " " + transition.from.ToDescriptionString() + " and will move next state " + transition.to.ToDescriptionString();
		descriptionText.text = text;
		Time.timeScale = 0f;
		userPromptPanel.SetActive(true);
	}

	public void OnNegativeButtonClicked() {
		ProcessReward(-100);
	}

	public void OnPositiveButtonClicked() {
		ProcessReward(100);
	}

	/* Evaluate user's feedback in terms of correctness than update qvalue */
	private void ProcessReward(int reward) {
		int fromIndex = Statics.allPossibleStates.IndexOf(transition.from);
		int toIndex = Statics.allPossibleStates.IndexOf(transition.to);
		int expectedReward = Statics.rewards[fromIndex, toIndex];
		if (expectedReward * reward < 0) {
			ScoreManager.numberOfWrongDecisions++;
			Debug.Log(expectedReward);
			Debug.Log(reward);
		}
		Statics.qvalues[fromIndex, toIndex] += reward;
		transition = null;
		userPromptPanel.SetActive(false);
		Time.timeScale = 1f;
	}

}
