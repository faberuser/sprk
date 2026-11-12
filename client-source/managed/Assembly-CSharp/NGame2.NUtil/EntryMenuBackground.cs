using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NGameSetting;
using NGame2.NUI;
using NGameAsset;
using NGameAsset.NNgui;
using NGameAsset.NNgui.NComponent;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUtil;

public class EntryMenuBackground
{
	private Camera _displayCamera;

	private Transform _rootTm;

	private List<WidgetInstance> _backgroundList = new List<WidgetInstance>();

	private List<WidgetInstance> _popupList = new List<WidgetInstance>();

	private List<WidgetInstance> _lastList = new List<WidgetInstance>();

	private Color _color;

	public EntryMenuSettingData MenuAppearance { get; private set; }

	public INguiPrefab NguiPrefab { get; private set; }

	public NguiInstance NguiInstance { get; private set; }

	public bool Open(string entryMenuType, GameUIRoot.ELayerType layerType)
	{
		EntryMenuSettingData dataByMenuType = GameRoot.Instance.EntryMenuSettingDataContainer.GetDataByMenuType(entryMenuType);
		if (dataByMenuType == null)
		{
			return false;
		}
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(layerType);
		Open(dataByMenuType, layerInfo.Camera, GameUIRoot.instance.ContentsGroup.transform);
		return true;
	}

	public void Open(EntryMenuSettingData menuAppearance, Camera displayCamera, Transform rootTm)
	{
		MenuAppearance = menuAppearance;
		NguiPrefab = SingletonAuto<Container>.instance.LoadPrefab(MenuAppearance.AssetNguiCode);
		_displayCamera = displayCamera;
		_rootTm = rootTm;
	}

	public void Close()
	{
		if (NguiInstance != null)
		{
			GlobalAutoBehaviour<GlobalCameraManager>.instance.PopMinCameraDepth(NguiInstance.Go);
			NguiInstance.Close();
			NguiInstance = null;
		}
		_backgroundList.Clear();
		_popupList.Clear();
		_lastList.Clear();
		if (NguiPrefab != null)
		{
			NguiPrefab.Close();
			NguiPrefab = null;
		}
		MenuAppearance = null;
	}

	public void SetEnable(bool visible)
	{
		if (NguiInstance != null)
		{
			if (visible)
			{
				GlobalAutoBehaviour<GlobalCameraManager>.instance.PushMinCameraDepth(NguiInstance.Go.layer, NguiInstance.Go);
			}
			else
			{
				GlobalAutoBehaviour<GlobalCameraManager>.instance.PopMinCameraDepth(NguiInstance.Go);
			}
			UIHelper.EnableGameObject(NguiInstance.Go, visible);
		}
	}

	public void SetColor(Color color)
	{
		if (NguiInstance != null && !(color == _color))
		{
			_color = color;
			SetColorInHierarchy(NguiInstance.Go.transform);
		}
	}

	private void SetColorInHierarchy(Transform tm)
	{
		UIBasicSprite component = tm.GetComponent<UIBasicSprite>();
		if (component != null)
		{
			component.color = _color;
			return;
		}
		for (int i = 0; i < tm.childCount; i++)
		{
			Transform child = tm.GetChild(i);
			SetColorInHierarchy(child);
		}
	}

	public void ChainUpdate()
	{
		if (NguiInstance != null && _displayCamera != null)
		{
			NguiInstance.Go.transform.localScale = Vector3.one * _displayCamera.orthographicSize;
		}
	}

