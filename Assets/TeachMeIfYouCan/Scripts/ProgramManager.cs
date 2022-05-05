using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * Main manager that handles all the flow of the simulation
 * 
 * Some outline cases: 
 * 
 * Depends on the previous transitions, some transitions may be correct or wrong. For instance, the correct order of donning shoecovers and gown might be different.
 * If both shoecovers have not been donned yet, it is correct to follow either left -> right -> gown or right -> left -> gown. In that case, both left -> gown, and right -> gown
 * should be rewarded positive. However, if the agent donned the gown after one of the shoe covers without donning the other shoecover, it should be rewarded negative. That is the
 * only outline case that needs to be controlled manually.
 * 
 */
public class ProgramManager : MonoBehaviour {

	public WalkManager manager;
	public FileWriter writer;
	public UserInteractionManager interactionManager;
	public ScoreManager scoreManager;
	public GameObject timer;
	public GameObject red;
	public GameObject green;
	public GameObject infoPanel;
	public GameObject userPromptPanel;
	public TextMeshProUGUI descriptionText;
	public FileReader fileReader;

	private QvalueUtil util;
	private int currentRow = 0;
	private int rowCount = 0;
	private Transition currentTransition = null;
	private Transition previousTransition = null;
	private Transition lastFeedbackTransition = null;
	private PPEType lastDonnedPPE = PPEType.Terminate;
	private int count = 0;
	private static bool showFeedback = true;
	private static bool lastRound = false;
	private static bool lastRoundLoaded = false;
	private int feedbackReward = 0;
	private bool userInteractionRetrieved = false;
	private int previousRow = -1;

	[HideInInspector]
	public static bool enableInteractions;
	// Start is called before the first frame update
	void Start() {
		if (!enableInteractions) {
			timer.SetActive(false);
		}
		util = new QvalueUtil();
		Next();
	}

	/* Find random transition from the environment. Loop through the environment randomly until find an available transition */
	private Transition GetTransition(int row) {
		if (previousRow == -1) {
			previousRow = row;
		}
		List<int> possibleStates = util.FindMaxsInRow(row).ToList();
		while (possibleStates.Count > 0) {
			int stateNum = 0;
			if ((row == 2 || row == 5) && possibleStates.Contains(row + 1)) {
				stateNum = row + 1;
			} else {
				stateNum = possibleStates.GetRandomElement();
			}
			int index = possibleStates.IndexOf(stateNum);
			PPEType from = Statics.allPossibleStates[row];
			PPEType to = Statics.allPossibleStates[stateNum];
			StateAction action = Statics.stateActions[row, stateNum];
			Transition transition = new Transition(from, action, to);

			possibleStates.RemoveAt(index);

			if (CheckTransitionAvailability(transition)) {
				previousRow = row;
				return transition;
			} else {
				if (previousRow == stateNum) {
					int secondMax = util.FindSecondMaxsInRow(row);
					Debug.Log(secondMax);
					from = Statics.allPossibleStates[row];
					to = Statics.allPossibleStates[secondMax];
					action = Statics.stateActions[row, secondMax];
					transition = new Transition(from, action, to);
					if (CheckTransitionAvailability(transition)) {
						return transition;
					}
					previousRow = row;
				}
			}
		}

		if (Statics.currentStates.Count > 0) {
			Transition transition;
			PPEType from = Statics.allPossibleStates[row];
			do {
				PPEType to;
				if (Statics.currentStates.Count == 3 && Statics.allPossibleStates.IndexOf(Statics.currentStates[0]) == 5 && row == 4) {
					to = PPEType.CleanGown;
				} else {
					to = Statics.currentStates.GetRandomElement();
				}
				StateAction action = Statics.stateActions[row, Statics.allPossibleStates.IndexOf(to)];
				transition = new Transition(from, action, to);
			} while (!CheckTransitionAvailability(transition));
			previousRow = row;
			return transition;
		}
		previousRow = row;
		return null;
	}

