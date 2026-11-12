using System;
using NShared;
using NShared.EventForge;
using NVespa.NGlobal;

namespace NGame2.NUI.NWindow.NEventForge;

public class EventForgeBreakConfirmPopup : BaseWindow
{
	public interface IListener
	{
		void ProgressReinforceItem(int slotIndex, EventForgeData eventForgeData, Action<NShared.EventForge.Response> resultAction);

		void ProgressCompleteSuccessReinforceItem(int slotIndex, EventForgeData eventForgeData, Action<NShared.EventForge.Response> resultAction);
	}

	public struct Info
	{
		public IListener IListener { get; set; }

		public int SlotIndex { get; set; }

		public EventForgeData EventForgeData { get; set; }

		public Action<NShared.EventForge.Response> ResultAction { get; set; }

		public Func<int, int, bool> CheckEnoughCompleteSuccessMaterialCountFunc { get; set; }
	}

	public const string PrefabPath = "Resources/Prefab2/Window/EventForge/EventForgeMessagePopup";

	public UILabel BreakMessageLabel;

	public ReinforcementMaterialItemInfo EventMaterialItemInfo;

	public UIButton CompleteSuccessReinforceButton;

	private Info? _info;

	private EventForgeData NextEventForgeData
	{
		get
		{
			if (!_info.HasValue || !_info.HasValue)
			{
				return null;
			}
			EventForgeData eventForgeData = _info.Value.EventForgeData;
			if (eventForgeData == null)
			{
				return null;
			}
			return GameRoot.Instance.EventForgeDataContainer.GetDataByLevel(eventForgeData.EnchatItemGroup, eventForgeData.EnchantLevel + 1);
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
		SetBreakMessageLabel();
		InitCompleteSuccessMaterialItemComponent();
		SetButtonState();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		EventMaterialItemInfo.Release();
	}

	public override void ReOpen()
	{
		base.ReOpen();
		SetButtonState();
	}

	private void SetBreakMessageLabel()
	{
		EventForgeData nextEventForgeData = NextEventForgeData;
		if (nextEventForgeData != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect("EVENT_FORGE_BREAK_POPUP", nextEventForgeData.BreakPercentage);
			UIHelper.SetLabelText(BreakMessageLabel, stringDirect);
		}
	}

	private void InitCompleteSuccessMaterialItemComponent()
	{
		EventForgeData nextEventForgeData = NextEventForgeData;
		if (nextEventForgeData != null)
		{
			EventMaterialItemInfo.Init(new ReinforcementMaterialItemInfo.Info
			{
				GroupIndex = nextEventForgeData.EnchatItemGroup,
				ReinforcementLevel = nextEventForgeData.EnchantLevel,
				IsCompleteSuccessReinforcement = true,
				IsChangeItem = true
			});
		}
	}

	private void SetButtonState()
	{
		UIHelper.EnableButton(CompleteSuccessReinforceButton, enable: false, grayscaleIfDisable: true);
		if (_info.HasValue && _info.HasValue && _info.Value.CheckEnoughCompleteSuccessMaterialCountFunc != null)
		{
			EventForgeData nextEventForgeData = NextEventForgeData;
			if (nextEventForgeData != null)
			{
				UIHelper.EnableButton(CompleteSuccessReinforceButton, _info.Value.CheckEnoughCompleteSuccessMaterialCountFunc(nextEventForgeData.EnchantLevel, nextEventForgeData.EnchatItemGroup), grayscaleIfDisable: true);
			}
		}
	}

	public void OnClickProgressReinforcement()
	{
		if (_info.HasValue && _info.HasValue && _info.Value.IListener != null)
		{
			_info.Value.IListener.ProgressReinforceItem(_info.Value.SlotIndex, _info.Value.EventForgeData, _info.Value.ResultAction);
		}
	}

	public void OnClickCompleteSuccessReinforcement()
	{
		if (_info.HasValue && _info.HasValue && _info.Value.IListener != null)
		{
			_info.Value.IListener.ProgressCompleteSuccessReinforceItem(_info.Value.SlotIndex, _info.Value.EventForgeData, _info.Value.ResultAction);
		}
	}
}
