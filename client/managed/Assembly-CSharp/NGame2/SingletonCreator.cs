using System;
using System.Globalization;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using NDrawCall;
using NGame2.IAP;
using NGame2.NAccount;
using NGame2.NBattleField.NManager.NCampaign.NState;
using NGame2.NBattleThread;
using NGame2.NBattleView.NUIOn3D;
using NGame2.NFunctionality;
using NGame2.NLogin;
using NGame2.NMatch;
using NGame2.NNotify;
using NGame2.NScene;
using NGame2.NScene.NLobbyScenePrerequisites;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NShared.NContentsDefine;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NNGUIExtension;
using NVespa.NScene;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2;

public class SingletonCreator : BehaviourUpdater, IInspectorGuiDrawer
{
	private static bool _initializedOnStartup = false;

	public static uint InitTotalAlloc;

	public static uint InitTotalReserved;

	public static uint InitUnusedReserved;

	public static float RandomValue;

	public static bool BitConvert_bool = false;

	public static string BitConvert_number = "0";

	public static SentrySdk sentry = null;

	public static LocalStorageString TestOpenUrl = new LocalStorageString(string.Empty, typeof(SingletonCreator).FullName + "TestOpenUrl");

	private static void InitEnum()
	{
		EnumAttributeContainer<FunctionalityType, FunctionalityAttribute>.Init();
		EnumAttributeContainer<EventBonusType, EventBonusAttribute>.Init();
		EnumAttributeContainer<OperationType, OperationTypeAttribute>.Init();
		EnumAttributeContainer<StatType, StatTypeAttribute>.Init();
		EnumAttributeContainer<ERightMenuButton, RightMenuButtonAttribute>.Init();
		EnumAttributeContainer<SkillTargetType, SkillTargetAttribute>.Init();
	}

	private static void InitializeOnStartup()
	{
	}

	private void InitializeOnSceneChange()
	{
		SceneAutoBehaviour<Game2UpdateManager>.CreateInstance();
	}

