using NGame2.NUI.NComponent2.NStat;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class ValanceTierUpgradeResultWindow : ValanceEnhanceResultBase
{
	public const string PrefabPath = "Resources/Prefab2/Window/Valance/ValanceTierUpgradeResultWindow";

	public UIPanel StatChangePanel;

	public UIScrollView StatChangeScrollView;

	private EquipItemInfo _oldEquipItemInfo;

	private EquipItemData _oldEquipItemData;

	private StatListComponent _statListComponent;

	public void Open(EquipItemInfo oldEquipItemInfo, EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			Close();
			return;
		}
		if (equipItemInfo.ItemData == null)
		{
			Close();
			return;
		}
		base.Open(delegate
		{
			base.EquipItemInfo = equipItemInfo;
			base.EquipItemData = equipItemInfo.EquipItemData;
			_oldEquipItemInfo = oldEquipItemInfo;
			if (oldEquipItemInfo != null)
			{
				_oldEquipItemData = oldEquipItemInfo.EquipItemData;
			}
			Init();
		}, delegate
		{
			UIHelper.EnableGameObject(HideRoot, enable: true);
		});
	}

	protected override void NodeTreeOpenOnEnd()
	{
		base.NodeTreeOpenOnEnd();
	}

	public override void Init()
	{
		base.Init();
		SetItemStat();
	}

	public void SetItemStat()
	{
		if (!(StatChangeScrollView == null) && !(StatChangePanel == null))
		{
			if (_statListComponent != null)
			{
				ClearStatComp();
			}
			_statListComponent = StatListComponent.Create(StatChangePanel, 2, isValance: true);
			StatData equipItemStatData = StatCalculator.GetEquipItemStatData(_oldEquipItemInfo.ItemIndex, _oldEquipItemInfo.Star, _oldEquipItemInfo.Level);
			StatData equipItemStatData2 = StatCalculator.GetEquipItemStatData(base.EquipItemInfo.ItemIndex, base.EquipItemInfo.Star, base.EquipItemInfo.Level);
			_statListComponent.AddStatChange(equipItemStatData, equipItemStatData2, all: false, StatListComponent.EStatChangeTemplate.ForStarUpgrade);
			StatListComponent.AddOptionStat(_statListComponent, base.EquipItemData, base.EquipItemInfo);
			if ((bool)StatChangeScrollView)
			{
				StatChangeScrollView.ResetPosition();
			}
		}
	}

	private void Release()
	{
		_oldEquipItemInfo = null;
		_oldEquipItemData = null;
		ClearStatComp();
	}

	public void ClearStatComp()
	{
		FactoryBase.SafeReleaseComponent(ref _statListComponent);
	}

	public override void OnClosing()
	{
		base.OnClosing();
		Release();
	}
}
