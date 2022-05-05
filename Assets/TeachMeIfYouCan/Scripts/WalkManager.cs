using RootMotion.FinalIK;
using System.Collections.Generic;
using UnityEngine;

/* Defines walking animations for walkinng towards PPEs or replacement cabinet */
public class WalkManager : MonoBehaviour {
	public List<PPE> ppe;
	public FullBodyBipedEffector effector;
	public Transform replacement;
	public ProgramManager programManager;

	[HideInInspector]
	public StateAction action;
	[HideInInspector]
	public bool shouldWalk;

	public InteractionSystem interactionSystem;
	public Animator animator;
	public Transform avatar;

	private PPE currentPPE;

	private List<Vector3> originalPositions = new List<Vector3>();
	private List<Quaternion> originalRotations = new List<Quaternion>();

	// Start is called before the first frame update
	void Start() {
		shouldWalk = false;
		action = StateAction.Idle;
		foreach (PPE obj in ppe) {
			originalPositions.Add(obj.transform.position);
			originalRotations.Add(obj.transform.rotation);
		}
	}

	// Update is called once per frame
	void Update() {
		if (action == StateAction.Don) {
			WalkTowardsPPE(currentPPE.donAction);
		}
		if (action == StateAction.Drop) {
			WalkTowardsPPE(currentPPE.dropAction);
		}
		if (action == StateAction.Replace && shouldWalk) {
			WalkTowardsReplacement();
		}
		if (action == StateAction.Replace && !shouldWalk) {
			WalkTowardsPPE(currentPPE.replaceAction);
		}
	}

	private void startInteraction(InteractionObject interaction) {
		interactionSystem.StartInteraction(effector, interaction, true);
		action = StateAction.Idle;
	}

	private void WalkTowardsPPE(InteractionObject action) {
		if (action == currentPPE.donAction && currentPPE.ppeType.ToDescriptionString().Contains("Dirty")) {
			Vector3 targetDir = new Vector3(currentPPE.transform.position.x - avatar.position.x, 0f, currentPPE.transform.position.z - avatar.position.z);
			Quaternion rotation = Quaternion.LookRotation(targetDir);
			avatar.rotation = rotation;
			startInteraction(action);
		} else {
			animator.SetBool("isWalking", true);
			animator.SetFloat("speed", 1f);
			Vector3 targetDir = new Vector3(currentPPE.transform.position.x - avatar.position.x, 0f, currentPPE.transform.position.z - avatar.position.z);
			Quaternion rotation = Quaternion.LookRotation(targetDir);
			avatar.rotation = Quaternion.Slerp(avatar.rotation, rotation, Time.deltaTime * 3);
			Vector3 avatarPos = new Vector3(avatar.position.x, 0, avatar.position.z);
			Vector3 ppePos = new Vector3(currentPPE.transform.position.x, 0, currentPPE.transform.position.z);
			if (Vector3.Distance(avatarPos, ppePos) < 1.1f) {
				avatar.rotation = rotation;
				startInteraction(action);
			}
		}
	}

	public void WalkTowardsReplacement() {
		animator.SetBool("isWalking", true);
		animator.SetFloat("speed", 1f);
		Vector3 targetDir = new Vector3(replacement.position.x - avatar.position.x, 0f, replacement.position.z - avatar.position.z);
		Quaternion rotation = Quaternion.LookRotation(targetDir);
		avatar.rotation = Quaternion.Slerp(avatar.rotation, rotation, 0.05f);
		Vector3 avatarPos = new Vector3(avatar.position.x, 0, avatar.position.z);
		Vector3 replacementPos = new Vector3(replacement.transform.position.x, 0, replacement.transform.position.z);
		if (Vector3.Distance(avatarPos, replacementPos) < 1.1f) {
			avatar.rotation = rotation;
			currentPPE.transform.SetParent(null, true);
			currentPPE.transform.position = originalPositions[ppe.IndexOf(currentPPE)];
			currentPPE.transform.rotation = originalRotations[ppe.IndexOf(currentPPE)];
			int index = Statics.allPossibleStates.IndexOf(currentPPE.ppeType);
			if (index > 8) {
				Statics.currentStates.Remove(currentPPE.ppeType);
				currentPPE.ppeType = Statics.allPossibleStates[index - 8];
				Statics.currentStates.Add(currentPPE.ppeType);
			}
			action = StateAction.Idle;
			shouldWalk = false;
			Invoke(nameof(NextDelayed), 0.5f);
		}
	}

	public void StartTransition(Transition transition) {
		Debug.Log(transition.ToString());
		currentPPE = ppe.Find((x) => x.ppeType == transition.from);
		action = transition.action;
	}

	private void NextDelayed() {
		programManager.Next();
	}
}
