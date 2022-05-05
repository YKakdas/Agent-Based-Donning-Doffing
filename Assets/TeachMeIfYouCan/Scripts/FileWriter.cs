using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class FileWriter : MonoBehaviour {
	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	/* Writes updated QValues after each iteration for statistical purposes */
	public void WriteQValueIntoFile(int iteration) {
		var filePath = "Assets/TeachMeIfYouCan/OutputFiles/" + iteration + ".csv";
		var csv = new StringBuilder();

		for (int i = 0; i < 16; i++) {
			List<double> row = Statics.qvalues.GetRow(i);
			csv.AppendLine(string.Join(",", row));
		}

		File.WriteAllText(filePath, csv.ToString());
	}
}
