/* Data class for defining transition from a state to another state with given action */
public class Transition {
	public PPEType from { get; private set; }
	public StateAction action { get; private set; }
	public PPEType to { get; set; }

	public Transition(PPEType from, StateAction action, PPEType to) {
		this.from = from;
		this.action = action;
		this.to = to;
	}

	public string ToString() {
		return from + "  " + action + "  " + to;
	}
}
