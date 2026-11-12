using NGame2.NAccount;
using NGameAsset;
using NNguiExtension;
using NShared;
using NShared.NWorldBoss;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class WorldBossGlobalRankerComponent : UIScrollItemBehaviour, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
		void OnClickComponent(long accountId);
	}

	public const string PrefabPath = "Prefab2/Component/WorldBoss/WorldBossGlobalRankerComponent";

	public static PrefabAuto<WorldBossGlobalRankerComponent> PrefabAuto = new PrefabAuto<WorldBossGlobalRankerComponent>("Prefab2/Component/WorldBoss/WorldBossGlobalRankerComponent");

	public UITexture Texture_HeroPortrait;

	public UITexture Texture_CountryFlag;

	public UILabel RankLabel_Other;

	public UILabel RankLabel_Me;

	public UISpriteSelector RankSpriteSelector;

	public UILabel Label_Name;

	public UILabel Label_Point;

	public UILabel Label_Server;

	public UILabel Label_GlobalName;

	public Transform GlobalNameRoot;

	private WorldBossRankerInfo rankerInfo;

	private Listener _Listener;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
	}

	public void SetData(WorldBossRankerInfo rankerInfo, string myServerGroup, Listener listener)
	{
		this.rankerInfo = rankerInfo;
		_Listener = listener;
		bool isMe = false;
		if (!String_Extension_Method.IsNullOrEmpty(myServerGroup))
		{
			isMe = rankerInfo.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId && rankerInfo.ServerGroup.Equals(myServerGroup);
		}
		SetHeroImage();
		SetRank(rankerInfo.Rank + 1, isMe);
		UIHelper.EnableGameObject(GlobalNameRoot, enable: true);
		UIHelper.EnableGameObject(Label_Name, enable: false);
		SetRegionInfo(rankerInfo.ServerGroup, rankerInfo.CountryCode);
		UIHelper.SetLabelText(Label_GlobalName, rankerInfo.Nick);
		UIHelper.SetLabelText(Label_Point, rankerInfo.Score.ToString("#,0"));
	}

	private void SetHeroImage()
	{
		string iconPath = UIAvatarHelper.GetIconPath(rankerInfo.AvatarHeroIndex);
		if (!String_Extension_Method.IsNullOrEmpty(iconPath))
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(iconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_HeroPortrait, plainTexture);
		}
	}

	private void SetRank(int rank, bool isMe)
	{
		if (rank <= 3)
		{
			if (RankSpriteSelector != null)
			{
				RankSpriteSelector.Select(rank.ToString());
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: true);
			UIHelper.EnableGameObject(RankLabel_Me, enable: false);
			UIHelper.EnableGameObject(RankLabel_Other, enable: false);
		}
		else
		{
			if (isMe)
			{
				UIHelper.SetLabelText(RankLabel_Me, rank);
			}
			else
			{
				UIHelper.SetLabelText(RankLabel_Other, rank);
			}
			UIHelper.EnableGameObject(RankSpriteSelector, enable: false);
			UIHelper.EnableGameObject(RankLabel_Me, isMe);
			UIHelper.EnableGameObject(RankLabel_Other, !isMe);
		}
	}

	private void SetRegionInfo(string serverGroup, string countryCode)
	{
		string serverName = GameRoot.Instance.ServerGroupNameDataContainer.GetServerName(serverGroup);
		if (!String_Extension_Method.IsNullOrEmpty(serverName))
		{
			UIHelper.SetLabelText(Label_Server, "[" + serverName + "]");
		}
		else
		{
			UIHelper.SetLabelText(Label_Server, string.Empty);
		}
		CountryCodeData dataByCode = GameRoot.Instance.CountryCodeDataContainer.GetDataByCode(countryCode);
		if (dataByCode != null)
		{
			AssetTexture plainTexture = UIHelper.GetPlainTexture(dataByCode.IconPath);
			UIHelper.ResetPlaneAssetTextureAsync(this, Texture_CountryFlag, plainTexture);
		}
	}

	public void OnClickComponent()
	{
		if (_Listener != null)
		{
			_Listener.OnClickComponent(rankerInfo.AccountId);
		}
	}
}
