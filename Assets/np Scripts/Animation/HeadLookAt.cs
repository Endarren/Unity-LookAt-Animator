using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadLookAt : MonoBehaviour
{
	#region Public Fields

	public MoveType moveType;

	public Animator animator;
	public Vector3 lookOffset;
	public float lookAtSpeed = 1f;
	public Transform lookAtTransform;
	public List<Transform> nextLookAtTransform = new List<Transform>();
	public float nextLookAtSpeed = 10f;
	public float smoothTime = 0.3f;
	bool isChangingLook = false;
	#endregion Public Fields

	#region Protected Fields

	protected float weight;
	protected Vector3 lookAtPosition;

	#endregion Protected Fields

	#region Public Enums

	public enum MoveType { MoveTowards, SmoothDamp }

	#endregion Public Enums

	#region Public Methods

	public void StartLookingAt(Transform t)
	{
		if (lookAtTransform == null)
		{
			lookAtTransform = t;
			lookAtPosition = lookAtTransform.position;
			StopAllCoroutines();
			StartCoroutine(LookAtRoutine());
		}
		else
		{
			if (!nextLookAtTransform.Contains(t) && t != lookAtTransform)
			{
				nextLookAtTransform.Add(t);
				StopAllCoroutines();
				StartCoroutine(ChangeLookAt());
			}
		}
	}
	private void Update()
	{
		if(lookAtTransform != null && !isChangingLook)
			lookAtPosition = lookAtTransform.position + lookOffset;
	}
	public void StopLookingAt()
	{
		
		StopAllCoroutines();
		nextLookAtTransform.Clear();
		StartCoroutine(StopLookAtRoutine());
	}

	#endregion Public Methods

	#region Private Methods

	private IEnumerator LookAtRoutine()
	{
		float currentWeight = 0;
		weight = 0f;
		float velocity = 0;
		while (currentWeight <= 1f)
		{
			//currentWeight = Mathf.MoveTowards(currentWeight, 1f, lookAtSpeed * Time.deltaTime);
			switch (moveType)
			{
				case MoveType.MoveTowards:
					currentWeight = Mathf.MoveTowards(currentWeight, 1f, lookAtSpeed * Time.deltaTime);
					break;

				case MoveType.SmoothDamp:
					currentWeight = Mathf.SmoothDamp(currentWeight, 1f, ref velocity, smoothTime);
					break;
			}

			weight = currentWeight;
			yield return null;
		}
	}

	private IEnumerator ChangeLookAt()
	{
		float distance = Vector3.Distance(lookAtTransform.position, nextLookAtTransform [0].position + lookOffset);
		Vector3 velocity = Vector3.zero;
		isChangingLook = true;
		while (nextLookAtTransform.Count > 0)
		{
			//lookAtPosition = lookAtTransform.position;
			switch (moveType)
			{
				case MoveType.MoveTowards:
					lookAtPosition = Vector3.MoveTowards(lookAtPosition, nextLookAtTransform [0].position + lookOffset, nextLookAtSpeed * Time.deltaTime);
					break;

				case MoveType.SmoothDamp:
					lookAtPosition = Vector3.SmoothDamp(lookAtPosition, nextLookAtTransform [0].position + lookOffset, ref velocity, smoothTime);
					break;
			}
			distance = Vector3.Distance(lookAtPosition, nextLookAtTransform [0].position + lookOffset);
			if (distance < 0.1f)
			{
				lookAtTransform = nextLookAtTransform [0];
				lookAtPosition = lookAtTransform.position + lookOffset;
				nextLookAtTransform.RemoveAt(0);
			}
			yield return null;
		}
		float velo = 0;
		while(weight < 1f)
		{
			switch (moveType)
			{
				case MoveType.MoveTowards:
					weight = Mathf.MoveTowards(weight, 1f, lookAtSpeed * Time.deltaTime);
					break;

				case MoveType.SmoothDamp:
					weight = Mathf.SmoothDamp(weight, 1f, ref velo, smoothTime);
					break;
			}
			yield return null;
		}
		isChangingLook = false;
	}

	private IEnumerator StopLookAtRoutine()
	{
		isChangingLook = false;
		lookAtTransform = null;
		float currentWeight = weight;
		float velocity = 0;
		while (currentWeight > 0)
		{
			switch (moveType)
			{
				case MoveType.MoveTowards:
					currentWeight = Mathf.MoveTowards(currentWeight, 0f, lookAtSpeed * Time.deltaTime);
					break;

				case MoveType.SmoothDamp:
					currentWeight = Mathf.SmoothDamp(currentWeight, 0f, ref velocity, smoothTime);
					break;
			}

			weight = currentWeight;
			yield return null;
		}
	}

	private void OnAnimatorIK()
	{
		Vector3 target = lookAtPosition + lookOffset;
		animator.SetLookAtPosition(target);
		animator.SetLookAtWeight(weight);
	}
	private void OnDrawGizmosSelected()
	{
		if (animator != null)
		{
			Debug.DrawLine(animator.GetBoneTransform(HumanBodyBones.Head).position, lookAtPosition);
		}
		//Gizmos.DrawSphere(lookAtPosition, 1f);
	}
	#endregion Private Methods
}