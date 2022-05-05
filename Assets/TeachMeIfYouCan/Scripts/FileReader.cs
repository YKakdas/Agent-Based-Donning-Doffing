using System;
using UnityEngine;

public class FileReader : MonoBehaviour {
	private static int count = 0;
	public bool showCorrectSequence = false;

	/* Read files only if it is first run of the program */
	void Start() {
		if (count == 0) {
			ReadStateActionTable();
			ReadRewardsTable();
			ReadQValueTable();
		}
		count++;
	}

	/* Reads MDP structure from csv and maps into Enums */
	private void ReadStateActionTable() {
		string[] lines = System.IO.File.ReadAllText("Assets/TeachMeIfYouCan/InputFiles/StateActionTable.csv").Split('\n');

		StateAction[,] stateActions = new StateAction[16, 16];
		for (int i = 0; i < lines.Length; i++) {
			string[] actions = lines[i].Split(',');
			for (int j = 0; j < actions.Length; j++) {
				if (Int64.Parse(actions[j]) == 0) {
					stateActions[i, j] = StateAction.Don;
				} else if (Int64.Parse(actions[j]) == 1) {
					stateActions[i, j] = StateAction.Drop;
				} else if (Int64.Parse(actions[j]) == 2) {
					stateActions[i, j] = StateAction.Replace;
				}
			}
		}
		Statics.stateActions = stateActions;
	}

	/* Reads predefined rewards for the training mode, and validation of interactive mode */
	private void ReadRewardsTable() {
		string[] lines = System.IO.File.ReadAllText("Assets/TeachMeIfYouCan/InputFiles/Rewards.csv").Split('\n');
		int[,] rewards = new int[16, 16];
		for (int i = 0; i < lines.Length; i++) {
			string[] rewardRow = lines[i].Split(',');
			for (int j = 0; j < rewardRow.Length; j++) {
				rewards[i, j] = Int32.Parse(rewardRow[j]);
			}
		}
		Statics.rewards = rewards;
	}

	/**
	 * Possible 3 different reading file operations
	 * 
	 * 1. For showing the correct sequence of the donning(CorrectOrder.csv)
	 * 
	 * 2. For training mode, reads Training.csv
	 * 
	 * 3. For interactive mode, reads Interactive.csv
	 */
	private void ReadQValueTable() {
		string path = showCorrectSequence == false ? "Assets/TeachMeIfYouCan/InputFiles/Training.csv" : "Assets/TeachMeIfYouCan/InputFiles/CorrectOrder.csv";
		if (ProgramManager.enableInteractions) {
			path = "Assets/TeachMeIfYouCan/InputFiles/Interactive.csv";
		}
		string[] lines = System.IO.File.ReadAllText(path).Split('\n');
		double[,] qvalues = new double[16, 16];
		for (int i = 0; i < lines.Length; i++) {
			string[] qvalueRow = lines[i].Split(',');
			for (int j = 0; j < qvalueRow.Length; j++) {
				if (ProgramManager.enableInteractions || showCorrectSequence) {
					qvalues[i, j] = Double.Parse(qvalueRow[j]);
				} else {
					qvalues[i, j] = 0;
				}
			}
		}
		Statics.qvalues = qvalues;
	}

	/**
	 * Reads prefilled Q-Value table for showing the correct sequence of the donning
	 */
	public static void ReadCorrectOrder() {
		string path = "Assets/TeachMeIfYouCan/InputFiles/CorrectOrder.csv";
		string[] lines = System.IO.File.ReadAllText(path).Split('\n');
		double[,] qvalues = new double[16, 16];
		for (int i = 0; i < lines.Length; i++) {
			string[] qvalueRow = lines[i].Split(',');
			for (int j = 0; j < qvalueRow.Length; j++) {
				qvalues[i, j] = Double.Parse(qvalueRow[j]);
			}
		}
		Statics.qvalues = qvalues;
	}

}

