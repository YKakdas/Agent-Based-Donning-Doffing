using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

/* Helper class for making List and Enum operations easier */

public static class Extensions {

	/* Returns all indexes of given val in the array */
	public static int[] FindAllIndexof<T>(this IEnumerable<T> values, T val) {
		return values.Select((b, i) => Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
	}

	/* Returns row of given matrix */
	public static List<T> GetRow<T>(this T[,] val, int rowNumber) {
		return Enumerable.Range(0, val.GetLength(1))
			.Select(x => val[rowNumber, x])
			.ToList();
	}

	/* Returns a random element from given list */
	public static T GetRandomElement<T>(this List<T> values) {
		return values[new System.Random().Next(0, values.Count)];
	}

	/* Returns description string of given enum */
	public static string ToDescriptionString(this PPEType val) {
		DescriptionAttribute[] attributes = (DescriptionAttribute[])val
		   .GetType()
		   .GetField(val.ToString())
		   .GetCustomAttributes(typeof(DescriptionAttribute), false);
		return attributes.Length > 0 ? attributes[0].Description : string.Empty;
	}

	public static string ToDescriptionString(this StateAction val) {
		DescriptionAttribute[] attributes = (DescriptionAttribute[])val
		   .GetType()
		   .GetField(val.ToString())
		   .GetCustomAttributes(typeof(DescriptionAttribute), false);
		return attributes.Length > 0 ? attributes[0].Description : string.Empty;
	}

}
