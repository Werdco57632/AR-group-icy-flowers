using UnityEngine;
using UnityEngine.Events;
using Mediapipe.Tasks.Vision.HandLandmarker;

public class HandGestures : MonoBehaviour
{
    [SerializeField] // allow private variables to be edited in Unity but not by other scripts
	private UnityEvent onPointingUp = new UnityEvent();

	private bool wasPointing = false;
	private int pointingTime = 0;
	
	public void ProcessHand(HandLandmarkerResult result)
	{
		// Make sure a hand exists
		if (result.handLandmarks == null || result.handLandmarks.Count == 0)
			return;

		// Get the first detected hand's joint positions
		var landmarks = result.handLandmarks[0].landmarks;

		// Make sure a 5-fingered hand exists
		if (landmarks == null || landmarks.Count < 21)
			return;

		// NOTE: Will adjust for actual camera aspect ratio
		float cameraAspect = 1f;

		// Find the finger angle by calculating the distance between joints
		float FingerAngle(int baseIndex, int jointIndex, int tipIndex)
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

		// Determine the position of each finger based on its angle
		bool indexStraight = FingerAngle(5, 6, 8) > 160f;
		bool middleCurled = FingerAngle(9, 10, 12) < 145f;
		bool ringCurled = FingerAngle(13, 14, 16) < 145f;
		bool pinkyCurled = FingerAngle(17, 18, 20) < 145f;

		// Pointing has all fingers curled with a straight index finger
		bool isPointing = indexStraight && middleCurled && ringCurled && pinkyCurled;

		// Only print the debug message once after pointing is registered
		if (isPointing && !wasPointing)
		{
			Debug.Log("POINTING!");
		}

		// Mark that pointing just happened
		wasPointing = isPointing;
	}
}
