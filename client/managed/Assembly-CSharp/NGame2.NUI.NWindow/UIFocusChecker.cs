using System.Collections.Generic;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class UIFocusChecker : MonoBehaviour
{
	public bool IsCheckFocusOut;

	public GameObject[] FocusCheckObject;

	[HideInInspector]
	public List<EventDelegate> focusOut = new List<EventDelegate>();

	private Camera[] useCam;

	private Bounds[] objBounds;

	private bool _isInitBoudns;

	private void Awake()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(FocusCheckObject))
		{
			Setup();
		}
	}

	private void Setup()
	{
		int num = FocusCheckObject.Length;
		useCam = new Camera[num];
		objBounds = new Bounds[num];
		for (int i = 0; i < num; i++)
		{
			LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(FocusCheckObject[i].layer);
			if (layerInfo.Camera != null)
			{
				useCam[i] = layerInfo.Camera;
			}
		}
	}

	public void SetupBounds()
	{
		if (_isInitBoudns)
		{
			return;
		}
		for (int i = 0; i < FocusCheckObject.Length; i++)
		{
			if (!(FocusCheckObject[i] == null))
			{
				objBounds[i] = NGUIMath.CalculateAbsoluteWidgetBounds(FocusCheckObject[i].transform);
			}
		}
		_isInitBoudns = true;
	}

	private bool IsEnableSetting()
	{
		if (!IsCheckFocusOut)
		{
			return false;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(FocusCheckObject))
		{
			return !__Extension_Method_ICollection__.IsNullOrEmpty(useCam);
		}
		return false;
	}

	private bool IsAllObjectActive()
	{
		for (int i = 0; i < FocusCheckObject.Length; i++)
		{
			if (FocusCheckObject[i] == null)
			{
				return false;
			}
			if (!FocusCheckObject[i].activeInHierarchy || !FocusCheckObject[i].activeSelf)
			{
				return false;
			}
		}
		return true;
	}

	public bool isFucosOn()
	{
		SetupBounds();
		Vector3 mousePosition = VespaInput.mousePosition;
		for (int i = 0; i < FocusCheckObject.Length; i++)
		{
			if (useCam[i] == null || FocusCheckObject[i] == null)
			{
				return false;
			}
			Vector3 vector = Vespa_Unity_Camera_Extension_Method.VespaScreenToWorldPoint(useCam[i], mousePosition);
			bool num = objBounds[i].min.x < vector.x && vector.x < objBounds[i].max.x;
			bool flag = objBounds[i].min.y < vector.y && vector.y < objBounds[i].max.y;
			if (num && flag)
			{
				return true;
			}
		}
		return false;
	}

	private void Update()
	{
		if (IsEnableSetting() && VespaInput.GetMouseButtonDown(0) && IsAllObjectActive() && !isFucosOn())
		{
			EventDelegate.Execute(focusOut);
		}
	}
}
