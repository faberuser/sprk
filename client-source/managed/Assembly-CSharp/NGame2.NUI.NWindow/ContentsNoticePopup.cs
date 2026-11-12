using System.Collections.Generic;
using NGame2.NContents;
using NGame2.NContentsInfo;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ContentsNoticePopup : BaseWindow
{
	public interface IListener
	{
		void OnClickMovoToContents(ContentsNoticePopupData popupData);

		void OnClickMoveToHonoraryChamber(ContentsNoticeType contentType);

		void OnClickStartPopupClose();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/ContentsNoticePopup";

	public UILabel LabelTitle;

	public UITexture TextureContentsImage;

	public UIButton ButtonMoveToHonoraryChamber;

	public UIButton ButtonMoveToContents;

	public Transform ContentsStartRoot;

	public Transform ContentsEndRoot;

	public UITable RankingTable;

	public Transform ServerTotalRankRoot;

	public UISimpleGrid GridGlobalRanking;

	public UISimpleGrid GridServerTotalRank;

	public Transform FrameAnimator;

	[Header("[BanPick league Only")]
	public Transform SeasonRewardRoot;

	public UISprite Sprite_Banpick;

	public UISimpleGrid SeasonRewardGrid;

	private ContentsNoticePopupData _popupData;

	private List<ContentsNoticePopupRankerComponent> _globalRankingComponentList = new List<ContentsNoticePopupRankerComponent>();

	private List<ContentsNoticePopupServerTotalRankingComponent> _serverTotalRankComponentList = new List<ContentsNoticePopupServerTotalRankingComponent>();

	private List<HeroPortraitComponent> _heroPortraitList = new List<HeroPortraitComponent>();

	private IListener _iListener;

	private const int MaxRankerListCount = 3;

	public void Open(IListener listener, ContentsNoticePopupData popupData, int seasonIndex, ContentsNoticePopupRankerInfo[] rankerInfos, ContentsNoticePopupServerTotalRankingInfo[] serverRankInfos)
	{
		_iListener = listener;
		Open(delegate
		{
			Init(popupData);
		}, delegate
		{
			UIHelper.EnableGameObject(FrameAnimator, enable: true);
			UIHelper.EnableGameObject(SeasonRewardRoot, enable: false);
			UIHelper.EnableGameObject(Sprite_Banpick, enable: false);
			switch (popupData.PopupType)
			{
			case ContentsNoticePopupType.GlobalStart:
			case ContentsNoticePopupType.GlobalNodeStart:
				InitMoveToContentsButton(popupData);
				break;
			case ContentsNoticePopupType.GlobalEnd:
				SetRankerInfo(rankerInfos);
				SetServerRankInfo(serverRankInfos);
				if (RankingTable != null)
				{
					RankingTable.Reposition();
				}
				break;
			}
		});
	}

	private void Init(ContentsNoticePopupData popupData)
	{
		_popupData = popupData;
		UIHelper.EnableGameObject(ContentsStartRoot, popupData.PopupType == ContentsNoticePopupType.GlobalStart || popupData.PopupType == ContentsNoticePopupType.GlobalNodeStart);
		UIHelper.EnableGameObject(ContentsEndRoot, popupData.PopupType == ContentsNoticePopupType.GlobalEnd);
		UIHelper.EnableGameObject(ButtonMoveToHonoraryChamber, popupData.UseHonoraryChamber);
		UIHelper.EnableButton(ButtonMoveToContents, enable: false, grayscaleIfDisable: true);
		AssetTexture plainTexture = UIHelper.GetPlainTexture(popupData.ContentsImagePath);
		UIHelper.ResetPlaneAssetTextureAsync(this, TextureContentsImage, plainTexture);
		string stringDirect = LocalizationManager.GetStringDirect(popupData.TitleDescKey);
		UIHelper.SetLabelText(LabelTitle, stringDirect);
	}

	private void InitMoveToContentsButton(ContentsNoticePopupData popupData)
	{
		if (!CheckContentsState(popupData))
		{
			return;
		}
		if (popupData.ContentsType == ContentsNoticeType.BanPick)
		{
			MatchSeasonType matchSeasonType = MatchSeasonType.Free;
			MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<MatchArena>.instance.GetMatchSeasonInfo(MatchPvpType.BanPick);
			if (matchSeasonInfo != null)
			{
				matchSeasonType = matchSeasonInfo.MatchSeasonType;
			}
			if (matchSeasonType == MatchSeasonType.Regular)
			{
				int[] costumeIndices = SceneAutoBehaviour<MatchManagement>.instance.GetBanPickSeasonCostumeReward(ItemType.CostumeSelectItem).SubData.CostumeIndices;
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(costumeIndices))
				{
					UIHelper.EnableGameObject(SeasonRewardRoot, enable: true);
					UIHelper.EnableGameObject(Sprite_Banpick, enable: true);
					int[] array = costumeIndices;
					foreach (int key in array)
					{
						CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(key);
						if (data != null)
						{
							CreatureData data2 = GameRoot.Instance.CreatureDataContainer.GetData(data.HeroIndex);
							HeroPortraitComponent heroPortraitComponent = UIHelper.AddGridItem(SeasonRewardGrid, HeroPortraitComponent.PrefabAuto);
							heroPortraitComponent.Open(data2, null, HeroPortraitComponent.EPortraitMode.BanPick, 0, 0, data.CostumeIndex);
							heroPortraitComponent.ShowName(data.CostumeName);
							heroPortraitComponent.ShowStar(show: false);
							UIHelper.EnableGameObject(heroPortraitComponent, enable: true);
							_heroPortraitList.Add(heroPortraitComponent);
						}
					}
					SeasonRewardGrid.RepositionGrid();
				}
			}
			SingletonAuto<MatchArena>.instance.RequestSeasonInfo(MatchPvpType.BanPick, delegate(MatchArena.MatchSeasonInfo seasonInfo)
			{
				if (seasonInfo != null)
				{
					UIHelper.EnableButton(ButtonMoveToContents, enable: true, grayscaleIfDisable: true);
				}
			});
		}
		else
		{
			UIHelper.EnableButton(ButtonMoveToContents, enable: true, grayscaleIfDisable: true);
		}
	}

	public override void Close()
	{
		FactoryBase.ReleaseAndClearList(_heroPortraitList);
		base.Close();
	}

	private bool CheckContentsState(ContentsNoticePopupData popupData)
	{
		if (SingletonAuto<ContentsStatusManager>.instance.GetContentsStatus(popupData.ContentsStatusType, popupData.ContentsStatusSubType) != ContentsStatus.On)
		{
			return false;
		}
		return true;
	}

	private void SetRankerInfo(ContentsNoticePopupRankerInfo[] rankerInfos)
	{
		FactoryBase.ReleaseAndClearList(_globalRankingComponentList);
		if (__Extension_Method_ICollection__.COUNT(rankerInfos) == 0)
		{
			return;
		}
		foreach (ContentsNoticePopupRankerInfo rankerInfo in rankerInfos)
		{
			if (__Extension_Method_ICollection__.COUNT(_globalRankingComponentList) >= 3)
			{
				break;
			}
			ContentsNoticePopupRankerComponent contentsNoticePopupRankerComponent = UIHelper.AddGridItem(GridGlobalRanking, ContentsNoticePopupRankerComponent.PrefabAuto);
			if (contentsNoticePopupRankerComponent == null || !contentsNoticePopupRankerComponent.SetData(_popupData.ContentsType, rankerInfo))
			{
				FactoryBase.ReleaseComponent(contentsNoticePopupRankerComponent);
			}
			else
			{
				_globalRankingComponentList.Add(contentsNoticePopupRankerComponent);
			}
		}
		UIHelper.RepositionGrid(GridGlobalRanking);
	}

	private void SetServerRankInfo(ContentsNoticePopupServerTotalRankingInfo[] serverRankInfos)
	{
		FactoryBase.ReleaseAndClearList(_serverTotalRankComponentList);
		UIHelper.EnableGameObject(ServerTotalRankRoot, __Extension_Method_ICollection__.COUNT(serverRankInfos) > 0);
		if (__Extension_Method_ICollection__.COUNT(serverRankInfos) == 0)
		{
			return;
		}
		foreach (ContentsNoticePopupServerTotalRankingInfo data in serverRankInfos)
		{
			if (__Extension_Method_ICollection__.COUNT(_serverTotalRankComponentList) >= 3)
			{
				break;
			}
			ContentsNoticePopupServerTotalRankingComponent contentsNoticePopupServerTotalRankingComponent = UIHelper.AddGridItem(GridServerTotalRank, ContentsNoticePopupServerTotalRankingComponent.PrefabAuto);
			if (contentsNoticePopupServerTotalRankingComponent == null || !contentsNoticePopupServerTotalRankingComponent.SetData(data))
			{
				FactoryBase.ReleaseComponent(contentsNoticePopupServerTotalRankingComponent);
			}
			else
			{
				_serverTotalRankComponentList.Add(contentsNoticePopupServerTotalRankingComponent);
			}
		}
		UIHelper.RepositionGrid(GridServerTotalRank);
	}

	public void OnClickMoveToContents()
	{
		if (_iListener != null)
		{
			_iListener.OnClickMovoToContents(_popupData);
		}
	}

	public void OnClickMoveToHonoraryChamber()
	{
		if (_iListener != null)
		{
			Close();
			_iListener.OnClickMoveToHonoraryChamber(_popupData.ContentsType);
		}
	}

	public void OnClickStartPopupClose()
	{
		if (_iListener != null)
		{
			_iListener.OnClickStartPopupClose();
		}
	}

	public override void Reset()
	{
		base.Reset();
		UIHelper.SetLabelText(LabelTitle, string.Empty);
		UIHelper.ClearPlaneAssetTexture(TextureContentsImage);
		UIHelper.EnableButton(ButtonMoveToContents, enable: false, grayscaleIfDisable: true);
		UIHelper.EnableGameObject(FrameAnimator, enable: false);
		FactoryBase.ReleaseAndClearList(_globalRankingComponentList);
		FactoryBase.ReleaseAndClearList(_serverTotalRankComponentList);
	}
}