	private void Start()
	{
		Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
		ZipConstants.DefaultCodePage = 0;
		if (sentry == null)
		{
			sentry = base.gameObject.AddComponent(typeof(SentrySdk)) as SentrySdk;
			if (sentry != null)
			{
				sentry.Dsn = "https://4933b5e3f36b47cb9c3a26fc971a2288@sentry.io/1517981";
			}
		}
		SingletonUpdater<Executer>.instance.NextRun(InitializeOnStartup);
		SingletonUpdater<Executer>.instance.NextRun(InitializeOnSceneChange);
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("Define", this, delegate
		{
			if (IAPWrapper.StoreType == IAPWrapper.EStoreType.OneStore)
			{
				GUILayout.Label("PLATFORM_ONESTORE");
			}
		});
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("_SingletonCreator", this, delegate
		{
			if (GUILayout.Button("Open AppReviewPopup"))
			{
				AppReviewPopup.Open();
			}
			GUILayout.Label("HashString Dic Count: " + HashStringContainer.Count);
			int totalIntCount = MeshIndexBuffer.IndexPool.TotalIntCount;
			int num = totalIntCount * 4;
			GUILayout.Label($"NGUI DrawCall Index Buffer Alloc: {totalIntCount}({Vespa_Int_Extension_Method.ToSizeString(num)})");
			Define.TestMiniGame.Value = GUILayout.Toggle(Define.TestMiniGame.Value, "TestMiniGame");
			Define.TableSingleLoad.Value = GUILayout.Toggle(Define.TableSingleLoad.Value, "TableSingleLoad");
			TextAssetAsyncLoader.FileReadMethod.Value = EnumTools.IngamePopup("FileReadMethod", TextAssetAsyncLoader.FileReadMethod.Value);
			LoadingManager.ManualLoading.Value = GUILayout.Toggle(LoadingManager.ManualLoading.Value, "ManualLoading");
			UnityTimeScaler.EBaseTimeScale value = UnityTimeScaler.BaseTimeScale.Value;
			UnityTimeScaler.BaseTimeScale.Value = EnumTools.IngamePopup("Base TimeScale", UnityTimeScaler.BaseTimeScale.Value);
			if (value != UnityTimeScaler.BaseTimeScale.Value)
			{
				UnityTimeScaler.UpdateUnityScale();
			}
			GameOptionData.EditorToggleAutoPlay = GUILayout.Toggle(GameOptionData.EditorToggleAutoPlay, "Editor AutoPlay");
			GameOptionData.EditorToggleAutoRepeat = GUILayout.Toggle(GameOptionData.EditorToggleAutoRepeat, "Editor AutoRepeat");
			GameOptionData.EditorToggleAutoSkill = GUILayout.Toggle(GameOptionData.EditorToggleAutoSkill, "Editor AutoSkill");
			using (new BlockManager().Horizontal())
			{
				GameOptionData.EditorToggleAutoPvp = GUILayout.Toggle(GameOptionData.EditorToggleAutoPvp, "Editor AutoPvp");
				GameOptionData.EditorAutoPvpPoint.Value = GUILayout.TextField(GameOptionData.EditorAutoPvpPoint.Value);
			}
			ThreadInstance.UsingThreadFake.Value = GUILayout.Toggle(ThreadInstance.UsingThreadFake.Value, "Thread Fake");
			ThreadInstance.SkipNodeTree.Value = GUILayout.Toggle(ThreadInstance.SkipNodeTree.Value, "Skip Battle NodeTree");
			HUD.ToggleHpNumber.Value = GUILayout.Toggle(HUD.ToggleHpNumber.Value, "Show Hp Number");
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("Begin Battle Delay " + WaveOpening.DelayTime);
				if (GUILayout.Button("+", GUILayout.Width(30f)))
				{
					WaveOpening.DelayTime += 5f;
				}
				if (GUILayout.Button("-", GUILayout.Width(30f)))
				{
					WaveOpening.DelayTime -= 5f;
				}
				if (GUILayout.Button("0", GUILayout.Width(30f)))
				{
					WaveOpening.DelayTime = 0f;
				}
			}
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("AsyncWait Delay " + AsyncManager.DelayFrame);
				if (GUILayout.Button("-", GUILayout.Width(30f)))
				{
					AsyncManager.DelayFrame--;
				}
				if (GUILayout.Button("+", GUILayout.Width(30f)))
				{
					AsyncManager.DelayFrame++;
				}
			}
			using (new BlockManager().Horizontal())
			{
				if (GUILayout.Button("WaitingPopup.Open"))
				{
					WaitingPopup.Open(this);
				}
				if (GUILayout.Button("WaitingPopup.Close"))
				{
					WaitingPopup.Close(this);
				}
			}
			NGame2.NScene.NLobbyScenePrerequisites.QueryMarketInfoWrapper.ForceTimeout = GUILayout.Toggle(NGame2.NScene.NLobbyScenePrerequisites.QueryMarketInfoWrapper.ForceTimeout, "QueryMarketInfoWrapper.ForceTimeout");
			NGame2.NScene.NLobbyScenePrerequisites.QueryServerDownWrapper.ForceTimeout = GUILayout.Toggle(NGame2.NScene.NLobbyScenePrerequisites.QueryServerDownWrapper.ForceTimeout, "QueryServerDownWrapper.ForceTimeout");
			NGame2.NScene.NLobbyScenePrerequisites.RaiseChapterMoveEventWrapper.ForceTimeout = GUILayout.Toggle(NGame2.NScene.NLobbyScenePrerequisites.RaiseChapterMoveEventWrapper.ForceTimeout, "RaiseChapterMoveEventWrapper.ForceTimeout");
			RequestEnterLobbyWrapper.ForceTimeout = GUILayout.Toggle(RequestEnterLobbyWrapper.ForceTimeout, "RequestEnterLobbyWrapper.ForceTimeout");
			NGame2.NScene.NLobbyScenePrerequisites.WorldMapOpenWrapper.ForceTimeout = GUILayout.Toggle(NGame2.NScene.NLobbyScenePrerequisites.WorldMapOpenWrapper.ForceTimeout, "WorldMapOpenWrapper.ForceTimeout");
			NGame2.NScene.NLobbyScenePrerequisites.GuildRaidInitWrapper.ForceTimeout = GUILayout.Toggle(NGame2.NScene.NLobbyScenePrerequisites.GuildRaidInitWrapper.ForceTimeout, "GuildRaidInitWrapper.ForceTimeout");
			if (GUILayout.Button("Open EventWebView"))
			{
				EventWebViewPopup.OpenGlobal();
			}
			GUILayout.Label("Host=" + SingletonUpdater<WebServiceManager>.instance.Requester.Host);
			using (new BlockManager().Horizontal())
			{
				using (new BlockManager().Enable(!String_Extension_Method.IsNullOrEmpty(TestOpenUrl.Value)))
				{
					if (GUILayout.Button("Open Url", GUILayout.Width(70f)))
					{
						string text = TestOpenUrl.Value;
						if (SingletonUpdater<WebServiceManager>.isValidInstance)
						{
							text = text + "?SessionKey=" + WWW.EscapeURL(SingletonUpdater<WebServiceManager>.instance.Requester.SessionKey);
						}
						Application.OpenURL(text);
					}
					if (GUILayout.Button("Open2", GUILayout.Width(70f)))
					{
						string text2 = TestOpenUrl.Value;
						if (SingletonUpdater<WebServiceManager>.isValidInstance)
						{
							text2 = text2 + "?SessionKey=" + WWW.EscapeURL(SingletonUpdater<WebServiceManager>.instance.Requester.SessionKey);
						}
						EventWebViewPopup.OpenGlobal(text2);
					}
				}
				TestOpenUrl.Value = GUILayout.TextField(TestOpenUrl.Value);
			}
			if (GUILayout.Button("Email"))
			{
				Application.OpenURL("mailto:test@vespainteractive.com?subject=Email&body=from Unity");
			}
		});
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("AndroidKeyboard", this, delegate
		{
			UIAndroidKeyboard.DrawGui();
		});
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("BitConverter", this, delegate
		{
			GUILayout.Label($"IsLittleEndian={BitConverter.IsLittleEndian}");
			BitConvert_bool = GUILayout.Toggle(BitConvert_bool, "bool");
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("Number", GUILayout.Width(70f));
				BitConvert_number = GUILayout.TextField(BitConvert_number);
			}
			byte[] bytes = BitConverter.GetBytes(BitConvert_bool);
			GUILayout.Label(string.Format("bool:{0} -> {1}", BitConvert_bool, Vespa_String_Extension_Method.ToStringJoin(bytes, ", ")));
			bytes = BitConverter2.GetBytes(BitConvert_bool);
			GUILayout.Label(string.Format("bool:{0} -> {1}", BitConvert_bool, Vespa_String_Extension_Method.ToStringJoin(bytes, ", ")));
			if (long.TryParse(BitConvert_number, out var result))
			{
				short num = (short)(result & 0xFFFF);
				int num2 = (int)(result & 0xFFFFFFFFu);
				byte[] bytes2 = BitConverter.GetBytes(num);
				GUILayout.Label(string.Format("short:{0} -> {1}", num, Vespa_String_Extension_Method.ToStringJoin(bytes2, ", ")));
				bytes2 = BitConverter2.GetBytes(num);
				GUILayout.Label(string.Format("short:{0} -> {1}", num, Vespa_String_Extension_Method.ToStringJoin(bytes2, ", ")));
				byte[] bytes3 = BitConverter.GetBytes(num2);
				GUILayout.Label(string.Format("int:{0} -> {1}", num2, Vespa_String_Extension_Method.ToStringJoin(bytes3, ", ")));
				bytes3 = BitConverter2.GetBytes(num2);
				GUILayout.Label(string.Format("int:{0} -> {1}", num2, Vespa_String_Extension_Method.ToStringJoin(bytes3, ", ")));
				byte[] bytes4 = BitConverter.GetBytes(result);
				GUILayout.Label(string.Format("long:{0} -> {1}", result, Vespa_String_Extension_Method.ToStringJoin(bytes4, ", ")));
				bytes4 = BitConverter2.GetBytes(result);
				GUILayout.Label(string.Format("long:{0} -> {1}", result, Vespa_String_Extension_Method.ToStringJoin(bytes4, ", ")));
			}
			if (double.TryParse(BitConvert_number, out var result2))
			{
				byte[] bytes5 = BitConverter.GetBytes(result2);
				GUILayout.Label(string.Format("double:{0} -> {1}", result2, Vespa_String_Extension_Method.ToStringJoin(bytes5, ", ")));
				bytes5 = BitConverter2.GetBytes(result2);
				GUILayout.Label(string.Format("double:{0} -> {1}", result2, Vespa_String_Extension_Method.ToStringJoin(bytes5, ", ")));
			}
		});
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("RematchTest", this, delegate
		{
			GUILayout.Label($"Can accept invitation: {InviteNotifyManager.IsOpenableRematch(0L)}");
			if (GUILayout.Button("Pop Invitation"))
			{
				GlobalAutoBehaviour<InviteNotifyManager>.instance.AddNotify(InviteNotifyType.Rematch, new InviteRematchReq
				{
					Nick = "테스트"
				}, delegate(bool accepted)
				{
					VespaDebug.LogError("Accepted: " + accepted);
				});
			}
			if (GUILayout.Button("Pop Waiting Accept"))
			{
				WindowPrefab<DuelWaitingPopup> windowPrefab = new WindowPrefab<DuelWaitingPopup>("Resources/Prefab2/Window/Match/DuelWaitingPopup", GameUIRoot.ELayerType.UI);
				windowPrefab.instance.Open(DuelWaitingPopup.EWaitingType.RequestDuel, SingletonAuto<RematchManager>.instance, 10);
				GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
			}
			if (GUILayout.Button("Close All Popup"))
			{
				GlobalAutoBehaviour<PopupManegement>.instance.CloseAll();
			}
		});
		if (SingletonUpdater<AssetCacheService>.isValidInstance)
		{
			SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("AssetCacheService", SingletonUpdater<AssetCacheService>.instance, SingletonUpdater<AssetCacheService>.instance.DrawGui);
		}
		if (GlobalAutoBehaviour<HostInfoManager>.isValidInstance)
		{
			SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("Host Info", GlobalAutoBehaviour<HostInfoManager>.instance, GlobalAutoBehaviour<HostInfoManager>.instance.DrawGui);
		}
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
		MainTimer.Refresh();
		GameRoot.Instance.Update();
		FrameSingletonCollection.ChainUpdate();
	}

	private static void DrawGui_ContentsDefine()
	{
		if (!AccountManager.IsLogin())
		{
			GUILayout.Label("아직 로그인 전");
			return;
		}
		if (!SingletonUpdater<LoginManager>.isValidInstance)
		{
			GUILayout.Label("아직 로그인 전 2");
			return;
		}
		OfficialChannel officialChannel = ContentsDefine.OfficialChannel;
		if (officialChannel == null)
		{
			GUILayout.Label("아직 사용 할 수 없음.");
			return;
		}
		string serverName = SingletonUpdater<LoginManager>.instance.ServerName;
		string selectedServer;
		OfficialChannel.Setting setting = officialChannel.GetSetting(serverName, out selectedServer);
		setting.OpenInAppBrowser = GUILayout.Toggle(setting.OpenInAppBrowser, "OpenInAppBrowser");
		GUILayout.Label($"ServerName: {serverName} (selected={selectedServer})");
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(setting.Channels); i++)
		{
			OfficialChannel.Channel channel = setting.Channels[i];
			string stringDirect = LocalizationManager.GetStringDirect(channel.Name_Local);
			GUILayout.Label($"[Channel: {channel.Name_Local}={stringDirect}]");
			using (new BlockManager().Horizontal())
			{
				GUILayout.Space(30f);
				using (new BlockManager().Vertical())
				{
					channel.IsEnable = GUILayout.Toggle(channel.IsEnable, "IsEnable");
					using (new BlockManager().Horizontal())
					{
						GUILayout.Label("Url", GUILayout.Width(100f));
						channel.Url = GUILayout.TextField(channel.Url);
					}
					using (new BlockManager().Horizontal())
					{
						GUILayout.Label("Icon", GUILayout.Width(100f));
						channel.Icon = GUILayout.TextField(channel.Icon);
					}
					channel.IsNewVisible = GUILayout.Toggle(channel.IsNewVisible, "IsNewVisible");
				}
			}
		}
	}
}
