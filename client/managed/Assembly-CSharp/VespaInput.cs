using UnityEngine;

public static class VespaInput
{
	public static int touchCount => Input.touchCount;

	public static Vector3 mousePosition => Input.mousePosition;

	public static Touch GetTouch(int index)
	{
		return Input.GetTouch(index);
	}

	public static bool GetMouseButton(int button)
	{
		return Input.GetMouseButton(button);
	}

	public static bool GetMouseButtonDown(int button)
	{
		return Input.GetMouseButtonDown(button);
	}

	public static bool GetMouseButtonUp(int button)
	{
		return Input.GetMouseButtonUp(button);
	}

	public static float GetWheelAxis()
	{
		return Input.GetAxis("Mouse ScrollWheel");
	}
}
