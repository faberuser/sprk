using System;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class HeroContentsTabComponent : LinkFactoryComponent, InitOnCreate
{
	public interface Listener
	{
		void OnArchiveTypeClick(CreatureTagType tagType);
	}

	[Serializable]
	public class Info
	{
		public UIToggle TagTypeToggle;

		public UILabel TitleLabel;

		public CreatureTagType TagType;
	}

	public const string PrefabPath = "Prefab2/Component/IllustratedBook/HeroContentsTabComponent";

	public static PrefabAuto<HeroContentsTabComponent> PrefabAuto = new PrefabAuto<HeroContentsTabComponent>("Prefab2/Component/IllustratedBook/HeroContentsTabComponent");

	private Listener _Listener;

	public Info info;

	public void Init(Listener listener)
	{
		_Listener = listener;
		if (info.TagType == CreatureTagType.None)
		{
			string text = LocalizationManager.GetString(Hero.HERO_DECK_TOTAL);
			UIHelper.SetLabelText(info.TitleLabel, text);
		}
		else
		{
			UIHelper.SetLabelText(info.TitleLabel, info.TagType);
		}
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public virtual void OnClickToggle()
	{
		if (!(info.TagTypeToggle == null) && info.TagTypeToggle.value && _Listener != null)
		{
			_Listener.OnArchiveTypeClick(info.TagType);
		}
	}

	public void SelectToggle(bool On)
	{
		info.TagTypeToggle.value = On;
	}
}
