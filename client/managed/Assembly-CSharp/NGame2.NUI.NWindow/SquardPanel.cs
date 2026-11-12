using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[Serializable]
public class SquardPanel
{
	public GameObject Root;

	public UIPanel PanelSquard;

	public UIButton ButtonSquardChange;

	public PartySlotComponent EmptySlotTemplate;

	public UIGrid EmptySlotGrid;

	public HeroPortraitComponent.PrefabSelector HeroPortraitSelector = new HeroPortraitComponent.PrefabSelector(HeroPortraitComponent.PrefabSelector.EPrefabType.Short);

	public HeroPortraitComponent.EPortraitMode PortraitMode;

	public Vector3 HeroPortraitSize = new Vector3(1f, 1f, 1f);

	public GameObject DisableRoot;

	public GameObject EnableEffectRoot;

	private List<PartySlotComponent> _listEmptySlotComponents = new List<PartySlotComponent>();

	private List<Common.HeroPortraitAnim> _listHeroPortraitAnim = new List<Common.HeroPortraitAnim>();

	private List<Common.HeroPortraitAnim> _listDiappearHeroPortraitAnim = new List<Common.HeroPortraitAnim>();

	private HeroPortraitComponent.Listener _heroPortraitComponentListener;

	public void SetSquardPanel(int squardSlotCount)
	{
		SetGameObject(isOn: true);
		SetEmptySlot(squardSlotCount);
	}

	public void SetHeroPortraitComponentListener(HeroPortraitComponent.Listener listener)
	{
		_heroPortraitComponentListener = listener;
	}

	public void Release()
	{
		SetGameObject(isOn: false);
		ReleaseEmptySlot();
		ReleaseHeroPortraitAnim();
		_heroPortraitComponentListener = null;
	}

	public void SetGameObject(bool isOn)
	{
		UIHelper.EnableGameObject(Root, isOn);
		UIHelper.EnableGameObject(PanelSquard, isOn);
		UIHelper.EnableGameObject(ButtonSquardChange, isOn);
		UIHelper.EnableGameObject(EmptySlotGrid, isOn);
		UIHelper.EnableGameObject(EmptySlotTemplate, enable: false);
	}

	public void SetEmptySlot(int squardSlotCount)
	{
		if (squardSlotCount == __Extension_Method_ICollection__.COUNT(_listEmptySlotComponents))
		{
			return;
		}
		ReleaseEmptySlot();
		for (int i = 0; i < squardSlotCount; i++)
		{
			PartySlotComponent partySlotComponent = UIHelper.AddEmptySlotTemplate(EmptySlotGrid, EmptySlotTemplate);
			if (!(partySlotComponent == null))
			{
				_listEmptySlotComponents.Add(partySlotComponent);
			}
		}
		UIHelper.ResetGridPosition(EmptySlotGrid);
	}

	public UIGrid GetEmptySlotGrid()
	{
		return EmptySlotGrid;
	}

	public void SetEmptySlotClassTag(int index, CreatureTagType tagType)
	{
		PartySlotComponent emptySlotByIndex = GetEmptySlotByIndex(index);
		if (emptySlotByIndex == null)
		{
			VespaDebug.LogError("[SetEmptySlotClassTag Error] GameObject Is Null. Index :: " + index);
		}
		else
		{
			emptySlotByIndex.SetClassCategory(tagType);
		}
	}

	private void ReleaseEmptySlot()
	{
		if (__Extension_Method_ICollection__.COUNT(_listEmptySlotComponents) > 0)
		{
			_listEmptySlotComponents.ForEach(delegate(PartySlotComponent e)
			{
				VespaObject.DestroyGoAndDetachFromParent(e.gameObject);
			});
			_listEmptySlotComponents.Clear();
		}
	}

	public UIPanel GetPanelSquard()
	{
		return PanelSquard;
	}

	public int GetSquardSlotCount()
	{
		return __Extension_Method_ICollection__.COUNT(_listEmptySlotComponents);
	}

	public PartySlotComponent GetEmptySlotByIndex(int index)
	{
		if (__Extension_Method_ICollection__.COUNT(_listEmptySlotComponents) <= index)
		{
			return null;
		}
		return _listEmptySlotComponents[index];
	}