	/* Checks whether randomly found transition performable in the current state of the environment */
	private bool CheckTransitionAvailability(Transition transition) {
		if (!Statics.currentStates.Contains(transition.from)) {
			return false;
		}
		if (transition.action == StateAction.Don) {
			if (Statics.currentStates.Count == 1) {
				transition.to = PPEType.Terminate;
				return true;
			}
			if (Statics.currentStates.Contains(transition.to)) {
				return true;
			} else {
				return false;
			}
		} else if (transition.action == StateAction.Drop) {
			int fromIndex = Statics.allPossibleStates.IndexOf(transition.from);
			int toIndex = Statics.allPossibleStates.IndexOf(transition.to);
			if (toIndex == fromIndex + 8)
				return true;
			return false;
		} else if (transition.action == StateAction.Replace) {
			int fromIndex = Statics.allPossibleStates.IndexOf(transition.from);
			int toIndex = Statics.allPossibleStates.IndexOf(transition.to);
			if (toIndex == fromIndex - 8)
				return true;
			return false;
		} else {
			return false;
		}
	}

	public void Next() {
		StartCoroutine(NextIteration());
	}

	public IEnumerator NextIteration() {

		/* If it is tutorial mode and the last round, show correct sequence one more time for training purposes */
		if (lastRound && !enableInteractions && !fileReader.showCorrectSequence) {
			yield return SetupLastRound();
		}
		if (showFeedback && count > 0 && previousTransition != null && previousTransition.to != PPEType.Terminate) {
			if (count > 1 && count < 4 && lastDonnedPPE == PPEType.Terminate) {
				lastDonnedPPE = PPEType.CleanMask;
			}
			if (count == 1 && previousTransition.action == StateAction.Don) { // First action is donning the mask
				if (enableInteractions) {
					yield return PromptUserInteraction("The agent donned the face mask first. Please give your feedback...");
					if (feedbackReward < 0) {
						ShowFeedback(false);
						ScoreManager.numberOfWrongDecisions++;
					}
				} else {
					ShowFeedback(true);
				}
				lastDonnedPPE = previousTransition.from;
			} else if (previousTransition.action == StateAction.Don && Statics.allPossibleStates.IndexOf(previousTransition.from) >= 8) { // Donning dirty ppe
				yield return ProcessAction(previousTransition, "The agent donned the " + previousTransition.from.ToDescriptionString() + ". Please give your feedback...");
				int previousFrom = Statics.allPossibleStates.IndexOf(previousTransition.from);
				lastDonnedPPE = Statics.allPossibleStates[previousFrom % 8];
			} else if (previousTransition.action == StateAction.Drop) { // Dropping the ppe
				yield return ProcessAction(previousTransition, "The agent dropped the " + previousTransition.from.ToDescriptionString() + ". Please give your feedback...");
			} else if (previousTransition.action == StateAction.Replace) { // Replacing the ppe
				yield return ProcessAction(previousTransition, "The agent replaced the " + previousTransition.from.ToDescriptionString() + " with new " + previousTransition.to.ToDescriptionString() + ". Please give your feedback...");
			} else if (lastDonnedPPE != PPEType.Terminate && previousTransition.action == StateAction.Don) { // Donning clean PPE
				int lastDonnedIndex = Statics.allPossibleStates.IndexOf(lastDonnedPPE) % 8;
				int newIndex = Statics.allPossibleStates.IndexOf(previousTransition.from) % 8;
				CheckOutlineCases(lastDonnedIndex, newIndex);

				if (!enableInteractions) {
					Statics.qvalues[lastDonnedIndex, newIndex] += Statics.rewards[lastDonnedIndex, newIndex];
					ShowFeedback(CheckDonningOrder(lastDonnedIndex, newIndex));
				} else {
					yield return PromptUserInteraction("The agent donned the " + previousTransition.from.ToDescriptionString() + " after the "
						+ Statics.allPossibleStates[lastDonnedIndex].ToDescriptionString().Substring(6) + ". Please give your feedback...");
					bool donningOrder = CheckDonningOrder(lastDonnedIndex, newIndex);
					if (donningOrder ^ (feedbackReward > 0)) {
						ShowFeedback(false);
						ScoreManager.numberOfWrongDecisions++;
					}

					double discountFactor = 0.0;
					double futureReward = util.FindMaxsInRow(newIndex % 15).ToList().Max();

					Statics.qvalues[lastDonnedIndex, newIndex] += feedbackReward + discountFactor * futureReward;
					feedbackReward = 0;
				}
				lastDonnedPPE = previousTransition.from;
			}
		}

		Transition transition = GetTransition(currentRow);
		if (transition != null) {
			if (transition.to != PPEType.Terminate) {
				int toIndex = Statics.allPossibleStates.IndexOf(transition.to);
				currentRow = toIndex;
				previousTransition = transition;
			} else {
				lastFeedbackTransition = previousTransition;
				previousTransition = null;
			}
			manager.StartTransition(transition);
			count++;

		} else {
			previousTransition = null;
			if (!enableInteractions) {
				ShowFeedback(CheckDonningOrder(Statics.allPossibleStates.IndexOf(lastFeedbackTransition.from) % 8, Statics.allPossibleStates.IndexOf(lastFeedbackTransition.to) % 8), 0.5f, true);
				UpdateQValue(lastFeedbackTransition);
			} else {
				yield return PromptUserInteraction("The agent donned the " + lastFeedbackTransition.to.ToDescriptionString() + " after the " + lastFeedbackTransition.from.ToDescriptionString().Substring(6) + ". Please give your feedback...");
				bool donningOrder = CheckDonningOrder(Statics.allPossibleStates.IndexOf(lastFeedbackTransition.from) % 8, Statics.allPossibleStates.IndexOf(lastFeedbackTransition.to));
				if (!((donningOrder && feedbackReward > 0) || (!donningOrder && feedbackReward < 0))) {
					ScoreManager.numberOfWrongDecisions++;
				}
				UpdateQValue(lastFeedbackTransition, feedbackReward);
				feedbackReward = 0;
			}
			lastFeedbackTransition = null;
			writer.WriteQValueIntoFile(Statics.count);
			if (analyzeQValues()) {
				if (!enableInteractions) {
					if (lastRoundLoaded) {
						UnityEditor.EditorApplication.isPlaying = false;
					}
					lastRound = true;
					showFeedback = false;
					lastRoundLoaded = true;
					ReloadScene();
				} else {
					scoreManager.ShowScore();
				}

				Debug.Log("Iteration count is " + (Statics.count - 1));
				Time.timeScale = 0f;
			} else {
				bool shouldContinue = scoreManager.CheckScore();
				if (enableInteractions && shouldContinue) {
					ReloadScene();
				}
			}
		}
		yield return null;
	}

