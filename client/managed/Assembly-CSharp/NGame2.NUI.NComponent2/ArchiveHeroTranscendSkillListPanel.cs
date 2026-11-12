using System.Collections.Generic;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ArchiveHeroTranscendSkillListPanel : MonoBehaviour, IHeroPanel, IArchiveHeroSubPanel, HeroTranscendSkillGroupComponent.Listener, ArchiveTranscendDescComponent.Listener
{
	public interface Listener
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Hero/ArchiveHeroTranscendSkillListPanel";

	public static PrefabAuto<HeroStatPanel> PrefabAuto = new PrefabAuto<HeroStatPanel>("Resources/Prefab2/Component/Hero/ArchiveHeroTranscendSkillListPanel");

	public UITable SkillGroupTable;

	public UILabel TranscendSkillPointLabel;

	private Listener _listener;

	private bool _isMine;

	private List<HeroTranscendSkillGroupComponent> _ListTranscendSkillGroup = new List<HeroTranscendSkillGroupComponent>();

	public HeroInfo HeroInfo { get; private set; }

	public void Close()
	{
		base.gameObject.SetActive(value: false);
	}

	private void Reset()
	{
		ClearGroupList();
	}

	private void ClearGroupList()
	{
		FactoryBase.ReleaseAndClearList(_ListTranscendSkillGroup);
	}

	public void Open(HeroInfo heroInfo, Listener listener, bool isMine = true)
	{
		Reset();
		_isMine = isMine;
		base.gameObject.SetActive(value: true);
		HeroInfo = heroInfo;
		_listener = listener;
		SetupTranscendSkillGroupList();
	}

	private void SetupTranscendSkillGroupList()
	{
		ClearGroupList();
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("TotalTranscendSkill");
		int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxTranscend");
		if (valueInt2 == 0 || valueInt == 0)
		{
			return;
		}
		int num = Mathf.Min(valueInt2, valueInt);
		for (int i = 1; i <= num; i++)
		{
			if (__Extension_Method_ICollection__.COUNT(HeroInfo.CreatureData.GetTranscendSkill(i)) != 0)
			{
				HeroTranscendSkillGroupComponent heroTranscendSkillGroupComponent = HeroTranscendSkillGroupComponent.PrefabAuto.Factory.Create(SkillGroupTable.transform);
				heroTranscendSkillGroupComponent.OpenForArchive(HeroInfo, i, this);
				_ListTranscendSkillGroup.Add(heroTranscendSkillGroupComponent);
			}
		}
		SkillGroupTable.Reposition();
	}

	public void Enable(bool enable, CachedHeroInfo cachedHeroInfo, IArchiveHeroSubPanelListener listener = null)
	{
		if (enable)
		{
			Open(cachedHeroInfo, null);
		}
		else
		{
			Close();
		}
	}

	public void Change(CachedHeroInfo cachedHeroInfo, IArchiveHeroSubPanelListener listener = null)
	{
		Open(cachedHeroInfo, null);
	}

	public void OnSelectComponent(int transcended, int skillSlot, int skillIndex)
	{
	}
}