	public Common.HeroPortraitAnim CreateHeroPortraitAnim(CachedHeroInfo heroInfo, Vector3 position, HeroPortraitComponent.OwnerInfo ownerInfo, bool isControllerable, Action<Common.HeroPortraitAnim> setAction)
	{
		UIPanel panelSquard = GetPanelSquard();
		if (panelSquard == null)
		{
			VespaDebug.LogError("[AddSlotHeroPortrait Error] Panel Is Null ");
			return null;
		}
		Common.HeroPortraitAnim heroPortraitAnim = new Common.HeroPortraitAnim();
		using (new RecommendPanel(panelSquard))
		{
			heroPortraitAnim.Component = HeroPortraitSelector.PrefabAuto.Factory.Create(panelSquard.transform);
		}
		if (heroPortraitAnim.Component == null)
		{
			VespaDebug.LogError("[AddSlotHeroPortrait Error] HeroPortrait Is Null ");
			return null;
		}
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(heroPortraitAnim.Component.transform);
		heroPortraitAnim.Component.transform.position = position;
		heroPortraitAnim.Component.transform.localScale = HeroPortraitSize;
		heroPortraitAnim.Component.Open(heroInfo, _heroPortraitComponentListener, ownerInfo, PortraitMode);
		heroPortraitAnim.Component.IsControllerable = isControllerable;
		_listHeroPortraitAnim.Add(heroPortraitAnim);
		Vespa_System_Action_Extension_Method.fire(setAction, heroPortraitAnim);
		return heroPortraitAnim;
	}

	public void RemoveHeroPortraitAnim(HeroPortraitComponent heroDeckPortrait, int heroIndex, long ownerAccontId = 0L)
	{
		Common.HeroPortraitAnim heroPortraitAnim = GetHeroPortraitAnim(heroIndex, ownerAccontId);
		if (heroPortraitAnim != null)
		{
			_listHeroPortraitAnim.Remove(heroPortraitAnim);
			_listDiappearHeroPortraitAnim.Add(heroPortraitAnim);
			Vector3 endPos = heroPortraitAnim.Component.transform.localPosition;
			if (heroDeckPortrait != null)
			{
				endPos = heroPortraitAnim.Component.transform.parent.InverseTransformPoint(heroDeckPortrait.transform.position);
			}
			heroPortraitAnim.StartAnimation(Common.HeroPortraitAnim.EAnimation.Disappear, heroPortraitAnim.Component.transform.localPosition, endPos);
		}
	}

