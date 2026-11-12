using UnityEngine;

public static class Vespa_Unity_Rect_Extension_Method
{
	public static Rect Merge(this Rect i_self, Rect i_rect)
	{
		if (i_self.center.x == 0f && i_self.center.y == 0f && i_self.width == 0f && i_self.height == 0f)
		{
			return i_rect;
		}
		return Rect.MinMaxRect(Mathf.Min(i_self.xMin, i_rect.xMin), Mathf.Min(i_self.yMin, i_rect.yMin), Mathf.Max(i_self.xMax, i_rect.xMax), Mathf.Max(i_self.yMax, i_rect.yMax));
	}

	public static Rect And(this Rect i_self, Rect i_rect)
	{
		if (i_self.center.x == 0f && i_self.center.y == 0f && i_self.width == 0f && i_self.height == 0f)
		{
			return i_rect;
		}
		return Rect.MinMaxRect(Mathf.Max(i_self.xMin, i_rect.xMin), Mathf.Max(i_self.yMin, i_rect.yMin), Mathf.Min(i_self.xMax, i_rect.xMax), Mathf.Min(i_self.yMax, i_rect.yMax));
	}

	public static Rect EnlargeRect(this Rect i_rect, float i_x, float i_y)
	{
		return Rect.MinMaxRect(i_rect.xMin - i_x, i_rect.yMin - i_y, i_rect.xMax + i_x, i_rect.yMax + i_y);
	}

	public static string MinMaxString(this Rect This)
	{
		return $"(xMin:{This.xMin}, yMin:{This.yMin}, xMax:{This.xMax}, yMax:{This.yMax})";
	}

	public static string MinMaxString(this Rect This, string format)
	{
		return $"(xMin:{This.xMin.ToString(format)}, yMin:{This.yMin.ToString(format)}, xMax:{This.xMax.ToString(format)}, yMax:{This.yMax.ToString(format)})";
	}
}
