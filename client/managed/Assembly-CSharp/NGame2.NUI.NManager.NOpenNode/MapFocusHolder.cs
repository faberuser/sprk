using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGame2.NUI.NWorldMap;
using NGame2.NUI.NWorldMap.NMapLayout;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NManager.NOpenNode;

public class MapFocusHolder : IDisposable, ITouchDisabler, IObserver
{
	private bool _enable = true;

	private bool _disposed;

	private MapLayout _mapLayout;

	private Action _onComplete;

	bool ITouchDisabler.IsValid => true;

	public MapFocusHolder(bool focusDisable)
	{
		bool valueBool = GameRoot.Instance.ConstantDataContainer.GetValueBool("NodeOpenFocusDisable");
		if (!(focusDisable || valueBool))
		{
			Init();
		}
	}

	private void Init()
	{
		if (SceneAutoBehaviour<WorldMapManagement>.isValidInstance)
		{
			_mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
			if (GetMapViewer() != null)
			{
				GetMapViewer().AddObserver(this);
			}
		}
	}

	public MapViewer GetMapViewer()
	{
		if (!(_mapLayout == null))
		{
			return _mapLayout.MapViewer;
		}
		return null;
	}

	public void Focus(List<Tuple<int, int, bool>> focusNodeList, Action onComplete)
	{
		_onComplete = onComplete;
		if (__Extension_Method_ICollection__.COUNT(focusNodeList) == 0)
		{
			OnComplete();
			return;
		}
		int num = focusNodeList.FindIndex(delegate(Tuple<int, int, bool> node)
		{
			CampaignNodeData data2 = GameRoot.Instance.CampaignNodeDataContainer.GetData(NGame2.NAccount.UserCampaignManager.ChapterIndex, node.Item2);
			return data2 != null && !__Extension_Method_ICollection__.IsNullOrEmpty(data2.NodeOpenValues);
		});
		_enable = num != -1;
		if (!IsValid())
		{
			OnComplete();
			return;
		}
		GlobalTopmostWindow.instance.AddTouchDisabler(this);
		MapViewer mapViewer = GetMapViewer();
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(NGame2.NAccount.UserCampaignManager.ChapterIndex, focusNodeList[num].Item2);
		float orthoSize = Vespa_String_Extension_Method.TOFLOAT(data.NodeOpenValues[0], 1f);
		int num2 = Vespa_String_Extension_Method.TOINT(data.NodeOpenValues[1], data.PortalPositionX);
		int num3 = Vespa_String_Extension_Method.TOINT(data.NodeOpenValues[2], data.PortalPositionY);
		Vector3 vector = mapViewer.ConvertNodePostion(new Vector2(num2, num3));
		mapViewer.FocusPositionMode(blendTime: (float)GameRoot.Instance.ConstantDataContainer.GetValueInt("NodeOpenEffectCameraBlendTime", 1000) / 1000f, orthoSize: orthoSize, v2Position: vector, onComplete: delegate
		{
			OnComplete();
		});
	}

	public bool IsValid()
	{
		if (_mapLayout == null)
		{
			return false;
		}
		if (!_enable)
		{
			return false;
		}
		if (_disposed)
		{
			return false;
		}
		return true;
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			OnComplete();
			_disposed = true;
			_enable = false;
			if (GetMapViewer() != null)
			{
				GetMapViewer().ReleaseFocusNode(null);
				GetMapViewer().RemoveObserver(this);
			}
			GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
		}
	}

	private void OnComplete()
	{
		Vespa_System_Action_Extension_Method.fire(_onComplete);
		_onComplete = null;
	}

	void IObserver.OnEvent(FocusEvent focusEvent)
	{
		if (focusEvent == FocusEvent.Release)
		{
			Dispose();
		}
	}

	void IObserver.OnTouch(bool isPress)
	{
	}
}
