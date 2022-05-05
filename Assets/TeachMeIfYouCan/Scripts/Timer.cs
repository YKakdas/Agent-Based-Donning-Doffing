using TMPro;
using UnityEngine;

/* Responsible for showing elapsed and remaining time in interactive mode */
public class Timer : MonoBehaviour {
	public static float timeRemaining = 600;
	public bool timerIsRunning = false;
	public TextMeshProUGUI timeText;

	private void Start() {
		timerIsRunning = true;
	}

	void Update() {
		if (timerIsRunning) {
			if (timeRemaining > 0) {
				timeRemaining -= Time.unscaledDeltaTime;
				DisplayTime(timeRemaining);
			} else {
				Debug.Log("Time has run out!");
				timeRemaining = 0;
				timerIsRunning = false;
			}
		}
	}

	void DisplayTime(float timeToDisplay) {
		timeToDisplay += 1;

		float remainingMinutes = Mathf.FloorToInt(timeToDisplay / 60);
		float remainingSeconds = Mathf.FloorToInt(timeToDisplay % 60);

		float elapsedMinutes = Mathf.FloorToInt((600 - timeToDisplay) / 60);
		float elapsedSeconds = Mathf.FloorToInt((600 - timeToDisplay) % 60);

		timeText.text = "Elapsed Time: " + string.Format("{0:00}:{1:00}", elapsedMinutes, elapsedSeconds) + "\n" + "Remaining Time: " + string.Format("{0:00}:{1:00}", remainingMinutes, remainingSeconds) + "\n";
	}
}