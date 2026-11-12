using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NTutorial;
using NShared;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NTutorial.NRuntime;

public class GetNewHero : ITutorialRuntime, IEventRuntime
{
	bool ITutorialRuntime.OnEvent(IEvent triggerEvent)
	{
		return false;
	}

	IEnumerator ITutorialRuntime.Run(TutorialWindow window, TutorialData tutorialData)
	{
		int argInt = TutorialUtility.GetArgInt(tutorialData, 0);
		int argInt2 = TutorialUtility.GetArgInt(tutorialData, 1);
		TutorialUtility.GetArgInt(tutorialData, 2);
		int argInt3 = TutorialUtility.GetArgInt(tutorialData, 3);
		int argInt4 = TutorialUtility.GetArgInt(tutorialData, 4);
		bool argBool = TutorialUtility.GetArgBool(tutorialData, 5);
		if (GameRoot.Instance.CreatureDataContainer.GetData(argInt) == null)
		{
			VespaDebug.LogError("Creature data not found : " + argInt);
			yield break;
		}
		GetNewHero2 getNewHeroWindow = GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<GetNewHero2>();
		if (getNewHeroWindow == null)
		{
			VespaDebug.LogError("GetNewHero window not found");
			yield break;
		}
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(argInt);
		if (cachedHeroInfo != null)
		{
			cachedHeroInfo.Star = (sbyte)((argInt2 > 0) ? argInt2 : cachedHeroInfo.Star);
			cachedHeroInfo.Level = (short)((argInt3 > 0) ? argInt3 : cachedHeroInfo.Level);
			cachedHeroInfo.Exp = ((argInt4 > 0) ? argInt4 : cachedHeroInfo.Exp);
		}
		else
		{
			cachedHeroInfo = new CachedHeroInfo(new HeroInfo
			{
				Star = (sbyte)argInt2,
				Level = (short)argInt3,
				Exp = argInt4,
				SkillLevel1 = 1
			}, GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager);
			GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Add(cachedHeroInfo);
		}
		bool completeOpen = false;
		GetNewHero2.EOpenMode openMode = ((!argBool) ? GetNewHero2.EOpenMode.NewHero : GetNewHero2.EOpenMode.Awaken);
		GetNewHero2.OpenData openData = GetNewHero2.OpenData.CreateSingleFromHeroInfo(cachedHeroInfo, openMode, hideInfo: false);
		getNewHeroWindow.Open(openData, delegate
		{
			completeOpen = true;
		}, null);
		while (!completeOpen)
		{
			yield return null;
		}
		do
		{
			yield return null;
		}
		while (!(getNewHeroWindow == null) && getNewHeroWindow.IsOpen());
	}
}
