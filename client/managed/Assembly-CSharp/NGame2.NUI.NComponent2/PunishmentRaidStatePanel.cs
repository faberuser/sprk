using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class PunishmentRaidStatePanel : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Raid/PunishmentRaidStatePanel";

	public static PrefabAuto<PunishmentRaidStatePanel> PrefabAuto = new PrefabAuto<PunishmentRaidStatePanel>("Prefab2/Component/Raid/PunishmentRaidStatePanel");

	public UIWidget Root;

	public UILabel TitleLabel;

	public UISprite Sprite_BG;

	public UIBaseGrid AffixStatusGrid;

	private List<PunishmentRaidStateComponent> _compList = new List<PunishmentRaidStateComponent>();

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ClearComp();
	}

	public void Open(int raidIndex, int raidLevel, GameObject rootGo)
	{
		SetRootAnchor(rootGo);
		List<PunishmentRaidTriggerData> triggerDatas = GetTriggerDatas(raidIndex, raidLevel);
		if (triggerDatas != null)
		{
			SetComp(triggerDatas);
		}
	}

	private List<PunishmentRaidTriggerData> GetTriggerDatas(int raidIndex, int raidLevel)
	{
		Debug.Log("환경스킬 여는 중! raid: " + raidIndex + ", level: " + raidLevel);
		PunishmentRaidData data = GameRoot.Instance.PunishmentRaidDataContainer.GetData(raidIndex, raidLevel);
		if (data == null)
		{
			return null;
		}
		int[] triggerIndices = data.TriggerIndices;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(triggerIndices))
		{
			return null;
		}
		return GameRoot.Instance.PunishmentRaidTriggerDataContainer.GetPunishmentRaidTriggerDatas(triggerIndices);
	}

	public void SetRootAnchor(GameObject rootGo)
	{
		Root.SetAnchor(rootGo, 0, 0, 0, 0);
		Root.ForceUpdateAnchors(includeChildren: true);
	}

	public void SetComp(List<PunishmentRaidTriggerData> triggerDatas)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(triggerDatas))
		{
			return;
		}
		foreach (PunishmentRaidTriggerData triggerData in triggerDatas)
		{
			PunishmentRaidStateComponent punishmentRaidStateComponent = UIHelper.AddGridItem(AffixStatusGrid, PunishmentRaidStateComponent.PrefabAuto);
			if (!(punishmentRaidStateComponent == null))
			{
				punishmentRaidStateComponent.Open(triggerData, null);
				_compList.Add(punishmentRaidStateComponent);
			}
		}
		UIHelper.RepositionGrid(AffixStatusGrid);
	}

	private void ClearComp()
	{
		FactoryBase.ReleaseAndClearList(_compList);
	}

	public void EnableTitleLabel(bool isEnabled)
	{
		UIHelper.EnableGameObject(TitleLabel, isEnabled);
	}

	public void Refresh()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_compList); i++)
		{
			PunishmentRaidStateComponent punishmentRaidStateComponent = _compList[i];
			if (!(punishmentRaidStateComponent == null))
			{
				punishmentRaidStateComponent.SetStatus();
			}
		}
	}
}
