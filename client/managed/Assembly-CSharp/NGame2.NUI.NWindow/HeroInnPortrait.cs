using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class HeroInnPortrait : MonoBehaviour
{
	public interface Listener
	{
		void OnClickPortrait(int heroIndex);
	}

	public HeroPortraitComponent HeroPortrait;

	public GameObject Obj_Select;

	public GameObject Obj_Have;

	public GameObject Obj_NotHave;

	public int HeroIndex;

	private Listener _listener;

	public void Open(int heroIndex, Listener listener)
	{
		if (heroIndex != 0)
		{
			HeroIndex = heroIndex;
			_listener = listener;
			SetHeroPortrait(heroIndex);
			SetStatus(heroIndex);
		}
	}

	private void SetHeroPortrait(int heroIndex)
	{
		CachedHeroInfo cachedHeroInfo = null;
		if (GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.IsHaveHero(heroIndex, out cachedHeroInfo))
		{
			OpenPortrait(cachedHeroInfo);
			return;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		OpenPortrait(data);
	}

	private void OpenPortrait(CachedHeroInfo heroInfo)
	{
		if (heroInfo != null && !(HeroPortrait == null))
		{
			HeroPortrait.Open(heroInfo, null, null, HeroPortraitComponent.EPortraitMode.Icon);
		}
	}

	private void OpenPortrait(CreatureData creatureData)
	{
		if (creatureData != null && !(HeroPortrait == null))
		{
			HeroPortrait.Open(creatureData, null, HeroPortraitComponent.EPortraitMode.Icon);
		}
	}

	private void SetStatus(int heroIndex)
	{
		if (heroIndex != 0)
		{
			bool isHave = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.IsHaveHero(heroIndex);
			bool isSelect = GlobalAutoBehaviour<AccountManager>.instance.HeroFriendlyManager.IsSelectHero(heroIndex);
			SetStatus(isHave, isSelect);
		}
	}

	private void SetStatus(bool isHave, bool isSelect)
	{
		UIHelper.EnableGameObject(Obj_Have, isHave);
		UIHelper.EnableGameObject(Obj_Select, !isHave && isSelect);
		UIHelper.EnableGameObject(Obj_NotHave, !isHave && !isSelect);
	}

	public void OnClickPortrait()
	{
		_listener.OnClickPortrait(HeroIndex);
	}
}
