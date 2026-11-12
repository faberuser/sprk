using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NEventForge;

[Serializable]
public class ReinforcementMaterialItemInfo : ItemIconComponent.Listener
{
	public struct Info
	{
		public int GroupIndex { get; set; }

		public int ReinforcementLevel { get; set; }

		public bool IsCompleteSuccessReinforcement { get; set; }

		public bool IsChangeItem { get; set; }
	}

	public Transform ItemIconCompRootTranform;

	public UILabel RequireItemCountLabel;

	public UILabel MyItemCountLabel;

	public Color DefaultMyCountLabelColor;

	public Color DefaultRequireCountLabelColor;

	private Info? _info;

	private ItemIconComponent _ItemIconComponet;

	private EventForgeData EventForgeData
	{
		get
		{
			if (!_info.HasValue || !_info.HasValue)
			{
				return null;
			}
			return GameRoot.Instance.EventForgeDataContainer.GetDataByLevel(_info.Value.GroupIndex, _info.Value.ReinforcementLevel);
		}
	}

	private ItemData ReinforcementMaterialItemData
	{
		get
		{
			if (!_info.HasValue || !_info.HasValue)
			{
				return null;
			}
			EventForgeData eventForgeData = EventForgeData;
			if (EventForgeData == null)
			{
				return null;
			}
			string code = (_info.Value.IsCompleteSuccessReinforcement ? eventForgeData.SuccessfulItemCode : eventForgeData.RequireItem);
			return GameRoot.Instance.ItemDataContainer.GetDataByCode(code);
		}
	}

	private int RequireItemCount
	{
		get
		{
			if (!_info.HasValue || !_info.HasValue)
			{
				return -1;
			}
			EventForgeData eventForgeData = EventForgeData;
			if (EventForgeData == null)
			{
				return -1;
			}
			if (!_info.Value.IsCompleteSuccessReinforcement)
			{
				return eventForgeData.RequireItemCount;
			}
			return eventForgeData.SuccessfulItemCount;
		}
	}

	private string RequireItemCountStr
	{
		get
		{
			if (!_info.HasValue || !_info.HasValue)
			{
				return "-";
			}
			_ = EventForgeData;
			if (EventForgeData == null)
			{
				return "-";
			}
			return "/" + RequireItemCount;
		}
	}

	private int MyItemCount
	{
		get
		{
			ItemData reinforcementMaterialItemData = ReinforcementMaterialItemData;
			if (reinforcementMaterialItemData == null)
			{
				return 0;
			}
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				return 0;
			}
			return GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemCount(reinforcementMaterialItemData.Index);
		}
	}

	private string MyItemCountStr
	{
		get
		{
			if (ReinforcementMaterialItemData == null)
			{
				return string.Empty;
			}
			if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
			{
				return string.Empty;
			}
			return MyItemCount.ToString();
		}
	}

	public void Init(Info info)
	{
		_info = info;
		if (_info.HasValue)
		{
			SetItemIconComponent();
			SetRequireItemCount();
			SetMyItemCount();
		}
	}

	public void Release()
	{
		ReleaseItemIconComponent();
	}

	private void ReleaseItemIconComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _ItemIconComponet);
	}

	private void SetItemIconComponent()
	{
		if (!_info.HasValue || !_info.HasValue || !_info.Value.IsChangeItem)
		{
			return;
		}
		ReleaseItemIconComponent();
		ItemData reinforcementMaterialItemData = ReinforcementMaterialItemData;
		if (reinforcementMaterialItemData != null && !(ItemIconCompRootTranform == null))
		{
			_ItemIconComponet = ItemIconComponent.PrefabAuto.Factory.Create(ItemIconCompRootTranform);
			if (!(_ItemIconComponet == null))
			{
				_ItemIconComponet.Open(reinforcementMaterialItemData, this);
				_ItemIconComponet.ResetPosition();
			}
		}
	}

	private void SetRequireItemCount()
	{
		string text = ColorUtility.ToHtmlStringRGB(DefaultRequireCountLabelColor);
		UIHelper.SetLabelText(RequireItemCountLabel, "[c][" + text + "]" + RequireItemCountStr + "[-][/c]");
	}

	private void SetMyItemCount()
	{
		int requireItemCount = RequireItemCount;
		string text = ColorUtility.ToHtmlStringRGB((MyItemCount >= requireItemCount) ? DefaultMyCountLabelColor : Color.red);
		UIHelper.SetLabelText(MyItemCountLabel, "[c][" + text + "]" + MyItemCountStr + "[-][/c]");
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		if (!(itemIconComponent == null) && _info.HasValue)
		{
			_ = _info.HasValue;
		}
	}
}
