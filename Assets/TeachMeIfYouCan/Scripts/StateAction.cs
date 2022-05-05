using System.ComponentModel;

/* Possible actions */
public enum StateAction {
	[Description("Drop")]
	Drop,
	[Description("Don")]
	Don,
	[Description("Replace")]
	Replace,
	[Description("Idle")]
	Idle
}
