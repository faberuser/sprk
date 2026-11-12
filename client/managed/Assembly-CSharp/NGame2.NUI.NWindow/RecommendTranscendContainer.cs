using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class RecommendTranscendContainer : BaseWindow, HeroTranscendSkillListPanel.Listener
{
	public enum ToggleState
	{
		NONE = -1,
		CHAPTER,
		RAID,
		PVP
	}

	[Serializable]
	public class RecommendData
	{
		public UIButton RecommendButton;

		public UIToggle RecommendToggle;

		public string LocalKey;

		[HideInInspector]
		public List<int> TranscendRecommendIndexs;
	}

	public interface Listener
	{
		void LearnRecommendTranscend(List<int> index, int page);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/RecommendTranscend/RecommendTranscendContainer";

	public const int RecommendMaxCount = 3;

	public UIButton LearnRecommendButton;

	public UIGrid RecommendGrid;

	public UITable SkillGroupTable;

	public List<RecommendData> _recommendDatas;

	public List<int> CurrRecommendIndex;

	public List<int> CurrHeroTranscendIndex;

	public List<int> PossibleTranscendIndex;

	public string CurrTapLocalKey = "";

	private HeroInfo _heroInfo;

	private ToggleState _currtoggleState = ToggleState.NONE;

	private List<HeroTranscendSkillGroupComponent> _ListTranscendSkillGroup = new List<HeroTranscendSkillGroupComponent>();

	private Listener _listener;

	private int _currPage;

	private int _maxPage;

	public void Open(HeroInfo heroInfo, int page, Listener listener)
	{
		_heroInfo = heroInfo;
		_currPage = page;
		_maxPage = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxTranscendHeroSkillPage");
		_listener = listener;
		InitPage();
	}

	private void InitPage()
	{
		CreatureData creatureData = _heroInfo.CreatureData;
		if (creatureData == null || !creatureData.IsValidRecommendTranscendData())
		{
			return;
		}
		FactoryBase.ReleaseAndClearList(_ListTranscendSkillGroup);
		base.Open();
		_currtoggleState = ToggleState.NONE;
		CurrRecommendIndex = null;
		Vespa_Generic_ICollection_Extension_Method.CLEAR(CurrHeroTranscendIndex);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(creatureData.TranscendRecommend1))
		{
			_recommendDatas[0].TranscendRecommendIndexs = creatureData.TranscendRecommend1.ToList();
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(creatureData.TranscendRecommend2))
		{
			_recommendDatas[1].TranscendRecommendIndexs = creatureData.TranscendRecommend2.ToList();
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(creatureData.TranscendRecommend3))
		{
			_recommendDatas[2].TranscendRecommendIndexs = creatureData.TranscendRecommend3.ToList();
		}
		int? num = null;
		for (int num2 = __Extension_Method_ICollection__.COUNT(_recommendDatas) - 1; num2 >= 0; num2--)
		{
			UIButton recommendButton = _recommendDatas[num2].RecommendButton;
			UIToggle recommendToggle = _recommendDatas[num2].RecommendToggle;
			List<int> transcendRecommendIndexs = _recommendDatas[num2].TranscendRecommendIndexs;
			recommendToggle.startsActive = false;
			recommendToggle.value = false;
			UIHelper.EnableGameObject(recommendButton, !__Extension_Method_ICollection__.IsNullOrEmpty(transcendRecommendIndexs));
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(transcendRecommendIndexs))
			{
				num = num2;
			}
		}
		RecommendGrid.Reposition();
		if (num.HasValue)
		{
			_recommendDatas[num.Value].RecommendToggle.value = true;
			ToggleRecommend();
		}
	}

	public void ToggleRecommend(bool isForce = false)
	{
		int num = _recommendDatas.FindIndex((RecommendData x) => x.RecommendToggle.value && !__Extension_Method_ICollection__.IsNullOrEmpty(x.TranscendRecommendIndexs));
		if ((num == (int)_currtoggleState || num < 0) && !isForce)
		{
			return;
		}
		_currtoggleState = (ToggleState)num;
		FactoryBase.ReleaseAndClearList(_ListTranscendSkillGroup);
		for (int num2 = 1; num2 <= 5; num2++)
		{
			if (__Extension_Method_ICollection__.COUNT(_heroInfo.CreatureData.GetTranscendSkill(num2)) != 0)
			{
				HeroTranscendSkillGroupComponent heroTranscendSkillGroupComponent = HeroTranscendSkillGroupComponent.PrefabAuto.Factory.Create(SkillGroupTable.transform);
				heroTranscendSkillGroupComponent.OpenForRecommend(_heroInfo, num2, _recommendDatas[num].TranscendRecommendIndexs);
				_ListTranscendSkillGroup.Add(heroTranscendSkillGroupComponent);
				CurrRecommendIndex = _recommendDatas[num].TranscendRecommendIndexs;
				CurrTapLocalKey = _recommendDatas[num].LocalKey;
			}
		}
		SkillGroupTable.Reposition();
		CompareRecommendTranscend();
	}

	public void ChangePageIndex(int pageIndex)
	{
		if (pageIndex <= _maxPage && _currPage != pageIndex)
		{
			_currPage = pageIndex;
			CompareRecommendTranscend();
		}
	}

	private void CompareRecommendTranscend()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(CurrRecommendIndex))
		{
			return;
		}
		if (_heroInfo.Transcended == 0)
		{
			UIHelper.EnableButton(LearnRecommendButton, enable: false);
			return;
		}
		Vespa_Generic_ICollection_Extension_Method.CLEAR(CurrHeroTranscendIndex);
		Vespa_Generic_ICollection_Extension_Method.CLEAR(PossibleTranscendIndex);
		TranscendSkillPageInfo transcendSkillPageInfo = _heroInfo.GetTranscendSkillPageInfo(_currPage);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(transcendSkillPageInfo.TranscendSkillDatas); i++)
		{
			int transcendSkillIndex = transcendSkillPageInfo.TranscendSkillDatas[i].TranscendSkillIndex;
			CurrHeroTranscendIndex.Add(transcendSkillIndex);
		}
		CreatureData creatureData = _heroInfo.CreatureData;
		for (int j = 0; j < CurrRecommendIndex.Count(); j++)
		{
			CreatureData.TranscendSkill transcendSkillData = creatureData.GetTranscendSkillData(CurrRecommendIndex[j]);
			if (transcendSkillData != null && _heroInfo.Transcended >= transcendSkillData.Transcended)
			{
				PossibleTranscendIndex.Add(transcendSkillData.TranscendSkillIndex);
			}
		}
		bool enable = true;
		if (PossibleTranscendIndex.Count() > 0)
		{
			if (__Extension_Method_ICollection__.COUNT(CurrHeroTranscendIndex) > 0)
			{
				IEnumerable<int> source = CurrRecommendIndex.Intersect(CurrHeroTranscendIndex);
				if (source.Count() > 0 && source.Count() == __Extension_Method_ICollection__.COUNT(CurrHeroTranscendIndex) && !_heroInfo.IsRemainLearnableTranscendSkill(_currPage))
				{
					enable = false;
				}
			}
			else if (__Extension_Method_ICollection__.COUNT(PossibleTranscendIndex) == 0)
			{
				enable = false;
			}
		}
		else
		{
			enable = false;
		}
		UIHelper.EnableButton(LearnRecommendButton, enable);
	}

	public override void Close()
	{
		FactoryBase.ReleaseAndClearList(_ListTranscendSkillGroup);
		HeroTranscendSkillListPanel heroTranscendSkillListPanel = SceneAutoBehaviour<MyHeroManagement>.instance.GetInstance<HeroList>().HeroTranscendSkillListPanel;
		if (heroTranscendSkillListPanel != null)
		{
			heroTranscendSkillListPanel.RecommendTranscendToggle.value = false;
		}
		for (int i = 0; i < _recommendDatas.Count; i++)
		{
			_recommendDatas[i].RecommendToggle.Set(state: false);
			_recommendDatas[i].TranscendRecommendIndexs = null;
		}
		base.Close();
	}

	public void RecommendTranscend(HeroInfo heroInfo, int page)
	{
	}

	public void OnClickClose()
	{
	}

	public void OnClickLearnRecommendTranscend()
	{
		if (_listener != null && __Extension_Method_ICollection__.COUNT(PossibleTranscendIndex) != 0)
		{
			_listener.LearnRecommendTranscend(PossibleTranscendIndex, _currPage);
		}
	}

	public void AddTranscendPreset(int page)
	{
	}

	public void ResetTranscendSkill(int page)
	{
	}

	public void SelectTranscendPreset(int heroIndex, int page)
	{
	}

	public void SelectTranscendSkill(int page, int GroupIndex, int SkillSlotIndex, int SkillIndex)
	{
	}
}
