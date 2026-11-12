using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NDescription;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NStringProxy;
using NGame2.NUtil.NStringProxy.NTypeText;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroSkillIconComponent : UIScrollItemBehaviour, InitOnCreate
{
	public enum FrameType
	{
		Hero,
		Monster
	}

	public interface Listener
	{
		void OnSelectSkillComponent(HeroSkillIconComponent comp);
	}

	[Serializable]
	public class FrameInfo
	{
		public FrameType Type;

		public Transform Root;
	}

	public const string PrefabPath = "Prefab2/Component/Icon/HeroSkillIconComponent";

	public static PrefabAuto<HeroSkillIconComponent> PrefabAuto = new PrefabAuto<HeroSkillIconComponent>("Prefab2/Component/Icon/HeroSkillIconComponent");

	[SerializeField]
	private UIBundleLoader _iconBundleTex;

	[SerializeField]
	private UIGame3DWidget _effectSkillLevelUp;

	public FrameInfo[] FrameInfos;

	public SkillTooltipTextInfo TooltipTextInfo;

	public int SkillSlotIndex;

	private bool _isChecked;

	private bool _isEmpty;

	public GameObject CheckRoot;

	private Listener _listener;

	public HeroInfo HeroInfo { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		_ = _effectSkillLevelUp != null;
		if (TooltipTextInfo != null)
		{
			TooltipTextInfo.Reset();
			TooltipTextInfo = null;
		}
	}

	public bool Open(HeroInfo heroInfo, int skillSlotIndex, Listener listener)
	{
		SkillSlotIndex = skillSlotIndex;
		SetEmptySlot(enable: false);
		Helper.SkillBasicInfo skillBasicInfo = Helper.GetSkillBasicInfo(heroInfo, skillSlotIndex);
		return Open(heroInfo, skillBasicInfo, listener);
	}

	public bool Open(HeroInfo heroInfo, Helper.SkillBasicInfo skillInfo, Listener listener)
	{
		_listener = listener;
		SetSelected(select: false);
		HeroInfo = heroInfo;
		if (skillInfo.TableData == null)
		{
			return false;
		}
		string skillIconPath = UIHelper.GetSkillIconPath(skillInfo.TableIndex, heroInfo);
		UIHelper.SetBundleTexture(_iconBundleTex, skillIconPath);
		SetupTooltipText(skillInfo);
		SetupFrame(FrameType.Hero);
		return true;
	}

	public bool Open(HeroInfo heroInfo, Helper.SkillDetailInfo skillInfo, int skillSlotIndex, Listener listener)
	{
		return Open(heroInfo, skillInfo.BasicInfo, listener);
	}

	public void PlayLevelUpEffect()
	{
		if (_effectSkillLevelUp != null)
		{
			_effectSkillLevelUp.Play(null);
		}
	}

	public void SetupFrame(FrameType type)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(FrameInfos); i++)
		{
			FrameInfo frameInfo = FrameInfos[i];
			UIHelper.EnableGameObject(frameInfo.Root, frameInfo.Type == type);
		}
	}

	public void SetEmptySlot(bool enable)
	{
		_isEmpty = enable;
		UIHelper.EnableGameObject(_iconBundleTex, !enable);
	}

	public void SetSelected(bool select)
	{
		UIHelper.EnableGameObject(CheckRoot, select);
		_isChecked = select;
	}

	public bool IsChecked()
	{
		return _isChecked;
	}

	public bool IsEmptySlot()
	{
		return _isEmpty;
	}

	public void OnClickComponent()
	{
		if (_listener != null)
		{
			_listener.OnSelectSkillComponent(this);
		}
	}

	public void OnShowTooltip()
	{
		SkillIconTooltip.Open(_iconBundleTex.UITEX(), TooltipTextInfo);
	}

	public void OnHideTooltip()
	{
		SkillIconTooltip.Hide(_iconBundleTex.UITEX());
	}

	private void SetupTooltipText(Helper.SkillBasicInfo basicInfo)
	{
		if (basicInfo.TableIndex != 0)
		{
			if (TooltipTextInfo == null)
			{
				TooltipTextInfo = new SkillTooltipTextInfo();
			}
			TooltipTextInfo.Reset();
			TooltipTextInfo.Name.Value = SkillDescContainer.GetSkillName(HeroInfo, basicInfo.TableData);
			TooltipTextInfo.Desc.Assign(new SkillDescBuilder(basicInfo.TableIndex, HeroInfo, appendLine: false));
			TooltipTextInfo.TypeText.Assign(new SkillBuilder(basicInfo, appendLine: false));
			TooltipTextInfo.SkillLevel = basicInfo.Level;
			TooltipTextInfo.CooltimeMs = ((basicInfo.TableData != null) ? basicInfo.TableData.CoolTimeMs : 0);
			TooltipTextInfo.isActiveSkill = basicInfo.TableData.TriggerType == SkillTriggerType.None;
			StatData heroStat = StatCalculator.GetHeroStat(HeroInfo, new StatType[1] { StatType.MaxMp });
			int reqMp = ((basicInfo.TableData != null) ? basicInfo.TableData.Mp : 0);
			TooltipTextInfo.InitReqManaGauge(reqMp, heroStat.MaxMp);
		}
	}

	public static List<HeroSkillIconComponent> CreateSkillIcon(HeroInfo heroInfo, UISimpleGrid iconGrid)
	{
		List<HeroSkillIconComponent> list = new List<HeroSkillIconComponent>();
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
		if (data == null)
		{
			return list;
		}
		for (int i = 0; i < 4; i++)
		{
			int num = i + 1;
			int skillIndexBySlot = data.GetSkillIndexBySlot(num);
			Helper.SkillDetailInfo skillInfo = new Helper.SkillDetailInfo
			{
				BasicInfo = 
				{
					TableIndex = data.GetSkillIndexBySlot(num),
					TableData = GameRoot.Instance.SkillDataContainer.GetData(skillIndexBySlot),
					Level = 0,
					IsLocked = true
				}
			};
			HeroSkillIconComponent heroSkillIconComponent = UIHelper.AddGridItem(iconGrid, PrefabAuto);
			heroSkillIconComponent.Open(heroInfo, skillInfo, num, null);
			list.Add(heroSkillIconComponent);
		}
		UIHelper.RepositionGrid(iconGrid);
		return list;
	}

	public static List<Component> CreateSkillIcon(HeroInfo heroInfo, Transform parent)
	{
		List<Component> list = new List<Component>();
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
		if (data == null)
		{
			return list;
		}
		for (int i = 0; i < 4; i++)
		{
			int num = i + 1;
			data.GetSkillIndexBySlot(num);
			int[] linkedSkillIndexBySlot = data.GetLinkedSkillIndexBySlot(num);
			if (__Extension_Method_ICollection__.COUNT(linkedSkillIndexBySlot) <= 0)
			{
				HeroSkillIconComponent heroSkillIconComponent = PrefabAuto.Factory.Create(parent);
				heroSkillIconComponent.Open(heroInfo, num, null);
				list.Add(heroSkillIconComponent);
			}
			else
			{
				HeroLinkedSkillIconComponent heroLinkedSkillIconComponent = null;
				heroLinkedSkillIconComponent = ((__Extension_Method_ICollection__.COUNT(linkedSkillIndexBySlot) != 1) ? HeroLinkedSkillIconComponent.PrefabAuto3.Factory.Create(parent) : HeroLinkedSkillIconComponent.PrefabAuto.Factory.Create(parent));
				heroLinkedSkillIconComponent.Open(heroInfo, num, null);
				list.Add(heroLinkedSkillIconComponent);
			}
		}
		return list;
	}
}
