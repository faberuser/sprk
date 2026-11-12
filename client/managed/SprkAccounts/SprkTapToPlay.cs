using NGame2.NUI.NWindow;
using UnityEngine;

public sealed class SprkTapToPlay : MonoBehaviour
{
	public const float Scale = 0.8f;

	public const float FallbackY = -464f;

	private UIWidget art;

	private Transform root;

	private LoginAccountInfo menu;

	private float nextSearch;

	private static float lineWorldY;

	private static bool hasLine;

	public static float CaptionCenterY(UILabel caption, Transform space)
	{
		Vector3 vector = (caption.worldCorners[0] + caption.worldCorners[3]) * 0.5f;
		return space.InverseTransformPoint(vector + caption.transform.TransformVector(new Vector3(0f, caption.printedSize.y * 0.5f, 0f))).y;
	}

	public static bool TryGetLineWorldY(out float y)
	{
		y = lineWorldY;
		return hasLine;
	}

	private void Start()
	{
		root = base.transform.Find("Panel/Root");
		Transform transform = ((root != null) ? root.Find("Sprite_backGround") : null);
		art = ((transform != null) ? transform.GetComponent<UIWidget>() : null);
		if (!(art == null))
		{
			art.SetAnchor((Transform)null);
			art.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
			LateUpdate();
		}
	}

	private void LateUpdate()
	{
		if (!(art == null))
		{
			if (menu == null && Time.unscaledTime >= nextSearch)
			{
				nextSearch = Time.unscaledTime + 0.5f;
				menu = Object.FindObjectOfType<LoginAccountInfo>();
			}
			float num = -464f;
			UILabel uILabel = ((menu != null && menu.LanguageButtonRoot != null) ? menu.LanguageButtonRoot.GetComponentInChildren<UILabel>(includeInactive: true) : null);
			if (uILabel != null && uILabel.gameObject.activeInHierarchy)
			{
				num = CaptionCenterY(uILabel, root);
			}
			Vector3 localPosition = art.transform.localPosition;
			if (Mathf.Abs(localPosition.x) > 0.01f || Mathf.Abs(localPosition.y - num) > 0.01f)
			{
				art.transform.localPosition = new Vector3(0f, num, localPosition.z);
			}
			lineWorldY = root.TransformPoint(new Vector3(0f, num, 0f)).y;
			hasLine = true;
		}
	}
}
