using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* Static tables to be used for each scene */
public class Statics : MonoBehaviour {
	public static StateAction[,] stateActions = new StateAction[16, 16];
	public static int[,] rewards = new int[16, 16];
	public static double[,] qvalues = new double[16, 16];
	public static List<PPEType> allPossibleStates = ((PPEType[])Enum.GetValues(typeof(PPEType))).ToList();
	public static List<PPEType> currentStates = allPossibleStates.GetRange(0, 8);
	public static List<List<int>> desiredOrders = new List<List<int>> {
		new List<int>(){ 0, 1, 2, 3, 4, 5, 6, 7 },
		new List<int>(){ 0, 1, 2, 4, 3, 5, 6, 7 },
		new List<int>(){ 0, 1, 2, 3, 4, 5, 7, 6 },
		new List<int>(){ 0, 1, 2, 4, 3, 5, 7, 6 }
	};
	public static int count = 0;

	public MyClass[] output;

	public List<PPEType> inspector;

	void Start() {
		allPossibleStates.Remove(PPEType.Terminate);
	}
	void Update() {
		inspector = currentStates;
		output = new MyClass[16];
		for (int i = 0; i < 16; i++) {
			MyClass my = new MyClass();
			my.row = qvalues.GetRow(i);
			output[i] = my;
		}
	}

	[Serializable]
	public class MyClass {
		public List<double> row;
	}

	public static void reset() {
		currentStates = allPossibleStates.GetRange(0, 8);
		count++;
	}
}