	private IEnumerator SetupLastRound() {
		infoPanel.SetActive(true);
		FileReader.ReadCorrectOrder();
		yield return new WaitForSecondsRealtime(3f);
		red.SetActive(false);
		green.SetActive(false);
		infoPanel.SetActive(false);
		lastRound = false;
		Time.timeScale = 1f;
	}

	private IEnumerator ProcessAction(Transition previousTransition, string text) {
		if (!enableInteractions) {
			ShowFeedback(false);
			UpdateQValue(previousTransition);
		} else {
			yield return PromptUserInteraction(text);
			OnUserInteractionRetrieved(previousTransition);
		}
	}

	private IEnumerator PromptUserInteraction(string text) {
		userPromptPanel.SetActive(true);
		descriptionText.text = text;
		Time.timeScale = 0f;
		yield return new WaitUntil(() => userInteractionRetrieved);
		Time.timeScale = 1f;
		userInteractionRetrieved = false;
		userPromptPanel.SetActive(false);
	}

	private void OnUserInteractionRetrieved(Transition previousTransition) {
		userInteractionRetrieved = false;
		userPromptPanel.SetActive(false);
		EvaluateUserInteraction(previousTransition);
		UpdateQValue(previousTransition, feedbackReward);
		feedbackReward = 0;
	}

	private void EvaluateUserInteraction(Transition previousTransition) {
		int expectedReward = getExpectedReward(previousTransition);
		if (feedbackReward * expectedReward < 0) {
			ShowFeedback(false);
			ScoreManager.numberOfWrongDecisions++;
		}
	}

