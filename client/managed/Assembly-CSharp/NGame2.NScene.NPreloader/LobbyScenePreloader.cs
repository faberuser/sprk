using NGame2.NAccount;
using NGame2.NHelper;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWorldMap.NMapLayout;
using NGameAsset;
using NGameAsset.NEffect;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NScene.NPreloader;

public class LobbyScenePreloader : RootPreloader
{
	public override void Begin()
	{
		base.Begin();
		CreaturePreloadType = EAsyncPreloadType.Normal;
		EffectPreloadType = EAsyncPreloadType.Delay;
		SoundPreloadType = EAsyncPreloadType.Delay;
		LoadLobbyBgm();
		AddUiPreload(SceneAutoBehaviour<LobbyManagement>.instance.ReserveCache, "LobbyManagement.instance.ReserveCache");
		MapNode.ReserveCache(base.Preloader);
		base.Preloader.ExecuteAction(SoundPreloadType, delegate
		{
			base.Preloader.AddAsyncLoadList(AssetSound.ReserveItemSound());
		});
	}

	private void LoadLobbyBgm()
	{
		string text = string.Empty;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(LobbyScene2.ListReturnState); i++)
		{
			LobbyScene2.IReturnState returnState = LobbyScene2.ListReturnState[i];
			if (returnState != null)
			{
				string bgmCode = returnState.BgmCode;
				if (!String_Extension_Method.IsNullOrEmpty(bgmCode))
				{
					text = bgmCode;
				}
			}
		}
		if (!String_Extension_Method.IsNullOrEmpty(text))
		{
			SingletonAuto<BgmManager2>.instance.Request(text, isEvent: true);
		}
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data != null && !String_Extension_Method.IsNullOrEmpty(data.BgmCode))
		{
			SingletonAuto<BgmManager2>.instance.Request(data.BgmCode, isEvent: false);
		}
	}

	private void AddAssetEffect(string code)
	{
		Builder builder = SingletonAuto<BuilderContainer>.instance.Get(code);
		if (builder != null)
		{
			base.Preloader.ExecuteAction(EffectPreloadType, delegate
			{
				base.Preloader.AddAsyncLoad(builder.CreateOpenData());
			});
		}
	}
}
