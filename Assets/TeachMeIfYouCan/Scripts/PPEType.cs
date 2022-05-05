using System;
using System.ComponentModel;

/* All possible states */
[Serializable]
public enum PPEType {
	[Description("Clean Mask")]
	CleanMask,
	[Description("Clean Hat")]
	CleanHat,
	[Description("Clean Face Shield")]
	CleanFaceshield,
	[Description("Clean Left Shoe Cover")]
	CleanLeftShoeCover,
	[Description("Clean Right Shoe Cover")]
	CleanRightShoeCover,
	[Description("Clean Gown")]
	CleanGown,
	[Description("Clean Left Glove")]
	CleanLeftGlove,
	[Description("Clean Right Glove")]
	CleanRightGlove,
	[Description("Dirty Mask")]
	DirtyMask,
	[Description("Dirty Hat")]
	DirtyHat,
	[Description("Dirty Face Shield")]
	DirtyFaceshield,
	[Description("Dirty Left Shoe Cover")]
	DirtyLeftShoeCover,
	[Description("Dirty Right Shoe Cover")]
	DirtyRightShoeCover,
	[Description("Dirty Gown")]
	DirtyGown,
	[Description("Dirty Left Glove")]
	DirtyLeftGlove,
	[Description("Dirty Right Glove")]
	DirtyRightGlove,
	[Description("Terminate")]
	Terminate
}