using TMPro;
using UnityEngine;

/* Class for calculating the player's score at the end of the interactive mode */
public class ScoreManager : MonoBehaviour {
	public GameObject scorePanel;
	public static int numberOfWrongDecisions = 0;
	public Timer timer;
	public TextMeshProUGUI score;
	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	public void ShowScore() {
		int totalScore = 100;
		if (Timer.timeRemaining <= 0f) {
			totalScore = 50;
		}
		totalScore = totalScore - 10 * numberOfWrongDecisions;
		string scoreText = "";
		if (totalScore > 0) {
			scoreText = "Congratulations! You have tought the NPC to how to don the personnel protective equipment in correct order. " +
				"Your score regarding to remaining time and number of wrong decisions is " + totalScore + " out of 100";
		} else {
			scoreText = "Unfortunately you couldn't get any score points in terms of the number of correct decisions and time constraints.";
		}
		scorePanel.SetActive(true);
		score.SetText(scoreText);
	}

	public bool CheckScore() {
		int totalScore = 100;
		if (Timer.timeRemaining <= 0f) {
			totalScore = 50;
		}
		totalScore = totalScore - 10 * numberOfWrongDecisions;
		if(totalScore <= 0) {
			Time.timeScale = 0f;
			string scoreText = "Unfortunately you couldn't get any score points in terms of the number of correct decisions and time constraints.";
			scorePanel.SetActive(true);
			score.SetText(scoreText);
			return false;
		}
		return true;
	}
}
