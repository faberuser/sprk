using System;
using System.Collections.Generic;
using NShared;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class MatchBanPickSlotComponent : MonoBehaviour
{
	public enum SlotState
	{
		None,
		Pick,
		Ban,
		Lock
	}

	[Serializable]
	public class ClassTagInfo
	{
		public CreatureTagType tagType;

		public UISprite Sprite_ClassTag;
	}

	public interface Listener
	{
		void OnLongClick(CachedHeroInfo heroInfo, long accountId);

		void OnClick(CachedHeroInfo heroInfo, long accountId);
	}

	public UILabel NameLabel;

	public UILabel LevelLabel;

	public UIBundleLoader bundleTex;

	public Transform LevelRoot;

	public Transform StarRoot;

	public Transform ClassTagRoot;

	public List<ClassTagInfo> ClassTagList = new List<ClassTagInfo>();

	public List<UIWidget> StarList = new List<UIWidget>();

	public List<UIWidget> TranscendStarList;

	public Transform PickEffect;

	public Transform BanEffect;

	public Transform LockEffect;

	public GameObject NPCFrameRoot;

	public GameObject BlindFrameRoot;

	private long _accountId;

	private Listener _listener;

	public long OwnerID { get; set; }

	public CachedHeroInfo HeroInfo { get; private set; }

	public SlotState CurrentSlotState { get; private set; }

	public void Init(Listener listener, CachedHeroInfo heroInfo, bool hideInfo, long accountId, SlotState slotState = SlotState.None)
	{
		_accountId = accountId;
		_listener = listener;
		SetState(slotState);
		SetHeroInfo(heroInfo, hideInfo);
	}

	public void InitEmpty(SlotState slotState = SlotState.None)
	{
		_accountId = 0L;
		_listener = null;
		SetState(slotState);
		SetHeroInfo(null);
	}

	private void SetEmpty()
	{
		HeroInfo = null;
		UIHelper.SetLabelText(NameLabel, string.Empty);
		UIHelper.EnableGameObject(bundleTex, enable: false);
		UIHelper.EnableGameObject(ClassTagRoot, enable: false);
		UIHelper.EnableGameObject(LevelRoot, enable: false);
		UIHelper.EnableGameObject(StarRoot, enable: false);
	}

	public void SetBlind()
	{
		UIHelper.EnableGameObject(BlindFrameRoot, enable: true);
	}

	public void SetNPC()
	{
		UIHelper.EnableGameObject(NPCFrameRoot, enable: true);
	}

	public bool IsEmptySlot()
	{
		if (HeroInfo != null)
		{
			return false;
		}
		return true;
	}

	private void SetHeroInfo(CachedHeroInfo heroInfo, bool hideInfo = true)
	{
		if (heroInfo == null)
		{
			SetEmpty();
			return;
		}
		HeroInfo = heroInfo;
		UIHelper.SetLabelText(NameLabel, HeroInfo.CreatureData.Name);
		string heroPortraitPath = UIHelper.GetHeroPortraitPath(HeroInfo.CreatureData.CodeName, (!hideInfo) ? HeroInfo.Star : 0, (!hideInfo) ? HeroInfo.CostumeIndex : 0);
		UIHelper.EAssetTextureAssign assetTexAssign = UIHelper.EAssetTextureAssign.ClipBottom;
		UIHelper.EnableGameObject(bundleTex, enable: true);
		UIHelper.SetBundleTexture(bundleTex, heroPortraitPath, assetTexAssign);
		if (!hideInfo)
		{
			UIHelper.EnableGameObject(LevelRoot, enable: true);
			int maxLevel = GameRoot.Instance.CreatureLevelDataContainer.MaxLevel;
			UIHelper.SetWidgetColor(LevelLabel, (HeroInfo.Level > maxLevel) ? Color.red : Color.white);
			UIHelper.SetLabelText(LevelLabel, HeroInfo.Level);
			UIHelper.EnableGameObject(StarRoot, enable: true);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(StarList); i++)
			{
				int num = i + 1;
				UIHelper.EnableGameObject(StarList[i], HeroInfo.Star == num && HeroInfo.Transcended == 0);
			}
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(TranscendStarList); j++)
			{
				int num2 = j + 1;
				UIHelper.EnableGameObject(TranscendStarList[j], HeroInfo.Transcended == num2);
			}
		}
	}

	public void OnLongClick()
	{
		if (_listener != null)
		{
			_listener.OnLongClick(HeroInfo, _accountId);
		}
	}

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClick(HeroInfo, _accountId);
		}
	}

	public void SetState(SlotState newState)
	{
		CurrentSlotState = newState;
		UIHelper.EnableGameObject(PickEffect, CurrentSlotState == SlotState.Pick);
		UIHelper.EnableGameObject(BanEffect, CurrentSlotState == SlotState.Ban);
		UIHelper.EnableGameObject(LockEffect, CurrentSlotState == SlotState.Lock);
	}
}
