using RootMotion.FinalIK;
using System.Collections;
using UnityEngine;

/* Manager class for Inverse Kinematics interactions. FinalIK was used */
public class IKManager : MonoBehaviour {
	public GameObject avatar;
	public ProgramManager programManager;
	public InteractionObject dropAction;
	public InteractionObject donAction;
	public InteractionObject replaceAction;
	public WalkManager walkManager;

	public OffsetPose dropOffset;
	public OffsetPose donOffset;
	private OffsetPose replaceOffset;

	public GameObject skinnedMesh;

	private FullBodyBipedIK ik;
	private Rigidbody rb;

	private float dropWeight;
	private float donWeight;
	private float replaceWeight;

	private bool isDrop;
	private bool isDon;
	private bool isReplace;

	private PPE ppe;
	private Animator animator;

	// Start is called before the first frame update
	void Start() {
		rb = GetComponent<Rigidbody>();
		animator = avatar.GetComponent<Animator>();
		ppe = GetComponent<PPE>();
		Physics.IgnoreLayerCollision(20, 20);
		Physics.IgnoreLayerCollision(8, 20);
		replaceOffset = dropOffset;
	}

	void LateUpdate() {
		if (ik == null) return;
		if (isDrop) {
			dropOffset.Apply(ik.solver, dropWeight, ik.transform.rotation);
		} else if (isDon) {
			donOffset.Apply(ik.solver, donWeight, ik.transform.rotation);
		} else if (isReplace) {
			replaceOffset.Apply(ik.solver, replaceWeight, ik.transform.rotation);
		}
	}

	/* If the current action is Drop, first walk little bit backward, after avatar finds proper place, call real dropping animation. */
	public IEnumerator OnDrop() {
		GetComponent<BoxCollider>().enabled = false;
		animator.SetBool("isWalking", true);
		animator.SetFloat("speed", -1f);
		isDrop = true;
		ik = dropAction.lastUsedInteractionSystem.GetComponent<FullBodyBipedIK>();
		rb.isKinematic = true;
		while (dropWeight < 1f) {
			if (dropWeight > 0.65) {
				animator.SetBool("isWalking", false);
			}
			dropWeight += Time.deltaTime;
			yield return null;
		}
		Drop();
	}

	/* If the current action is Don, first walk little bit backward, after avatar finds proper place, call real donning animation. */
	public IEnumerator OnDon() {
		GetComponent<BoxCollider>().enabled = false;
		if (ppe.ppeType.ToDescriptionString().Contains("Clean")) {
			animator.SetBool("isWalking", true);
			animator.SetFloat("speed", -1f);
		} else {
			animator.SetBool("isWalking", false);
			animator.SetFloat("speed", 1f);
		}

		isDon = true;
		ik = donAction.lastUsedInteractionSystem.GetComponent<FullBodyBipedIK>();
		rb.isKinematic = true;
		while (donWeight < 1f) {
			if (donWeight > 0.65) {
				animator.SetBool("isWalking", false);
			}
			donWeight += Time.deltaTime;
			yield return null;
		}
		Don();
	}

	/* If the current action is Replace, first walk little bit backward, after avatar finds proper place, call real replacing animation. */
	IEnumerator OnReplace() {
		GetComponent<BoxCollider>().enabled = false;
		animator.SetFloat("speed", -1f);
		isReplace = true;
		ik = replaceAction.lastUsedInteractionSystem.GetComponent<FullBodyBipedIK>();
		rb.isKinematic = true;
		while (replaceWeight < 1f) {
			if (replaceWeight > 0.65) {
				animator.SetBool("isWalking", false);
			}
			replaceWeight += Time.deltaTime;
			yield return null;
		}
		Replace();
	}

	/* Action was Don, enable skinned mesh on avatar, remove scene PPE from environment */
	private void Don() {
		isDon = false;
		ik = null;
		donWeight = 0;
		skinnedMesh.SetActive(true);
		gameObject.SetActive(false);
		Statics.currentStates.Remove(ppe.ppeType);
		GetComponent<BoxCollider>().enabled = true;
		walkManager.action = StateAction.Idle;
		Invoke(nameof(NextDelayed), 0.5f);
	}

	/* Action was drop, update the environment, replace clean PPE with the corresponding dirty one*/
	void Drop() {
		isDrop = false;
		ik = null;
		dropWeight = 0;
		transform.SetParent(null, true);
		rb.isKinematic = false;
		int index = Statics.allPossibleStates.IndexOf(ppe.ppeType);
		if (index < 8) {
			Statics.currentStates.Remove(ppe.ppeType);
			ppe.ppeType = Statics.allPossibleStates[index + 8];
			Statics.currentStates.Add(ppe.ppeType);
		}
		GetComponent<BoxCollider>().enabled = true;
		walkManager.action = StateAction.Idle;
		Invoke(nameof(NextDelayed), 0.5f);
	}

	/* Action was replace, start animation for walking towards the replacement cabinet*/
	private void Replace() {
		isReplace = false;
		ik = null;
		replaceWeight = 0;
		GetComponent<BoxCollider>().enabled = true;
		walkManager.shouldWalk = true;
		walkManager.action = StateAction.Replace;
	}

	private void NextDelayed() {
		programManager.Next();
	}
}
