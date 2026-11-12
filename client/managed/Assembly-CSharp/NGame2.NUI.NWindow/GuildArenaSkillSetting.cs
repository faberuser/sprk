using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NGuild;
using NGame2.NUI.NComponent2;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildArenaSkillSetting : BaseWindow, GuildArenaSkillSelectComponent.Listener
{
	public interface Listener
	{
		void OnClickSaveSkillIndices(GuildArenaDeckInfo[] deckInfos);
	}

	private Listener _listener;

	public const string PrefabPath = "Resources/Prefab2/Window/GuildArena/GuildArenaSkillSetting";

	public UISimpleGrid Grid_SimpleGrid;

	public UIToggle Toggle_Deck1;

	public UIToggle Toggle_Deck2;

	public UIToggle Toggle_Deck3;

	public UIToggle Toggle_Deck4;

	public UIToggle Toggle_Deck5;

	private UIToggle _currentToggle;

	private int _slotCount;

	private List<GuildArenaSkillSelectComponent> _skillSelectComponentList = new List<GuildArenaSkillSelectComponent>();

	private GuildArenaDeckInfo[] _savedDeckInfos;

	private GuildArenaDeckInfo[] _originSavedDeckInfos;

	private bool _IsSaved = true;

	public override void Close()
	{
		if (!_IsSaved)
		{
			MessagePopup3Button.Open(LocalizationManager.GetStringDirect("COMMON_NOTIFICATION"), LocalizationManager.GetStringDirect("DEFENCE_SKILL_SETTING_NOT_APPLY"), delegate
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.SetSavedDeckInfos(_originSavedDeckInfos);
				_savedDeckInfos = null;
				base.Close();
			}, delegate
			{
				OnClickSaveSkillIndices();
			}, delegate
			{
			}, LocalizationManager.GetStringDirect("COMMON_EXIT"), LocalizationManager.GetStringDirect("COMMON_SAVE"));
		}
		else
		{
			base.Close();
		}
	}

	public void Open(Listener listener, Action onComplete = null)
	{
		_listener = listener;
		_slotCount = 3;
		_IsSaved = true;
		if (_currentToggle != null)
		{
			_currentToggle.value = false;
			_currentToggle.Set(state: false);
		}
		Toggle_Deck1.value = true;
		_savedDeckInfos = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetSavedDeckInfos();
		_originSavedDeckInfos = new GuildArenaDeckInfo[_savedDeckInfos.Length];
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_originSavedDeckInfos); i++)
		{
			_originSavedDeckInfos[i] = JsonMarshaler.Clone(_savedDeckInfos[i]);
		}
		Open(Init, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		});
	}

	private void Init()
	{
		InitToggles();
		InitSkillSelectComponents(0);
	}

	private void InitToggles()
	{
		int num = 3;
		int tierIndex = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GuildArenaManager.GetMyGuildInfo().TierIndex;
		GuildArenaTierData data = GameRoot.Instance.GuildArenaTierDataContainer.GetData(tierIndex);
		if (data != null)
		{
			num = data.DefenseDeckCount;
		}
		List<UIToggle> list = new List<UIToggle>();
		list.Add(Toggle_Deck1);
		list.Add(Toggle_Deck2);
		list.Add(Toggle_Deck3);
		list.Add(Toggle_Deck4);
		list.Add(Toggle_Deck5);
		for (int i = 0; i < list.Count; i++)
		{
			UIHelper.SetWidgetGrayScale(list[i].gameObject.GetComponent<UIWidget>(), i >= num, setColliders: true);
		}
	}

	private void ResetSkillSelectComponents()
	{
		FactoryBase.ReleaseAndClearList(_skillSelectComponentList);
	}

	private void InitSkillSelectComponents(int deckIndex)
	{
		ResetSkillSelectComponents();
		if (_savedDeckInfos == null)
		{
			return;
		}
		GuildArenaDeckInfo guildArenaDeckInfo = _savedDeckInfos[deckIndex];
		if (guildArenaDeckInfo == null)
		{
			return;
		}
		for (int i = 0; i < _slotCount; i++)
		{
			CachedHeroInfo cachedHeroInfo = null;
			int selectedSkillIndex = 0;
			if (guildArenaDeckInfo.HeroIndices != null && guildArenaDeckInfo.HeroIndices.Length >= guildArenaDeckInfo.CachedHeroInfos.Length)
			{
				if (guildArenaDeckInfo.HeroIndices.Length > i)
				{
					int num = guildArenaDeckInfo.HeroIndices[i];
					CachedHeroInfo[] cachedHeroInfos = guildArenaDeckInfo.CachedHeroInfos;
					foreach (CachedHeroInfo cachedHeroInfo2 in cachedHeroInfos)
					{
						if (cachedHeroInfo2.HeroIndex == num)
						{
							cachedHeroInfo = cachedHeroInfo2;
							break;
						}
					}
				}
			}
			else if (guildArenaDeckInfo.CachedHeroInfos != null && guildArenaDeckInfo.CachedHeroInfos.Length > i)
			{
				cachedHeroInfo = guildArenaDeckInfo.CachedHeroInfos[i];
			}
			if (guildArenaDeckInfo.SkillSlotIndices != null && guildArenaDeckInfo.SkillSlotIndices.Length > i)
			{
				selectedSkillIndex = guildArenaDeckInfo.SkillSlotIndices[i];
			}
			GuildArenaSkillSelectComponent guildArenaSkillSelectComponent = UIHelper.AddGridItem(Grid_SimpleGrid, GuildArenaSkillSelectComponent.PrefabAuto);
			guildArenaSkillSelectComponent.Init(this, deckIndex, i, cachedHeroInfo, selectedSkillIndex);
			_skillSelectComponentList.Add(guildArenaSkillSelectComponent);
		}
		UIHelper.RepositionGrid(Grid_SimpleGrid);
	}

	public void OnClickToggle()
	{
		int deckIndex = 0;
		if (Toggle_Deck1.value)
		{
			_currentToggle = Toggle_Deck1;
			deckIndex = 0;
		}
		if (Toggle_Deck2.value)
		{
			_currentToggle = Toggle_Deck2;
			deckIndex = 1;
		}
		if (Toggle_Deck3.value)
		{
			_currentToggle = Toggle_Deck3;
			deckIndex = 2;
		}
		if (Toggle_Deck4.value)
		{
			_currentToggle = Toggle_Deck4;
			deckIndex = 3;
		}
		if (Toggle_Deck5.value)
		{
			_currentToggle = Toggle_Deck5;
			deckIndex = 4;
		}
		InitSkillSelectComponents(deckIndex);
	}

	public void OnClickSaveSkillIndices()
	{
		if (_listener != null)
		{
			_listener.OnClickSaveSkillIndices(_savedDeckInfos);
			SoundStock stock = new SoundStock
			{
				Code = "UI2.Guild.GuildGuildSaveDeck"
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 10f);
		}
	}

	public void NotApplyAndClose()
	{
		base.Close();
	}

	public void SaveAndClose()
	{
		_IsSaved = true;
		VespaCoroutine.Start(this, coClose());
	}

	private IEnumerator coClose()
	{
		CenterMessage.Open(LocalizationManager.GetStringDirect("GUILD_CHANGE_APPLY_INFO"));
		yield return new WaitForSeconds(0.5f);
		Close();
	}

	void GuildArenaSkillSelectComponent.Listener.OnSelectSkillComponent(int deckIndex, int slotIndex, int skillSlotNum)
	{
		if (_savedDeckInfos.Length > deckIndex && _savedDeckInfos[deckIndex].SkillSlotIndices != null && _savedDeckInfos[deckIndex].SkillSlotIndices.Length > slotIndex)
		{
			_savedDeckInfos[deckIndex].SkillSlotIndices[slotIndex] = skillSlotNum;
			_IsSaved = false;
		}
	}
}
