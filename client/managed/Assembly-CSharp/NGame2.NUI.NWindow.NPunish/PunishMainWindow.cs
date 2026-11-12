using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NPunish;

public class PunishMainWindow : RenewResourceWindow
{
	public interface IListenenr
	{
		void OnClickEnter(PunishmentGroupData data);
	}

	public static string PrefabPath = "Resources/Prefab2/Window/Raid/PunishmentMainWindow";

	public UIButton PrevButton;

	public UIButton NextButton;

	public UILabel BossNameLabel;

	public UILabel BossDescLabel;

	public UISimpleGrid CreatureWidgetGrid;

	public int CreatureWidgetCompDepth = 2;

	private string _beforeBGM = string.Empty;

	private Dictionary<int, CreatureWidgetComponent> _creatureWidgetComponentDic = new Dictionary<int, CreatureWidgetComponent>();

	private CreatureWidgetComponent _currentCreatureWidget;

	private IList<PunishmentGroupData> _listBoss;

	private int _curBossIndex;

	private IListenenr _listener;

	private int _forceGroupIndex;

	public void Open(IListenenr listener, int forceGroupIndex = 0)
	{
		_listener = listener;
		_forceGroupIndex = forceGroupIndex;
		Open(Set, null);
		PlayBGM();
	}

	public override void ReOpen()
	{
		base.ReOpen();
		SelectCreatureWidget(_curBossIndex);
	}

	public override void OnOtherWindowOpened()
	{
		base.OnOtherWindowOpened();
		HideAllCreatureWidget();
	}

	public override void OnClosing()
	{
		base.OnClosing();
		_listener = null;
		_curBossIndex = 0;
		_listener = null;
		RelaseCreatureWidgetComp();
		SingletonAuto<BgmManager2>.instance.StopEvent("Raid.PunishmentRaid.SelectLobby");
		if (_beforeBGM != "Raid.PunishmentRaid.SelectLobby")
		{
			SingletonAuto<BgmManager2>.instance.Request(_beforeBGM, isEvent: true);
		}
	}

	public void PlayBGM()
	{
		SingletonAuto<BgmManager2>.instance.PendingRequest = false;
		_beforeBGM = SingletonAuto<BgmManager2>.instance.GetCurrentEventAssetBgmCode();
		if (_beforeBGM != "Raid.PunishmentRaid.SelectLobby")
		{
			SingletonAuto<BgmManager2>.instance.Request("Raid.PunishmentRaid.SelectLobby", isEvent: true);
		}
	}

	public void Set()
	{
		Init();
		SetDatatList();
		SetBossCreatureList();
		if (_forceGroupIndex != 0)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listBoss); i++)
			{
				PunishmentGroupData punishmentGroupData = _listBoss[i];
				if (punishmentGroupData != null && punishmentGroupData.GroupIndex == _forceGroupIndex)
				{
					_curBossIndex = i;
					break;
				}
			}
			_forceGroupIndex = 0;
		}
		SetBoss(_curBossIndex);
	}

	public void SetDatatList()
	{
		_listBoss = GameRoot.Instance.PunishmentGroupDataContainer.GetBossPunishmentDataList();
	}

	public void SetBossCreatureList()
	{
		RelaseCreatureWidgetComp();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_listBoss))
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listBoss); i++)
		{
			PunishmentGroupData punishmentGroupData = _listBoss[i];
			if (punishmentGroupData != null && punishmentGroupData.CreatureIndex != 0)
			{
				CreatureWidgetComponent creatureWidgetComponent = UIHelper.AddGridItem(CreatureWidgetGrid, CreatureWidgetComponent.PrefabAuto);
				if (!(creatureWidgetComponent == null))
				{
					creatureWidgetComponent.Open(punishmentGroupData.CreatureIndex, punishmentGroupData.CreaturePosition, punishmentGroupData.CreatureRotation, punishmentGroupData.CreatureScale);
					creatureWidgetComponent.SetDepth(CreatureWidgetCompDepth);
					creatureWidgetComponent.SetCustomIdle(DefineCreature.EMotion.NamedAction, "IdleUI");
					_creatureWidgetComponentDic.Add(punishmentGroupData.CreatureIndex, creatureWidgetComponent);
				}
			}
		}
	}

	public void SetBoss(int index)
	{
		PunishmentGroupData data = GetData(index);
		if (data != null)
		{
			_curBossIndex = index;
			CreatureData data2 = GameRoot.Instance.CreatureDataContainer.GetData(data.CreatureIndex);
			if (data2 != null)
			{
				UIHelper.SetLabelTextLocalizeDirect(BossNameLabel, data2.Description);
				UIHelper.SetLabelTextLocalizeDirect(BossDescLabel, data2.Name);
				UIHelper.EnableButton(PrevButton, _curBossIndex > 0, grayscaleIfDisable: true);
				UIHelper.EnableButton(NextButton, _curBossIndex < __Extension_Method_ICollection__.COUNT(_listBoss) - 1, grayscaleIfDisable: true);
				SelectCreatureWidget(index);
			}
		}
	}

	private PunishmentGroupData GetData(int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_listBoss, index))
		{
			return null;
		}
		return _listBoss[index];
	}

	private void RelaseCreatureWidgetComp()
	{
		foreach (KeyValuePair<int, CreatureWidgetComponent> item in _creatureWidgetComponentDic)
		{
			CreatureWidgetComponent value = item.Value;
			if (!(value == null))
			{
				FactoryBase.ReleaseComponent(value);
			}
		}
		_creatureWidgetComponentDic.Clear();
	}

	private void SelectCreatureWidget(int index)
	{
		PunishmentGroupData data = GetData(index);
		if (data != null)
		{
			if (_currentCreatureWidget != null)
			{
				_currentCreatureWidget.SetAlpha(0f);
			}
			if (_creatureWidgetComponentDic.TryGetValue(data.CreatureIndex, out _currentCreatureWidget))
			{
				_currentCreatureWidget.SetAlpha(1f);
			}
		}
	}

	private void HideAllCreatureWidget()
	{
		foreach (KeyValuePair<int, CreatureWidgetComponent> item in _creatureWidgetComponentDic)
		{
			CreatureWidgetComponent value = item.Value;
			if (!(value == null))
			{
				value.SetAlpha(0f);
			}
		}
	}

	public void OnClickPrev()
	{
		if (_curBossIndex - 1 >= 0)
		{
			SetBoss(_curBossIndex - 1);
		}
	}

	public void OnClickNext()
	{
		if (_curBossIndex + 1 < __Extension_Method_ICollection__.COUNT(_listBoss))
		{
			SetBoss(_curBossIndex + 1);
		}
	}

	public void OnClickEnter()
	{
		if (_listener != null)
		{
			PunishmentGroupData data = GetData(_curBossIndex);
			if (data != null)
			{
				_listener.OnClickEnter(data);
			}
		}
	}
}