	private void UpdateQValue(Transition previousTransition, int reward = 0) {
		int previousFrom = Statics.allPossibleStates.IndexOf(previousTransition.from);
		int previousTo = Statics.allPossibleStates.IndexOf(previousTransition.to);
		double discountFactor = 0.0;
		if (!enableInteractions) {
			double futureReward = util.FindMaxsInRow(previousTo % 15).ToList().Max();
			Statics.qvalues[previousFrom, previousTo] += Statics.rewards[previousFrom, previousTo] + futureReward * discountFactor;
		} else {
			Statics.qvalues[previousFrom, previousTo] += reward + feedbackReward * discountFactor;
		}
	}

	private int getExpectedReward(Transition previousTransition) {
		int previousFrom = Statics.allPossibleStates.IndexOf(previousTransition.from);
		int previousTo = Statics.allPossibleStates.IndexOf(previousTransition.to);
		return Statics.rewards[previousFrom, previousTo];
	}

	/* Check all possible correct endings of the simulation */
	private bool CheckDonningOrder(int lastDonnedIndex, int newIndex) {
		if (lastDonnedIndex + 1 == newIndex) {
			if (lastDonnedIndex == 4 && Statics.currentStates.Contains(PPEType.CleanLeftShoeCover)) {
				return false;
			} else {
				return true;
			}
		} else if ((lastDonnedIndex - 1 == newIndex) && (lastDonnedIndex == 7 || lastDonnedIndex == 4)) {
			return true;
		} else if (lastDonnedIndex + 2 == newIndex) {
			if (lastDonnedIndex == 2 || lastDonnedIndex == 3 || lastDonnedIndex == 5) {
				if (lastDonnedIndex == 3 && Statics.currentStates.Contains(PPEType.CleanRightShoeCover)) {
					return false;
				} else {
					return true;
				}
			}
		}
		return false;
	}

	private void ShowFeedback(bool positive, float time = 0.5f, bool reloadScene = false) {
		Time.timeScale = 0f;
		if (positive == true) {
			green.SetActive(true);
		} else {
			red.SetActive(true);
		}
		StartCoroutine(RunAfterDelay(time, reloadScene));
	}

	IEnumerator RunAfterDelay(float time, bool reloadScene) {
		yield return new WaitForSecondsRealtime(time);
		red.SetActive(false);
		green.SetActive(false);
		infoPanel.SetActive(false);
		lastRound = false;
		Time.timeScale = 1f;
		if (reloadScene) {
			ReloadScene();
		}
	}

	/* Special control function for order of the shoecovers */
	private void CheckOutlineCases(int fromIndex, int toIndex) {
		if (fromIndex == 3 && toIndex == 5) {
			if (Statics.currentStates.Contains(PPEType.CleanRightShoeCover)) {
				Statics.rewards[fromIndex, toIndex] = -100;
			} else {
				Statics.rewards[fromIndex, toIndex] = 150;
			}
		} else if (fromIndex == 4 && toIndex == 5) {
			if (Statics.currentStates.Contains(PPEType.CleanLeftShoeCover)) {
				Statics.rewards[fromIndex, toIndex] = -100;
			} else {
				Statics.rewards[fromIndex, toIndex] = 150;
			}
		}
	}

	private bool analyzeQValues() {
		List<int> temp = new List<int>();
		temp.Add(0);
		int row = 0;
		for (int i = 0; i < 7; i++) {
			List<int> maxNums = util.FindMaxsInRow(row).ToList();
			int num = maxNums.Max();
			temp.Add(num);
			row = num;
		}

		bool isCorrect = false;
		foreach (List<int> desiredOrder in Statics.desiredOrders) {
			if (temp.SequenceEqual(desiredOrder)) {
				isCorrect = true;
				break;
			}
		}
		return isCorrect;
	}

	private void ReloadScene() {
		Statics.reset();
		Scene scene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(scene.name);
	}

	public void OnNegativeButtonClicked() {
		userInteractionRetrieved = true;
		feedbackReward = -100;
	}

	public void OnPositiveButtonClicked() {
		userInteractionRetrieved = true;
		feedbackReward = 100;
	}


}
