using System;
using NGameAsset;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GuildArenaRecordRankingComponent : UIScrollItemBehaviour, InitOnCreate
{
	[Serializable]
	public class GuildInfo
	{
		public UILabel Label_GuildName;

		public UILabel Label_Score;

		public UITexture Texture_GuildLogo;

		public UITexture Texture_GuildBack;
	}

	public const string PrefabPath = "Prefab2/Component/GuildArena/GuildArenaRecordRankingComponent";

	public static PrefabAuto<GuildArenaRecordRankingComponent> PrefabAuto = new PrefabAuto<GuildArenaRecordRankingComponent>("Prefab2/Component/GuildArena/GuildArenaRecordRankingComponent");

	public GuildInfo MyGuildInfo;

	public GuildInfo EnemyGuildInfo;

	public UILabel Label_Result;

	public UILabel Label_Session;

	private GuildArenaRankerRecordInfo _recordInfo;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		Clear();
	}

	public void Open(GuildArenaRankerRecordInfo recordInfo)
	{
		_recordInfo = recordInfo;
		Init();
	}

	public void Init()
	{
		if (_recordInfo == null)
		{
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("GUILD_ARENA_SESSION_WIN");
		if (_recordInfo.Win == 0)
		{
			stringDirect = LocalizationManager.GetStringDirect("GUILD_ARENA_SESSION_LOSE");
		}
		if (_recordInfo.Win == 2)
		{
			stringDirect = LocalizationManager.GetStringDirect("GUILD_ARENA_DRAW");
		}
		UIHelper.SetLabelText(Label_Result, stringDirect);
		string text = (stringDirect = LocalizationManager.GetStringDirect("GUILD_ARENA_SESSION_INFO", _recordInfo.SessionNumber));
		UIHelper.SetLabelText(Label_Session, text);
		if (_recordInfo.LeftRanker != null)
		{
			string text2 = _recordInfo.LeftRanker.Name;
			int sessionScore = _recordInfo.LeftRanker.SessionScore;
			byte logo = _recordInfo.LeftRanker.Logo;
			byte b = _recordInfo.LeftRanker.Back;
			if (b <= 0 || b > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
			{
				b = 1;
			}
			UIHelper.SetLabelText(MyGuildInfo.Label_GuildName, text2);
			UIHelper.SetLabelText(MyGuildInfo.Label_Score, sessionScore);
			AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(logo);
			UIHelper.ResetPlaneAssetTextureAsync(this, MyGuildInfo.Texture_GuildLogo, guildLogoIcon);
			AssetTexture guildBackGround2D = UIHelper.GetGuildBackGround2D(b);
			UIHelper.ResetPlaneAssetTextureAsync(this, MyGuildInfo.Texture_GuildBack, guildBackGround2D);
		}
		if (_recordInfo.RightRanker != null)
		{
			string text3 = _recordInfo.RightRanker.Name;
			int sessionScore2 = _recordInfo.RightRanker.SessionScore;
			byte logo2 = _recordInfo.RightRanker.Logo;
			byte b2 = _recordInfo.RightRanker.Back;
			if (b2 <= 0 || b2 > GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildBG"))
			{
				b2 = 1;
			}
			UIHelper.SetLabelText(EnemyGuildInfo.Label_GuildName, text3);
			UIHelper.SetLabelText(EnemyGuildInfo.Label_Score, sessionScore2);
			AssetTexture guildLogoIcon2 = UIHelper.GetGuildLogoIcon2(logo2);
			UIHelper.ResetPlaneAssetTextureAsync(this, EnemyGuildInfo.Texture_GuildLogo, guildLogoIcon2);
			AssetTexture guildBackGround2D2 = UIHelper.GetGuildBackGround2D(b2);
			UIHelper.ResetPlaneAssetTextureAsync(this, EnemyGuildInfo.Texture_GuildBack, guildBackGround2D2);
		}
	}

	private void Clear()
	{
	}
}