	public void UpdateHeroPortraitAnim()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listHeroPortraitAnim); i++)
		{
			_listHeroPortraitAnim[i].UpdateAnimation();
		}
		for (int num = __Extension_Method_ICollection__.COUNT(_listDiappearHeroPortraitAnim) - 1; num >= 0; num--)
		{
			Common.HeroPortraitAnim heroPortraitAnim = _listDiappearHeroPortraitAnim[num];
			if (!heroPortraitAnim.UpdateAnimation())
			{
				FactoryBase.ReleaseComponent(heroPortraitAnim.Component);
				_listDiappearHeroPortraitAnim.RemoveAt(num);
			}
		}
	}

	public Common.HeroPortraitAnim GetHeroPortraitAnim(int heroIndex, long ownerAccontId = 0L)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listHeroPortraitAnim); i++)
		{
			Common.HeroPortraitAnim heroPortraitAnim = _listHeroPortraitAnim[i];
			if ((ownerAccontId <= 0 || heroPortraitAnim.Component.OwnerId == ownerAccontId) && heroPortraitAnim.HeroIndex == heroIndex)
			{
				return heroPortraitAnim;
			}
		}
		return null;
	}

	public List<Common.HeroPortraitAnim> GetHeroPortraitAnimListByAccountId(long ownerAccountId)
	{
		List<Common.HeroPortraitAnim> list = new List<Common.HeroPortraitAnim>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listHeroPortraitAnim); i++)
		{
			Common.HeroPortraitAnim heroPortraitAnim = _listHeroPortraitAnim[i];
			if (heroPortraitAnim != null && heroPortraitAnim.Component.OwnerId == ownerAccountId)
			{
				list.Add(heroPortraitAnim);
			}
		}
		return list;
	}

	public List<Common.HeroPortraitAnim> GetAllHeroPortraitAnim()
	{
		return _listHeroPortraitAnim;
	}

	public void ReleaseHeroPortraitAnim()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listHeroPortraitAnim); i++)
		{
			Common.HeroPortraitAnim heroPortraitAnim = _listHeroPortraitAnim[i];
			if (heroPortraitAnim != null && !(heroPortraitAnim.Component == null))
			{
				FactoryBase.ReleaseComponent(heroPortraitAnim.Component);
			}
		}
		_listHeroPortraitAnim.Clear();
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_listDiappearHeroPortraitAnim); j++)
		{
			Common.HeroPortraitAnim heroPortraitAnim2 = _listDiappearHeroPortraitAnim[j];
			if (heroPortraitAnim2 != null && !(heroPortraitAnim2.Component == null))
			{
				FactoryBase.ReleaseComponent(heroPortraitAnim2.Component);
			}
		}
		_listDiappearHeroPortraitAnim.Clear();
	}

	public void SortSquardByWavePosition()
	{
		Common.SortMemberByWavePosition(_listHeroPortraitAnim);
	}

	public void AnimateSqaurdPosition(Common.HeroPortraitAnim heroPortraitAnim, Action<List<Common.HeroPortraitAnim>, List<PartySlotComponent>, Common.HeroPortraitAnim> animateAction)
	{
		SortSquardByWavePosition();
		Vespa_System_Action_Extension_Method.fire(animateAction, _listHeroPortraitAnim, _listEmptySlotComponents, heroPortraitAnim);
	}

	public List<int> GetSquardMyHeroIndexList()
	{
		List<int> list = new List<int>();
		long id = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listHeroPortraitAnim); i++)
		{
			Common.HeroPortraitAnim heroPortraitAnim = _listHeroPortraitAnim[i];
			if (IsValidHeroPortraitAnim(heroPortraitAnim) && heroPortraitAnim.Component.OwnerId == id)
			{
				list.Add(heroPortraitAnim.HeroIndex);
			}
		}
		return list;
	}

	public List<int> GetTotalSquardHeroIndexList()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listHeroPortraitAnim); i++)
		{
			Common.HeroPortraitAnim heroPortraitAnim = _listHeroPortraitAnim[i];
			if (IsValidHeroPortraitAnim(heroPortraitAnim))
			{
				list.Add(heroPortraitAnim.HeroIndex);
			}
		}
		return list;
	}

	public void AddTotalSquardHeroIndexList(List<int> heroIndexList)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listHeroPortraitAnim); i++)
		{
			Common.HeroPortraitAnim heroPortraitAnim = _listHeroPortraitAnim[i];
			if (IsValidHeroPortraitAnim(heroPortraitAnim))
			{
				if (heroIndexList == null)
				{
					heroIndexList = new List<int>();
				}
				heroIndexList.Add(heroPortraitAnim.HeroIndex);
			}
		}
	}

	public bool IsValidHeroPortraitAnim(Common.HeroPortraitAnim heroPortraitAnim)
	{
		if (heroPortraitAnim == null || heroPortraitAnim.HeroIndex == 0 || heroPortraitAnim.Component == null)
		{
			return false;
		}
		return true;
	}

	public List<long> GetTotalSquardHeroOwnerAccountIdList()
	{
		List<long> list = new List<long>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listHeroPortraitAnim); i++)
		{
			Common.HeroPortraitAnim heroPortraitAnim = _listHeroPortraitAnim[i];
			if (heroPortraitAnim != null && heroPortraitAnim.HeroIndex != 0 && !(heroPortraitAnim.Component == null))
			{
				list.Add(heroPortraitAnim.Component.OwnerId);
			}
		}
		return list;
	}

	public int GetHeroPortraitAnimCount()
	{
		return __Extension_Method_ICollection__.COUNT(_listHeroPortraitAnim);
	}

	public List<CreatureTagType> GetLeftClassTagList(List<CreatureTagType> classTagList)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(classTagList))
		{
			return null;
		}
		List<CreatureTagType> list = new List<CreatureTagType>(classTagList);
		int squardSlotCount = GetSquardSlotCount();
		List<int> totalSquardHeroIndexList = GetTotalSquardHeroIndexList();
		for (int i = 0; i < squardSlotCount; i++)
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(list))
			{
				break;
			}
			if (__Extension_Method_ICollection__.IsNullOrEmpty(totalSquardHeroIndexList))
			{
				break;
			}
			CreatureTagType creatureTagType = list[0];
			for (int num = __Extension_Method_ICollection__.COUNT(totalSquardHeroIndexList) - 1; num >= 0; num--)
			{
				int key = totalSquardHeroIndexList[num];
				CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(key);
				if (data != null && data.TagType == creatureTagType)
				{
					totalSquardHeroIndexList.RemoveAt(num);
					list.RemoveAt(0);
					break;
				}
			}
		}
		return list;
	}

	public void UpdateSquardPortraitAnimDepth()
	{
		int num = 100;
		for (int num2 = __Extension_Method_ICollection__.COUNT(_listHeroPortraitAnim) - 1; num2 >= 0; num2--)
		{
			UIDepthHelper componentAuto = Vespa_Unity_Component_Extension_Method.GetComponentAuto<UIDepthHelper>(_listHeroPortraitAnim[num2].Component);
			if (componentAuto != null)
			{
				num = componentAuto.RearrangeWidgetDepth(num + 1);
			}
		}
	}

	public void SetFocusingObject(bool isOn)
	{
		if (DisableRoot != null)
		{
			UIHelper.EnableGameObject(DisableRoot, !isOn);
		}
		if (EnableEffectRoot != null)
		{
			UIHelper.EnableGameObject(EnableEffectRoot, isOn);
		}
	}
}
