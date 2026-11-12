using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GuildInfoComponent : UIScrollItemBehaviour
{
	public enum EShowMode
	{
		Join,
		CancelJoin,
		None
	}

	public interface Listener
	{
		void OnClickJoinGuild(GuildInfo info);

		void OnClickCancelRequest(GuildInfo info);
	}

	public const string PrefabPath = "Prefab2/Component/Guild/GuildInfoComponent";

	public static PrefabAuto<GuildInfoComponent> PrefabAuto = new PrefabAuto<GuildInfoComponent>("Prefab2/Component/Guild/GuildInfoComponent");

	public UILabel Label_Level;

	public UILabel Label_Name;

	public UILabel Label_MemberCount;

	public UILabel Label_Introduce;

	public UILabel Label_reqTeamLevel;

	public UILabel Label_MasterName;

	public UIButton Button_FreeJoin;

	public UIButton Button_AcceptJoin;

	public UIButton Button_CancelRequest;

	public UITexture TextureLogo;

	private EShowMode _showMode;

	private Listener _listener;

	public GuildInfo GuildInfo { get; private set; }

	public void Open(GuildInfo guildInfo, Listener listener, EShowMode showMode)
	{
		GuildInfo = guildInfo;
		_showMode = showMode;
		_listener = listener;
		SetupGuildInfo();
	}

	private void SetupGuildInfo()
	{
		if (GuildInfo != null)
		{
			UIHelper.SetLabelText(Label_Level, "Lv " + GuildInfo.Level);
			UIHelper.SetLabelText(Label_Name, GuildInfo.Name);
			int maxMember = GameRoot.Instance.GuildLevelDataContainer.GetData(GuildInfo.Level).MaxMember;
			UIHelper.SetLabelText(Label_MemberCount, "(" + GuildInfo.CurMember + "/" + maxMember + ")");
			UIHelper.SetLabelText(Label_Introduce, GuildInfo.Introduce);
			UIHelper.SetLabelText(Label_reqTeamLevel, LocalizationManager.GetString(Guild.GUILD_REQTEAMLEVEL, GuildInfo.ReqTeamLevel));
			UIHelper.SetLabelText(Label_MasterName, GuildInfo.MasterNick);
			SetGuildJoinButton(GuildInfo.JoinWay, _showMode);
			SetGuildLogo(GuildInfo.Logo);
		}
	}

	private void SetGuildJoinButton(byte joinWay, EShowMode showMode)
	{
		bool flag = joinWay == 2;
		bool flag2 = showMode == EShowMode.Join;
		bool enable = showMode == EShowMode.CancelJoin;
		UIHelper.EnableGameObject(Button_FreeJoin, flag2 && !flag);
		UIHelper.EnableGameObject(Button_AcceptJoin, flag2 && flag);
		UIHelper.EnableGameObject(Button_CancelRequest, enable);
	}

	private void SetGuildLogo(int logoIndex)
	{
		int logoIndex2 = logoIndex;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildIcon");
		if (logoIndex <= 0 || logoIndex > valueInt)
		{
			logoIndex2 = 1;
		}
		AssetTexture guildLogoIcon = UIHelper.GetGuildLogoIcon2(logoIndex2);
		UIHelper.ResetPlaneAssetTextureAsync(this, TextureLogo, guildLogoIcon);
	}

	public void JoinGuild()
	{
		if (_listener == null)
		{
			return;
		}
		if (GuildInfo.JoinWay == 1)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("GUILD_REQUEST_RECONFIRM", GuildInfo.Name);
			MessagePopup.Open(title, stringDirect, delegate
			{
				_listener.OnClickJoinGuild(GuildInfo);
			}, null);
		}
		else
		{
			_listener.OnClickJoinGuild(GuildInfo);
		}
	}

	public void CancelRequest()
	{
		if (_listener != null)
		{
			_listener.OnClickCancelRequest(GuildInfo);
		}
	}
}
