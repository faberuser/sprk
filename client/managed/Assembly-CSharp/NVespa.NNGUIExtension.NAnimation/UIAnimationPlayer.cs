using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NWindow;
using NVespa.NPrefab2.NEvent;
using UnityEngine;

namespace NVespa.NNGUIExtension.NAnimation;

public class UIAnimationPlayer : MonoBehaviour, IInspectorGuiDrawer, IReleaseEvent
{
	private List<IUpdater> _listUpdater;

	private List<IUpdater> _listRemovedUpdater = new List<IUpdater>();

	private BaseWindow _baseWindow;

	private bool _IgnoreTimeScale;

	private UIPanel _clippingPanel;

	private float _value;

	protected virtual float GetUpdateValue(BaseData.EUpdateValueType updateType)
	{
		switch (updateType)
		{
		case BaseData.EUpdateValueType.Time:
			if (!_IgnoreTimeScale)
			{
				return Time.deltaTime;
			}
			return RealTime.deltaTime;
		case BaseData.EUpdateValueType.ScrollViewHorizontal:
			if (!(_clippingPanel != null))
			{
				return 0f;
			}
			return _clippingPanel.UIAnimationScrollPosition.x;
		case BaseData.EUpdateValueType.ScrollViewVertical:
			if (!(_clippingPanel != null))
			{
				return 0f;
			}
			return _clippingPanel.UIAnimationScrollPosition.y;
		default:
			return 0f;
		}
	}

	public bool Setup(BaseWindow baseWindow, BaseAnimationPlayData data, GameObject[] objs, UIPanel panel, float delay)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(objs))
		{
			return false;
		}
		if (data == null || !data.IsValid())
		{
			return false;
		}
		if (baseWindow == null)
		{
			return false;
		}
		Reset();
		_baseWindow = baseWindow;
		_IgnoreTimeScale = data.IgnoreTimeScale;
		_clippingPanel = panel;
		CreateUpdaterStock stock = CreateUpdaterStock(panel, delay, data.IsReverse);
		_listUpdater = data.GetUpdaterList(objs, stock);
		base.enabled = true;
		return true;
	}

	public void UpdaterRefresh()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listUpdater); i++)
		{
			_listUpdater[i]?.Refresh();
		}
	}

	public void Setup(BaseWindow baseWindow, BaseAnimationPlayData data)
	{
		if (data != null && data.IsValid() && !(baseWindow == null))
		{
			Reset();
			_baseWindow = baseWindow;
			_IgnoreTimeScale = data.IgnoreTimeScale;
			CreateUpdaterStock stock = CreateUpdaterStock(null, 0f, data.IsReverse);
			base.enabled = true;
			_listUpdater = data.GetUpdaterList(null, stock);
		}
	}

	public void SetupNonReset(BaseWindow baseWindow, BaseAnimationPlayData data)
	{
		if (data != null && data.IsValid() && !(baseWindow == null))
		{
			_baseWindow = baseWindow;
			_IgnoreTimeScale = data.IgnoreTimeScale;
			CreateUpdaterStock stock = CreateUpdaterStock(null, 0f, data.IsReverse);
			base.enabled = true;
			_listUpdater = data.GetUpdaterList(null, stock);
		}
	}

	private CreateUpdaterStock CreateUpdaterStock(UIPanel panel, float delay, bool isReverse)
	{
		CreateUpdaterStock createUpdaterStock = new CreateUpdaterStock
		{
			IsReverse = isReverse,
			Delay = delay
		};
		if (panel != null)
		{
			createUpdaterStock.PosX = panel.clipOffset.x;
			createUpdaterStock.PosY = panel.clipOffset.y;
		}
		return createUpdaterStock;
	}

	private void Update()
	{
		if (_baseWindow == null || _baseWindow.WindowState != BaseWindow.EState.OpenedShow || IsEnd())
		{
			return;
		}
		for (int num = __Extension_Method_ICollection__.COUNT(_listUpdater) - 1; num >= 0; num--)
		{
			IUpdater updater = _listUpdater[num];
			_value = GetUpdateValue(updater.UpdateValueType);
			if (!updater.IsEnd())
			{
				updater.OnUpdate(_value);
			}
			else
			{
				_listRemovedUpdater.Add(updater);
				_listUpdater.Remove(updater);
			}
		}
		IsEnd();
	}

	public bool IsEnd()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_listUpdater))
		{
			return true;
		}
		for (int i = 0; i < _listUpdater.Count(); i++)
		{
			if (!_listUpdater[i].IsEnd())
			{
				return false;
			}
		}
		return true;
	}

	public void Close()
	{
		Reset();
	}

	private void Reset()
	{
		_IgnoreTimeScale = false;
		_baseWindow = null;
		_clippingPanel = null;
		_value = 0f;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listRemovedUpdater); i++)
		{
			_listRemovedUpdater[i]?.Reset();
		}
		_listRemovedUpdater.Clear();
		if (_listUpdater != null)
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_listUpdater); j++)
			{
				_listUpdater[j]?.Reset();
			}
			_listUpdater.Clear();
		}
	}

	public static void Begin(BaseWindow baseWindow, BaseAnimationPlayData data, GameObject obj)
	{
		if (!(obj == null) && data != null && data.IsValid() && !(baseWindow == null))
		{
			Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<UIAnimationPlayer>(obj).Setup(baseWindow, data);
		}
	}

	public static UIAnimationPlayer Begin(BaseWindow baseWindow, BaseAnimationPlayData data, GameObject obj, UIPanel panel, float delay)
	{
		if (obj == null)
		{
			return null;
		}
		if (data == null || !data.IsValid())
		{
			return null;
		}
		if (baseWindow == null)
		{
			return null;
		}
		UIAnimationPlayer componentAuto = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<UIAnimationPlayer>(obj);
		if (componentAuto.Setup(baseWindow, data, new GameObject[1] { obj }, panel, delay))
		{
			return componentAuto;
		}
		return null;
	}

	void IReleaseEvent.OnRelease()
	{
		Reset();
		base.enabled = false;
	}
}
