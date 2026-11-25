using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Tasks.Components.Containers;

public class HandGestures : MonoBehaviour
{
	// SerializeField allows private variables to be edited in Unity but not by other scripts
    [SerializeField] private UnityEvent onPointing = new UnityEvent();
	[SerializeField] private UnityEvent onPeaceSign = new UnityEvent();
	[SerializeField] private UnityEvent onOpenHand = new UnityEvent();

	private List<NormalizedLandmark> landmarks;
	float cameraAspect = 1f;

	private bool wasPointing = false;
	private bool wasPeaceSign = false;
	private bool wasOpenHand = false;


	// Detect a hand and check for gestures
	public void ProcessHand(HandLandmarkerResult result)
	{
		// Make sure a hand exists
		if (result.handLandmarks == null || result.handLandmarks.Count == 0)
			return;

		// Get the first detected hand's joint positions
		landmarks = result.handLandmarks[0].landmarks;

		// Make sure a 5-fingered hand exists
		if (landmarks == null || landmarks.Count < 21)
			return;

		// Assign the correct camera aspect ratio
		cameraAspect = 1920f/1080f;

		// Check for any gestures
		CheckPointing();
		CheckPeaceSign();
		CheckOpenHand();
	}

	// Find the finger angle by calculating the distance between joints
	private float FingerAngle(int baseIndex, int jointIndex, int tipIndex)
	{
		// Get the point of one joint
		Vector2 GetPoint(int index)
		{
			// Gets the point based on the mapped indices in MediaPipe
			var point = landmarks[index];

			// Return the coords, adjusting for camera aspect ratio
			return new Vector2(point.x * cameraAspect, point.y);
		}

		// Calculate the three points of a finger to get the finger angle
		Vector2 joint = GetPoint(jointIndex);
		return Vector2.Angle(GetPoint(baseIndex) - joint, GetPoint(tipIndex) - joint);
	}

	// Pointing gesture: Used for (?)
	private void CheckPointing()
	{
		// Determine the position of each finger based on its angle
		bool indexStraight = FingerAngle(5, 6, 8) > 160f;
		bool middleCurled = FingerAngle(9, 10, 12) < 145f;
		bool ringCurled = FingerAngle(13, 14, 16) < 145f;
		bool pinkyCurled = FingerAngle(17, 18, 20) < 145f;
		bool thumbCurled = FingerAngle(1, 2, 4) < 145f;

		// Assign the required finger positions to the gesture
		bool isPointing = indexStraight && middleCurled && ringCurled && pinkyCurled && thumbCurled;

		// Only print the debug message once, after the gesture is registered
		if (isPointing && !wasPointing)
		{
			Debug.Log("POINTING!");

			// Invoke the event to be used in other scripts for implementation
			onPointing.Invoke();
		}

		// Mark that the gesture just happened
		wasPointing = isPointing;
	}

	// Peace Sign gesture: Used for (?)
	private void CheckPeaceSign()
	{
		// Determine the position of each finger based on its angle
		bool indexStraight = FingerAngle(5, 6, 8) > 160f;
		bool middleStraight = FingerAngle(9, 10, 12) > 160f;
		bool ringCurled = FingerAngle(13, 14, 16) < 145f;
		bool pinkyCurled = FingerAngle(17, 18, 20) < 145f;
		bool thumbCurled = FingerAngle(1, 2, 4) < 145f;

		// Assign the required finger positions to the gesture
		bool isPeaceSign = indexStraight && middleStraight && ringCurled && pinkyCurled && thumbCurled;

		// Only print the debug message once, after the gesture is registered
		if (isPeaceSign && !wasPeaceSign)
		{
			Debug.Log("PEACE!");

			// Invoke the event to be used in other scripts for implementation
			onPeaceSign.Invoke();
		}

		// Mark that the gesture just happened
		wasPeaceSign = isPeaceSign;
	}

	// Open Hand gesture: Used as a platform for the player
	private void CheckOpenHand()
	{
		// Determine the position of each finger based on its angle
		bool indexStraight = FingerAngle(5, 6, 8) > 160f;
		bool middleStraight = FingerAngle(9, 10, 12) > 160f;
		bool ringStraight = FingerAngle(13, 14, 16) > 160f;
		bool pinkyStraight = FingerAngle(17, 18, 20) > 160f;
		bool thumbStraight = FingerAngle(1, 2, 4) > 160f;
		// Assign the required finger positions to the gesture
		bool isOpenHand = indexStraight && middleStraight && ringStraight && pinkyStraight && thumbStraight;

		// Only print the debug message once, after the gesture is registered
		if (isOpenHand && !wasOpenHand)
		{
			Debug.Log("OPENED!");

			// Invoke the event to be used in other scripts for implementation
			onOpenHand.Invoke();
		}

		// Mark that the gesture just happened
		wasOpenHand = isOpenHand;
	}
}