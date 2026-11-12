using UnityEngine;

public static class Vespa_Unity_Camera_Extension_Method
{
	private static Vector3 ConvertFromScreen(Camera curCamera, Vector3 position)
	{
		if (curCamera.targetTexture == null)
		{
			return position;
		}
		int width = Screen.width;
		int height = Screen.height;
		int width2 = curCamera.targetTexture.width;
		int height2 = curCamera.targetTexture.height;
		return new Vector3(position.x * (float)width2 / (float)width, position.y * (float)height2 / (float)height, position.z);
	}

	private static Vector3 ConvertToScreen(Camera curCamera, Vector3 position)
	{
		if (curCamera.targetTexture == null)
		{
			return position;
		}
		int width = Screen.width;
		int height = Screen.height;
		int width2 = curCamera.targetTexture.width;
		int height2 = curCamera.targetTexture.height;
		return new Vector3(position.x * (float)width / (float)width2, position.y * (float)height / (float)height2, position.z);
	}

	public static Ray VespaScreenPointToRay(this Camera This, Vector3 position)
	{
		if (This == null)
		{
			return new Ray(Vector3.zero, Vector3.forward);
		}
		Vector3 pos = ConvertFromScreen(This, position);
		return This.ScreenPointToRay(pos);
	}

	public static Vector3 VespaScreenToViewportPoint(this Camera This, Vector3 position)
	{
		if (This == null)
		{
			return position;
		}
		Vector3 position2 = ConvertFromScreen(This, position);
		return This.ScreenToViewportPoint(position2);
	}

	public static Vector3 VespaScreenToWorldPoint(this Camera This, Vector3 position)
	{
		if (This == null)
		{
			return position;
		}
		Vector3 position2 = ConvertFromScreen(This, position);
		return This.ScreenToWorldPoint(position2);
	}

	public static Vector3 VespaViewportToScreenPoint(this Camera This, Vector3 position)
	{
		if (This == null)
		{
			return position;
		}
		This.ViewportToScreenPoint(position);
		return ConvertToScreen(This, position);
	}

	public static Vector3 VespaWorldToScreenPoint(this Camera This, Vector3 position)
	{
		if (This == null)
		{
			return position;
		}
		This.WorldToScreenPoint(position);
		return ConvertToScreen(This, position);
	}
}