	public IEnumerator coShowNgui(bool skipAnim, bool reverse, Action onComplete = null)
	{
		if (!NguiPrefab.ISVALID())
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		if (NguiInstance == null)
		{
			try
			{
				NguiInstance = NguiPrefab.CreateInstance(_rootTm, _displayCamera.gameObject.layer);
				_color = Color.white;
			}
			catch (Exception)
			{
				VespaDebug.LogError("CreateInstance Fail");
			}
		}
		if (NguiInstance == null || NguiInstance.Go == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		_backgroundList.Clear();
		_popupList.Clear();
		_lastList.Clear();
		NguiInstance.Go.SetActive(value: true);
		if (reverse)
		{
			UISprite[] componentsInChildren = NguiInstance.Go.GetComponentsInChildren<UISprite>(includeInactive: true);
			if (componentsInChildren != null)
			{
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(componentsInChildren); i++)
				{
					componentsInChildren[i].flip = UIBasicSprite.Flip.Horizontally;
				}
			}
		}
		UIPanel[] componentsInChildren2 = NguiInstance.Go.GetComponentsInChildren<UIPanel>(includeInactive: true);
		if (componentsInChildren2 != null)
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(componentsInChildren2); j++)
			{
				componentsInChildren2[j].sortingOrder++;
			}
		}
		for (int k = 0; k < NguiInstance.WidgetList.Count; k++)
		{
			WidgetInstance widgetInstance = NguiInstance.WidgetList[k];
			if (!(widgetInstance.Widget == null) && widgetInstance.Data != null)
			{
				if (widgetInstance.Data.AppearanceType == WidgetData.EAppearanceType.Background)
				{
					_backgroundList.Add(widgetInstance);
					widgetInstance.Widget.alpha = 0f;
				}
				else if (widgetInstance.Data.AppearanceType == WidgetData.EAppearanceType.Popup)
				{
					_popupList.Add(widgetInstance);
					UIHelper.EnableGameObject(widgetInstance.Widget, enable: false);
				}
				else if (widgetInstance.Data.AppearanceType == WidgetData.EAppearanceType.Last)
				{
					_lastList.Add(widgetInstance);
					UIHelper.EnableGameObject(widgetInstance.Widget, enable: false);
				}
			}
		}
		yield return coShowNguiAnim1(skipAnim);
		yield return coShowNguiAnim2(skipAnim);
		yield return coShowNguiAnim3(skipAnim);
		if (NguiInstance == null || NguiInstance.Go == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		GlobalAutoBehaviour<GlobalCameraManager>.instance.PushMinCameraDepth(NguiInstance.Go.layer, NguiInstance.Go);
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	private IEnumerator coShowNguiAnim1(bool skipAnim)
	{
		EntryMenuData entryMenuSetting = GameSetting.data.EntryMenu;
		float elapsedTime = 0f;
		do
		{
			if (skipAnim)
			{
				elapsedTime += entryMenuSetting.BlendTimeLength;
			}
			else
			{
				yield return null;
				elapsedTime += (float)UnitySystemTimer.unscaledDeltaTick * 0.001f;
			}
			float alpha = Mathf.Clamp01(elapsedTime / entryMenuSetting.BlendTimeLength);
			for (int i = 0; i < _backgroundList.Count; i++)
			{
				_backgroundList[i].Widget.alpha = alpha;
			}
		}
		while (!(elapsedTime >= entryMenuSetting.BlendTimeLength));
	}

	private IEnumerator coShowNguiAnim2(bool skipAnim)
	{
		float popupInternal = 0.1f;
		if (MenuAppearance != null)
		{
			popupInternal = MenuAppearance.PopupInterval;
		}
		EntryMenuData entryMenuSetting = GameSetting.data.EntryMenu;
		float elapsedTime = 0f;
		bool flag;
		do
		{
			if (skipAnim)
			{
				elapsedTime += entryMenuSetting.BlendTimeLength;
			}
			else
			{
				yield return null;
				elapsedTime += (float)UnitySystemTimer.unscaledDeltaTick * 0.001f;
			}
			flag = true;
			for (int i = 0; i < _popupList.Count; i++)
			{
				float num = popupInternal * (float)i;
				float num2 = num + entryMenuSetting.PopupTimeLength;
				WidgetInstance widgetInstance = _popupList[i];
				float num3 = Mathf.InverseLerp(num, num2, elapsedTime);
				float y = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(entryMenuSetting.PopupCurve, num3, num3);
				UIHelper.EnableGameObject(widgetInstance.Widget, num < elapsedTime);
				if (num < elapsedTime)
				{
					widgetInstance.Widget.alpha = Mathf.Clamp01(num3 * 2f);
					widgetInstance.Widget.transform.localScale = new Vector3(1f, y, 1f);
				}
				if (elapsedTime < num2)
				{
					flag = false;
				}
			}
		}
		while (!flag);
	}

	private IEnumerator coShowNguiAnim3(bool skipAnim)
	{
		for (int index = 0; index < _lastList.Count; index++)
		{
			_ = _lastList[index];
			WidgetInstance lastWidget = _lastList[index];
			EntryMenuData entryMenuSetting = GameSetting.data.EntryMenu;
			float elapsedTime = 0f;
			lastWidget.Widget.transform.localScale = new Vector3(1f, 1f, 1f);
			lastWidget.Widget.alpha = 0f;
			UIHelper.EnableGameObject(lastWidget.Widget, enable: true);
			do
			{
				if (skipAnim)
				{
					elapsedTime += entryMenuSetting.PopupTimeLength;
				}
				else
				{
					yield return null;
					elapsedTime += (float)UnitySystemTimer.unscaledDeltaTick * 0.001f;
				}
				float alpha = Mathf.Clamp01(elapsedTime / entryMenuSetting.PopupTimeLength);
				lastWidget.Widget.alpha = alpha;
			}
			while (!(elapsedTime >= entryMenuSetting.PopupTimeLength));
		}
	}

	public IEnumerator coHideNgui(Action onComplete = null)
	{
		if (NguiInstance == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		GlobalAutoBehaviour<GlobalCameraManager>.instance.PopMinCameraDepth(NguiInstance.Go);
		EntryMenuData entryMenuSetting = GameSetting.data.EntryMenu;
		float elapsedTime = 0f;
		do
		{
			yield return null;
			elapsedTime += (float)UnitySystemTimer.unscaledDeltaTick * 0.001f;
			float num = Mathf.Clamp01(elapsedTime / entryMenuSetting.BlendTimeLength);
			for (int i = 0; i < _backgroundList.Count; i++)
			{
				UIHelper.SetWidgetAlpha(_backgroundList[i].Widget, 1f - num);
			}
			for (int j = 0; j < _popupList.Count; j++)
			{
				UIHelper.SetWidgetAlpha(_popupList[j].Widget, 1f - num);
			}
			for (int k = 0; k < _lastList.Count; k++)
			{
				UIHelper.SetWidgetAlpha(_lastList[k].Widget, 1f - num);
			}
		}
		while (!(elapsedTime >= entryMenuSetting.BlendTimeLength));
		if (NguiInstance != null)
		{
			UIHelper.EnableGameObject(NguiInstance.Go, enable: false);
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}
}
