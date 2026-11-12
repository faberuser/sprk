using System;
using NGame2.N3D;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class ChapterAvatarManager
{
	public interface IListener
	{
		void ChangeToMyAvatar();

		void RemovePartyMemberAvatars();

		void RemoveAllCrowdAvatar();
	}

	public class ChapterAvatarInfo
	{
		public int ChapterIndex { get; set; }

		public int DungeonIndex { get; set; }

		public int CostumeIndex { get; set; }

		public Mediator MyAvatarMediator { get; set; }
	}

	private ChapterAvatarInfo _chapterAvatarInfo;

	private IListener _iListener;

	public ChapterAvatarManager(IListener iListener)
	{
		_iListener = iListener;
	}

	public void Init(ChapterAvatarInfo chapterAvatarInfo)
	{
		_chapterAvatarInfo = chapterAvatarInfo;
		if (chapterAvatarInfo != null)
		{
			ChangeToChapterAvatar(chapterAvatarInfo.CostumeIndex, chapterAvatarInfo.DungeonIndex);
		}
	}

	public void Close()
	{
		if (_iListener != null)
		{
			_iListener.RemoveAllCrowdAvatar();
		}
		RemovePairEffect();
		_chapterAvatarInfo = null;
		_iListener = null;
	}

	private void ChangeToChapterAvatar(int costumeIndex, int dungeonIndex)
	{
		if (_chapterAvatarInfo != null && _chapterAvatarInfo.MyAvatarMediator != null && _chapterAvatarInfo.MyAvatarMediator.Avatar != null && GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
			if (data != null)
			{
				RenderFacadeBuilder renderFacadeBuilder = UIAvatarHelper.CreateRenderFacadeBuilder(new UIAvatarHelper.AvatarInfo(data).AvatarIndex);
				_chapterAvatarInfo.MyAvatarMediator.Avatar.CreateRenderFacade(renderFacadeBuilder);
				_chapterAvatarInfo.MyAvatarMediator.Avatar.SetPositionByDungeonIndex(dungeonIndex);
				NGame2.NAccount.UserCampaignManager.DungeonIndex = dungeonIndex;
			}
		}
	}

	public void ChangeToMyAvatar()
	{
		if (_iListener != null && GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			_iListener.ChangeToMyAvatar();
		}
	}

	public void PlayPairEffect(PairEffectType type, Action completeAction)
	{
		if (_chapterAvatarInfo != null && _chapterAvatarInfo.MyAvatarMediator != null && _chapterAvatarInfo.MyAvatarMediator.Avatar != null)
		{
			_chapterAvatarInfo.MyAvatarMediator.Avatar.PlayPairEffect(type, completeAction);
		}
	}

	private void RemovePairEffect()
	{
		if (_chapterAvatarInfo != null && _chapterAvatarInfo.MyAvatarMediator != null && _chapterAvatarInfo.MyAvatarMediator.Avatar != null)
		{
			_chapterAvatarInfo.MyAvatarMediator.Avatar.RemovePairEffect();
		}
	}

	public void WarpTo(int dungeonIndex)
	{
		if (_chapterAvatarInfo != null && _chapterAvatarInfo.MyAvatarMediator != null && _chapterAvatarInfo.MyAvatarMediator.AvatarController != null)
		{
			_chapterAvatarInfo.MyAvatarMediator.AvatarController.WarpTo(dungeonIndex);
		}
	}

	public bool CheckUseSpecificAvatar(int chapterIndex)
	{
		return GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex)?.MultiAvatarUse ?? false;
	}
}
