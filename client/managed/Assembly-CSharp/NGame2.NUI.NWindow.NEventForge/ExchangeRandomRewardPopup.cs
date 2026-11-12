using System;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NEventForge;

public class ExchangeRandomRewardPopup : BaseWindow
{
	public interface IListener
	{
		void ExchangeRandomReward(int slotIndex, Action resultAction);
	}

	public struct Info
	{
		public IListener IListener { get; set; }

		public int SlotIndex { get; set; }

		public EventForgeData EventForgeData { get; set; }

		public Action ResultAction { get; set; }
	}

	public const string PrefabPath = "Resources/Prefab2/Window/EventForge/EventForgeItemChangeMessagePopup";

	public Transform EventItemRootTransform;

	public GetRewardListInfo RewardListInfo;

	public UILabel RewardInformLabel;

	private Info? _info;

	private ItemIconComponent _eventItemIconComponent;

	public void Open(Info info)
	{
		Open(delegate
		{
			init(info);
		}, null);
	}

	private void init(Info? info)
	{
		if (!info.HasValue || !info.HasValue)
		{
			Close();
			return;
		}
		_info = info;
		InitEventItemIconComponent();
		initRewardListInfo();
		SetRewardInformLabel();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		ReleaseEventItemIconComponent();
		RewardListInfo.Release();
	}

	private void ReleaseEventItemIconComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _eventItemIconComponent);
	}

	private void InitEventItemIconComponent()
	{
		ReleaseEventItemIconComponent();
		if (!_info.HasValue || !_info.HasValue)
		{
			return;
		}
		EventForgeData eventForgeData = _info.Value.EventForgeData;
		if (eventForgeData == null)
		{
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(eventForgeData.ItemCode);
		if (dataByCode != null && !(EventItemRootTransform == null))
		{
			_eventItemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(EventItemRootTransform);
			if (!(_eventItemIconComponent == null))
			{
				_eventItemIconComponent.Open(dataByCode);
				_eventItemIconComponent.ResetPosition();
			}
		}
	}

	private void initRewardListInfo()
	{
		if (_info.HasValue && _info.HasValue)
		{
			EventForgeData eventForgeData = _info.Value.EventForgeData;
			if (eventForgeData != null)
			{
				RewardListInfo.Init(new GetRewardListInfo.Info
				{
					Group = eventForgeData.EnchatItemGroup,
					ReinforcementLevel = eventForgeData.EnchantLevel
				});
				RewardListInfo.SetInfo(eventForgeData.EnchantLevel, eventForgeData.MaxEnchantLevel);
			}
		}
	}

	private void SetRewardInformLabel()
	{
		string stringDirect = LocalizationManager.GetStringDirect((RewardListInfo.RewardCount > 1) ? "EVENT_FORGE_MGS_REWARD" : "EVENT_FORGE_MGS_REWARD_ONE");
		UIHelper.SetLabelText(RewardInformLabel, stringDirect);
	}

	public void OnClickExchangeRandomReward()
	{
		if (_info.HasValue && _info.HasValue && _info.Value.IListener != null)
		{
			_info.Value.IListener.ExchangeRandomReward(_info.Value.SlotIndex, _info.Value.ResultAction);
		}
	}
}
