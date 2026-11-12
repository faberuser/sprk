using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPunish;

public class ShardSelectWidget : MonoBehaviour
{
	public interface Listener
	{
		void OnClickShardSelect();
	}

	public UILabel Label_ShardName;

	public UILabel Label_ShardCount;

	public Transform Transform_ItemIcon;

	public UILabel Label_Select;

	private PunishmentRaidPartySetting.IDataProvider _dataProvider;

	private Listener _listener;

	private EPunishmentDungeonType _punishmentDungeonType = EPunishmentDungeonType.Karma;

	private ItemIconComponent _selectItemIcon;

	public void Init(Listener listener, PunishmentRaidPartySetting.IDataProvider dataProvider)
	{
		if (listener != null && dataProvider != null)
		{
			_listener = listener;
			_dataProvider = dataProvider;
			SetShardUI();
		}
	}

	public void Reset()
	{
		_listener = null;
		_dataProvider = null;
		_punishmentDungeonType = EPunishmentDungeonType.Karma;
		ClearSelectItemIcon();
	}

	public void SetShardUI()
	{
		ClearSelectItemIcon();
		int shardItemIndex = _dataProvider.GetShardItemIndex();
		int shardItemCount = _dataProvider.GetShardItemCount();
		bool num = shardItemIndex != 0;
		bool flag = true;
		PunishmentGroupData punishmentGroupData = _dataProvider.GetPunishmentGroupData();
		if (punishmentGroupData != null)
		{
			_punishmentDungeonType = punishmentGroupData.PunishmentDungeonType;
		}
		UIHelper.SetLabelText(text: (_punishmentDungeonType == EPunishmentDungeonType.Karma) ? LocalizationManager.GetStringDirect("BUTTON_SHARD_FLASK_SELECT") : LocalizationManager.GetStringDirect("BUTTON_SHARD_ITEM_SELECT"), label: Label_Select);
		if (_punishmentDungeonType == EPunishmentDungeonType.Karma)
		{
			ShardFlaskItemData data = GameRoot.Instance.ShardFlaskItemDataContainer.GetData(shardItemIndex);
			if (data != null && data.IsDefault)
			{
				flag = false;
			}
		}
		UIHelper.EnableGameObject(Label_ShardCount, flag);
		if (!num)
		{
			UIHelper.SetLabelText(Label_ShardName, "-");
			if (flag)
			{
				UIHelper.SetLabelText(Label_ShardCount, "-");
			}
			return;
		}
		ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(shardItemIndex);
		SetSelectItemIcon(data2);
		if (!flag)
		{
			return;
		}
		ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(shardItemIndex);
		if (itemInfo != null && itemInfo.Count > 0)
		{
			if (_punishmentDungeonType == EPunishmentDungeonType.Boss)
			{
				UIHelper.SetLabelText(Label_ShardCount, $"{shardItemCount}/{itemInfo.Count}");
			}
			else
			{
				UIHelper.SetLabelText(Label_ShardCount, itemInfo.Count);
			}
		}
		else
		{
			UIHelper.SetLabelText(Label_ShardCount, "-");
		}
	}

	private void SetSelectItemIcon(ItemData itemData)
	{
		if (itemData != null)
		{
			_selectItemIcon = ItemIconComponent.PrefabAuto.Factory.Create(Transform_ItemIcon);
			_selectItemIcon.Open(itemData);
			_selectItemIcon.ResetPosition();
			UIHelper.SetLabelText(Label_ShardName, itemData.Name);
		}
	}

	private void ClearSelectItemIcon()
	{
		if (!(_selectItemIcon == null))
		{
			FactoryBase.SafeReleaseComponent(ref _selectItemIcon);
			_selectItemIcon = null;
			UIHelper.SetLabelText(Label_ShardName, string.Empty);
		}
	}

	public void OnClickShardSelect()
	{
		if (_listener != null)
		{
			_listener.OnClickShardSelect();
		}
	}
}
