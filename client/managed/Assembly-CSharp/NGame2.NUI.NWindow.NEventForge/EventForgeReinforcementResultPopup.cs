using System;
using System.Collections;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.EventForge;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NEventForge;

public class EventForgeReinforcementResultPopup : BaseWindow
{
	public struct Info
	{
		public EventForgeData EventForgeData { get; set; }

		public NShared.EventForge.Response ResultInfo { get; set; }

		public Action FailResultAction { get; set; }
	}

	public const string PrefabPath = "Resources/Prefab2/Window/EventForge/BaseEventForgeResultPopup";

	public Transform EffectRootTransform;

	[Header("[성공]")]
	public Transform SuccessRootTransform;

	public Transform SuccessEventItemRootTransform;

	public UILabel CurrentReinforcementLevel;

	public UILabel SuccessReinforcementLevel;

	public UIGame3DWidget SuccessReadyEffectWidget;

	public UIGame3DWidget SuccessResultEffectWidget;

	[Header("[실패]")]
	public Transform FailRootTransform;

	public Transform FailEventItemRootTransform;

	public UIGame3DWidget FailReadyEffectWidget;

	public UIGame3DWidget FailResultEffectWidget;

	public Transform EmptyIconTransform;

	public float WaitSecondsEnableFailEventItem;

	private Info? _info;

	private bool _isShowResult;

	private ItemIconComponent _resultEventItemIconComponent;

	private bool IsSuccess
	{
		get
		{
			if (!_info.HasValue || !_info.HasValue)
			{
				return false;
			}
			return _info.Value.ResultInfo?.Success ?? false;
		}
	}

	public void Open(Info info)
	{
		Open(delegate
		{
			Init(info);
		}, null);
	}

	private void Init(Info? info)
	{
		if (!info.HasValue || !info.HasValue)
		{
			Close();
			return;
		}
		_info = info;
		_isShowResult = false;
		UIHelper.EnableGameObject(EffectRootTransform, enable: true);
		UIHelper.PlaySound("FX.Air.AirWaver01");
		InitResultInfo();
	}

	public override void OnSystemBackButton()
	{
		base.OnSystemBackButton();
		if (_isShowResult)
		{
			Close();
		}
		else
		{
			ShowResult();
		}
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ReleaseResultEventItemIconComponent();
	}

	private void InitResultInfo()
	{
		ResultEventItemIconComponent();
		ResultSuccess();
		UIHelper.EnableGameObject(SuccessRootTransform, enable: false);
		UIHelper.EnableGameObject(FailRootTransform, enable: false);
	}

	private void ResultSuccess()
	{
		if (_info.HasValue && _info.HasValue)
		{
			NShared.EventForge.Response resultInfo = _info.Value.ResultInfo;
			if (resultInfo != null && resultInfo.Success && resultInfo.ResultEquipItem != null)
			{
				int num = resultInfo.ResultEquipItem.Level - 1;
				UIHelper.SetLabelText(CurrentReinforcementLevel, "+" + num);
				UIHelper.SetLabelText(SuccessReinforcementLevel, "+" + resultInfo.ResultEquipItem.Level);
			}
		}
	}

	private void ReleaseResultEventItemIconComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _resultEventItemIconComponent);
	}

	private void ResultEventItemIconComponent()
	{
		ReleaseResultEventItemIconComponent();
		if (!_info.HasValue || !_info.HasValue)
		{
			return;
		}
		NShared.EventForge.Response resultInfo = _info.Value.ResultInfo;
		if (resultInfo == null || resultInfo.ResultEquipItem == null)
		{
			return;
		}
		Transform transform = (resultInfo.Success ? SuccessEventItemRootTransform : FailEventItemRootTransform);
		if (!(transform == null))
		{
			_resultEventItemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(transform);
			if (!(_resultEventItemIconComponent == null))
			{
				_resultEventItemIconComponent.Open(resultInfo.ResultEquipItem, null);
				_resultEventItemIconComponent.ResetPosition();
			}
		}
	}

	private void ShowResult()
	{
		_isShowResult = true;
		UIHelper.EnableGameObject(EffectRootTransform, enable: false);
		if (!_info.HasValue || !_info.HasValue)
		{
			return;
		}
		bool isSuccess = IsSuccess;
		UIHelper.EnableGameObject(SuccessRootTransform, isSuccess);
		UIHelper.EnableGameObject(FailRootTransform, !isSuccess);
		UIHelper.EnableGameObject(EmptyIconTransform, enable: false);
		if (isSuccess)
		{
			UIHelper.PlaySound("UI.SoulWeapon.EtherSuccess");
			PlayEffect(SuccessReadyEffectWidget, SuccessResultEffectWidget, delegate
			{
			}, delegate
			{
			});
			return;
		}
		UIHelper.EnableGameObject(FailEventItemRootTransform, enable: true);
		VespaCoroutine.Start(CoEnableBreakItemComponent());
		UIHelper.PlaySound("UI.SoulWeapon.EtherFailed");
		PlayEffect(FailReadyEffectWidget, FailResultEffectWidget, delegate
		{
			VespaCoroutine.Start(CoEnableBreakItemComponent());
		}, delegate
		{
		});
	}

	private void PlayEffect(UIGame3DWidget readyEffectWidget, UIGame3DWidget resultEffectWidget, Action endReadyAction = null, Action endResultAction = null)
	{
		if (readyEffectWidget == null)
		{
			return;
		}
		readyEffectWidget.Play(delegate
		{
			readyEffectWidget.Stop(immediatelyClear: false);
			if (endReadyAction != null)
			{
				Vespa_System_Action_Extension_Method.fire(endReadyAction);
			}
			if (!(resultEffectWidget == null))
			{
				resultEffectWidget.Play(delegate
				{
					resultEffectWidget.Stop(immediatelyClear: false);
					if (endResultAction != null)
					{
						Vespa_System_Action_Extension_Method.fire(endResultAction);
					}
				});
			}
		});
	}

	private IEnumerator CoEnableBreakItemComponent()
	{
		yield return new WaitForSeconds(WaitSecondsEnableFailEventItem);
		UIHelper.EnableGameObject(FailEventItemRootTransform, enable: false);
		UIHelper.EnableGameObject(EmptyIconTransform, enable: true);
	}

	public void OnClick()
	{
		if (_isShowResult)
		{
			if (IsSuccess)
			{
				Close();
			}
			else if (_info.HasValue && _info.HasValue && _info.Value.FailResultAction != null)
			{
				Vespa_System_Action_Extension_Method.fire(_info.Value.FailResultAction);
			}
		}
		else
		{
			ShowResult();
		}
	}
}
