using System.Collections.Generic;
using System.Linq;

/* Helper class for reading operations on qvalue table */
public class QvalueUtil {
	public int[] FindMaxsInRow(int row) {
		double max = Statics.qvalues[row, 0];
		for (int i = 0; i < 16; i++) {
			if (max < Statics.qvalues[row, i]) {
				max = Statics.qvalues[row, i];
			}
		}
		return Statics.qvalues.GetRow(row).FindAllIndexof(max);
	}

	public int FindSecondMaxsInRow(int row) {
		List<double> qvalueRow = Statics.qvalues.GetRow(row).ToList();
		int maxIndex = 0;
		double max = qvalueRow[0];
		for (int i = 0; i < 16; i++) {
			if (max < Statics.qvalues[row, i]) {
				max = Statics.qvalues[row, i];
				maxIndex = i;
			}
		}
		qvalueRow.RemoveAt(maxIndex);

		double secondMax = qvalueRow[0];
		int secondMaxIndex = 0;
		for (int i = 0; i < 15; i++) {
			if (secondMax < qvalueRow[i]) {
				secondMax = qvalueRow[i];
			}
		}
		return Statics.qvalues.GetRow(row).ToList().IndexOf(secondMax);
	}
}
