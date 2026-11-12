using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.ChapterClearReward;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.LobbyChapterReward;

public class LobbyChapterClearReward : BaseWindow, ChapterRewardComponent.IListener, ChapterRewardComponent.INotiListener, IUserObserver<ChapterDungeonInfo>, IUserObserver
{
	public interface IListener
	{
		void GetReward(int chapterIndex, int step, Action responseComplete);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/LobbyChapterClearReward/LobbyChapterClearReward";

	public UILabel ChapterNameLabel;

	public UILabel StarCountLabel;

	public UITable RewardTable;

	public Transform FoldTranform;

	public Transform UnFoldTranform;

	public UISprite StarProgressSprite;

	public Transform NewTransform;

	private int _chapterIndex;

	private int _chapterTotalStar;

	private IListener _iListener;

	private bool _isFold;

	private static LocalAccountBool _foldLocal = new LocalAccountBool(i_default: false, "ChapterClearReward_Fold");

	private List<bool> _isGetRewardList = new List<bool>();

	private List<ChapterRewardComponent> _chapterRewardComponentList = new List<ChapterRewardComponent>();

	public int CurrentClearDungeonStar { get; private set; }

	private int ClearDungeonStar => GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.ClearDungeonStar(_chapterIndex, _chapterTotalStar);

	private bool IsGetRewardable
	{
		get
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_isGetRewardList))
			{
				return false;
			}
			return _isGetRewardList.Contains(item: true);
		}
	}

	public void Open(IListener iListener)
	{
		base.Open();
		Init(iListener);
		GlobalAutoBehaviour<AccountManager>.instance.AddUserObserver(this);
	}

	public override void Close()
	{
		base.Close();
		ClearRewardTable();
		GlobalAutoBehaviour<AccountManager>.instance.RemoveUserObserver(this);
	}

	private void Init(IListener iListener)
	{
		_iListener = iListener;
		_isFold = _foldLocal.Value;
		CurrentClearDungeonStar = ClearDungeonStar;
		if (_chapterRewardComponentList == null)
		{
			_chapterRewardComponentList = new List<ChapterRewardComponent>();
		}
		SetChapterRewardInfo();
		SetFold(_isFold);
	}

	public void SetChapterRewardInfo()
	{
		_chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		SetChapterAllStarCount();
		SetChapterName();
		SetStarCount();
		SetChaterRewardInfo();
		SetNew(IsGetRewardable);
	}

	private void SetChapterName()
	{
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(_chapterIndex);
		if (data != null)
		{
			string stringDirect = LocalizationManager.GetStringDirect(data.Name);
			UIHelper.SetLabelText(ChapterNameLabel, stringDirect);
		}
	}

	private void SetChapterAllStarCount()
	{
		_chapterTotalStar = GameRoot.Instance.CampaignDungeonDataContainer.GetTotalStarByChapterClearReward(_chapterIndex);
	}

	public void SetStarCount()
	{
		CurrentClearDungeonStar = ClearDungeonStar;
		UIHelper.SetLabelText(StarCountLabel, CurrentClearDungeonStar + "/" + _chapterTotalStar);
		SetProgress(CurrentClearDungeonStar);
	}

	private void SetProgress(int star)
	{
		if (!(StarProgressSprite == null))
		{
			if (_chapterTotalStar <= 0)
			{
				StarProgressSprite.fillAmount = 0f;
			}
			else
			{
				StarProgressSprite.fillAmount = (float)star / (float)_chapterTotalStar;
			}
		}
	}

	private void SetNew(bool isNew)
	{
		if (!(NewTransform == null))
		{
			UIHelper.EnableGameObject(NewTransform, isNew);
		}
	}

	private void ClearRewardTable()
	{
		FactoryBase.ReleaseAndClearList(_chapterRewardComponentList);
	}

	private void SetChaterRewardInfo()
	{
		ClearRewardTable();
		_isGetRewardList.Clear();
		List<ChapterClearRewardData> rewardDataByChapterIndex = GameRoot.Instance.ChapterClearRewardDataContainer.GetRewardDataByChapterIndex(_chapterIndex);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewardDataByChapterIndex); i++)
		{
			ChapterClearRewardData chapterClearRewardData = rewardDataByChapterIndex[i];
			if (chapterClearRewardData != null)
			{
				ChapterRewardComponent chapterRewardComponent = ChapterRewardComponent.PrefabAuto.Factory.Create(RewardTable.transform);
				if (chapterRewardComponent != null)
				{
					_isGetRewardList.Add(item: false);
					chapterRewardComponent.Open(this, this, chapterClearRewardData);
					_chapterRewardComponentList.Add(chapterRewardComponent);
				}
			}
		}
		RewardTable.repositionNow = true;
	}

	private void EnableFold(bool isEnable)
	{
		UIHelper.EnableGameObject(FoldTranform, isEnable);
	}

	private void EnableUnFold(bool isEnable)
	{
		UIHelper.EnableGameObject(UnFoldTranform, isEnable);
	}

	public void SetFold(bool isFold)
	{
		_isFold = isFold;
		_foldLocal.Value = _isFold;
		EnableFold(_isFold);
		EnableUnFold(!_isFold);
	}

	public void OnClickFold()
	{
		SetFold(!_isFold);
	}

	void ChapterRewardComponent.INotiListener.IsGetReward(int index, bool isGetReward)
	{
		if (index > 0 && _isGetRewardList.Count > index - 1)
		{
			_isGetRewardList[index - 1] = isGetReward;
			SetNew(IsGetRewardable);
		}
	}

	void ChapterRewardComponent.IListener.GetReward(int step, Action responseComplete)
	{
		if (_iListener != null)
		{
			_iListener.GetReward(_chapterIndex, step, responseComplete);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ChapterDungeonInfo>.OnChanged(ChapterDungeonInfo changedInfo)
	{
		if (changedInfo.ChapterIndex == _chapterIndex)
		{
			SetChapterRewardInfo();
		}
	}
}
