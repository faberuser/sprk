using System;
using NGame2.NUI.NWindow;
using UnityEngine;

public static class SprkIllustrationZoom
{
	public static void Update(HeroIllustBook window)
	{
		if (window == null || !window.IsOpen() || !window.gameObject.activeInHierarchy || !Application.isFocused || window.IllustRoot == null || window.RootPanel == null || window.HeroIllustTexture == null || window.HeroIllustTexture.mainTexture == null)
		{
			return;
		}
		float axis = Input.GetAxis("Mouse ScrollWheel");
		bool mouseButtonDown = Input.GetMouseButtonDown(2);
		if (axis == 0f && !mouseButtonDown)
		{
			return;
		}
		GameObject hoveredObject = UICamera.hoveredObject;
		if (hoveredObject != null && (!hoveredObject.transform.IsChildOf(window.transform) || hoveredObject.GetComponentInParent<UIButton>() != null))
		{
			return;
		}
		if (mouseButtonDown)
		{
			window.OnDoubleClick();
			return;
		}
		Transform illustRoot = window.IllustRoot;
		Vector3 localScale = illustRoot.localScale;
		float num = SprkIllustrationZoomMath.NextScale(localScale.x, axis);
		if (!(Math.Abs(num - localScale.x) < 1E-05f))
		{
			if (window.TweenScale != null)
			{
				window.TweenScale.enabled = false;
			}
			if (window.TweenPosition != null)
			{
				window.TweenPosition.enabled = false;
			}
			Vector3 localPosition = illustRoot.localPosition;
			float num2 = ((localScale.x > 0f) ? (num / localScale.x) : 1f);
			localPosition.x *= num2;
			localPosition.y *= num2;
			localScale.x = (localScale.y = num);
			illustRoot.localScale = localScale;
			Vector2 localSize = window.RootPanel.localSize;
			localPosition.x = SprkIllustrationZoomMath.ClampOffset(localPosition.x, num, window.HeroIllustTexture.width, localSize.x);
			localPosition.y = SprkIllustrationZoomMath.ClampOffset(localPosition.y, num, window.HeroIllustTexture.height, localSize.y);
			illustRoot.localPosition = localPosition;
		}
	}
}
