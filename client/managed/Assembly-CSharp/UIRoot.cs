using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Root")]
public class UIRoot : MonoBehaviour
{
	public enum Scaling
	{
		Flexible,
		Constrained,
		ConstrainedOnMobiles
	}

	public enum Constraint
	{
		Fit,
		Fill,
		FitWidth,
		FitHeight
	}

	public static List<UIRoot> list = new List<UIRoot>();

	public Scaling scalingStyle;

	public int manualWidth = 1280;

	public int manualHeight = 720;

	public int minimumHeight = 320;

	public int maximumHeight = 1536;

	public bool fitWidth;

	public bool fitHeight = true;

	public bool adjustByDPI;

	public bool shrinkPortraitUI;

	private Transform mTrans;

	// SPRK: user UI scale for the game's main 1920x1080 roots. Shrinking the
	// virtual screen enlarges the UI while anchors still follow the screen edges.
	[NonSerialized]
	public bool sprkUserScale;

	public const string SprkUserScaleKey = "Sprk.Graphics.UIScale";

	public static readonly int[] SprkUserScaleChoices = new int[5] { 80, 90, 100, 110, 120 };

	private static int sprkUserScalePercent = -1;

	public static int SprkUserScalePercent
	{
		get
		{
			if (sprkUserScalePercent < 0)
			{
				sprkUserScalePercent = SprkClampUserScale(SprkTestPrefs.GetInt(SprkUserScaleKey, 100));
			}
			return sprkUserScalePercent;
		}
	}

	private static int SprkClampUserScale(int percent)
	{
		return Mathf.Clamp(percent, SprkUserScaleChoices[0], SprkUserScaleChoices[SprkUserScaleChoices.Length - 1]);
	}

	public static void SetSprkUserScale(int percent)
	{
		percent = SprkClampUserScale(percent);
		if (percent == SprkUserScalePercent)
		{
			return;
		}
		sprkUserScalePercent = percent;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] != null)
			{
				list[i].Update();
			}
		}
		Broadcast("UpdateAnchors");
		if (UICamera.onScreenResize != null)
		{
			UICamera.onScreenResize();
		}
	}

	public Constraint constraint
	{
		get
		{
			if (fitWidth)
			{
				if (fitHeight)
				{
					return Constraint.Fit;
				}
				return Constraint.FitWidth;
			}
			if (fitHeight)
			{
				return Constraint.FitHeight;
			}
			return Constraint.Fill;
		}
	}

	public Scaling activeScaling
	{
		get
		{
			Scaling scaling = scalingStyle;
			if (scaling == Scaling.ConstrainedOnMobiles)
			{
				return Scaling.Flexible;
			}
			return scaling;
		}
	}

	public int activeHeight
	{
		get
		{
			int num = nativeActiveHeight;
			if (!sprkUserScale || SprkUserScalePercent == 100)
			{
				return num;
			}
			return Mathf.Max(2, Mathf.RoundToInt((float)num * 100f / (float)SprkUserScalePercent));
		}
	}

	private int nativeActiveHeight
	{
		get
		{
			if (activeScaling == Scaling.Flexible)
			{
				Vector2 screenSize = NGUITools.screenSize;
				float num = screenSize.x / screenSize.y;
				if (screenSize.y < (float)minimumHeight)
				{
					screenSize.y = minimumHeight;
					screenSize.x = screenSize.y * num;
				}
				else if (screenSize.y > (float)maximumHeight)
				{
					screenSize.y = maximumHeight;
					screenSize.x = screenSize.y * num;
				}
				int num2 = Mathf.RoundToInt((shrinkPortraitUI && screenSize.y > screenSize.x) ? (screenSize.y / num) : screenSize.y);
				if (!adjustByDPI)
				{
					return num2;
				}
				return NGUIMath.AdjustByDPI(num2);
			}
			Constraint constraint = this.constraint;
			if (constraint == Constraint.FitHeight)
			{
				return manualHeight;
			}
			Vector2 screenSize2 = NGUITools.screenSize;
			float num3 = screenSize2.x / screenSize2.y;
			float num4 = (float)manualWidth / (float)manualHeight;
			switch (constraint)
			{
			case Constraint.FitWidth:
				return Mathf.RoundToInt((float)manualWidth / num3);
			case Constraint.Fit:
				if (!(num4 > num3))
				{
					return manualHeight;
				}
				return Mathf.RoundToInt((float)manualWidth / num3);
			case Constraint.Fill:
				if (!(num4 < num3))
				{
					return manualHeight;
				}
				return Mathf.RoundToInt((float)manualWidth / num3);
			default:
				return manualHeight;
			}
		}
	}

	public float pixelSizeAdjustment
	{
		get
		{
			int num = Mathf.RoundToInt(NGUITools.screenSize.y);
			if (num != -1)
			{
				return GetPixelSizeAdjustment(num);
			}
			return 1f;
		}
	}

	public static float GetPixelSizeAdjustment(GameObject go)
	{
		UIRoot uIRoot = NGUITools.FindInParents<UIRoot>(go);
		if (!(uIRoot != null))
		{
			return 1f;
		}
		return uIRoot.pixelSizeAdjustment;
	}

	public float GetPixelSizeAdjustment(int height)
	{
		height = Mathf.Max(2, height);
		if (activeScaling == Scaling.Constrained)
		{
			return (float)activeHeight / (float)height;
		}
		if (height < minimumHeight)
		{
			return (float)minimumHeight / (float)height;
		}
		if (height > maximumHeight)
		{
			return (float)maximumHeight / (float)height;
		}
		return 1f;
	}

	protected virtual void Awake()
	{
		mTrans = base.transform;
	}

	protected virtual void OnEnable()
	{
		list.Add(this);
	}

	protected virtual void OnDisable()
	{
		list.Remove(this);
	}

	protected virtual void Start()
	{
		UIOrthoCamera componentInChildren = GetComponentInChildren<UIOrthoCamera>();
		if (componentInChildren != null)
		{
			Debug.LogWarning("UIRoot should not be active at the same time as UIOrthoCamera. Disabling UIOrthoCamera.", componentInChildren);
			Camera component = componentInChildren.gameObject.GetComponent<Camera>();
			componentInChildren.enabled = false;
			if (component != null)
			{
				component.orthographicSize = 1f;
			}
		}
		else
		{
			Update();
		}
	}

	private void Update()
	{
		if (mTrans == null)
		{
			return;
		}
		float num = activeHeight;
		if (!(num < Mathf.Epsilon))
		{
			float num2 = 2f / num;
			Vector3 vector = Vespa_Unity_Vector3_Extension_Method.Abs(mTrans.localScale - Vector3.one * num2);
			if (!(vector.x < Mathf.Epsilon) || !(vector.y < Mathf.Epsilon) || !(vector.z < Mathf.Epsilon))
			{
				mTrans.localScale = new Vector3(num2, num2, num2);
			}
		}
	}

	public static void Broadcast(string funcName)
	{
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			UIRoot uIRoot = list[i];
			if (uIRoot != null)
			{
				uIRoot.BroadcastMessage(funcName, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public static void Broadcast(string funcName, object param)
	{
		if (param == null)
		{
			Debug.LogError("SendMessage is bugged when you try to pass 'null' in the parameter field. It behaves as if no parameter was specified.");
			return;
		}
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			UIRoot uIRoot = list[i];
			if (uIRoot != null)
			{
				uIRoot.BroadcastMessage(funcName, param, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
