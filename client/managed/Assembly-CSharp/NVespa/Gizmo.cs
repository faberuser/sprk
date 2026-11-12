using UnityEngine;

namespace NVespa;

public static class Gizmo
{
	public static void DrawArrow(Vector3 i_start, Vector3 i_end)
	{
		float num = Vector3.Distance(i_start, i_end);
		if (!(num < Mathf.Epsilon))
		{
			Gizmos.DrawLine(i_start, i_end);
			Vector3 normalized = (i_end - i_start).normalized;
			Vector3 vector = ((!(Vector3.Distance(normalized, Vector3.up) < Mathf.Epsilon)) ? Vector3.Cross(normalized, Vector3.up).normalized : Vector3.right);
			float num2 = Mathf.Min(1f, num) * 0.2f;
			Gizmos.DrawLine(i_end, i_end - (normalized + vector) * num2);
			Gizmos.DrawLine(i_end, i_end - (normalized - vector) * num2);
		}
	}
}
