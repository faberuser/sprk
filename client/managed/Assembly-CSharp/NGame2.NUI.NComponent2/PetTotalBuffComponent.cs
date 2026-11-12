using NGame2.NPet;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class PetTotalBuffComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Pet/PetTotalBuffComponent";

	public static PrefabAuto<PetTotalBuffComponent> PrefabAuto = new PrefabAuto<PetTotalBuffComponent>("Prefab2/Component/Pet/PetTotalBuffComponent");

	public UILabel LabelName;

	public UILabel LabelValue;

	public UISprite SpriteNew;

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
	}

	public void Open<T>(T buffData)
	{
		bool notice = false;
		string optionName = string.Empty;
		string optionValue = string.Empty;
		if (buffData is DefaultExtraStatData)
		{
			if (!(buffData is DefaultExtraStatData defaultExtraStatData))
			{
				return;
			}
			notice = GlobalAutoBehaviour<PetManager>.instance.IsChangedPetStat(defaultExtraStatData.OptionType);
			GetDefaultStatDesc(defaultExtraStatData, out optionName, out optionValue);
		}
		else if (buffData is PetStatDataBase)
		{
			if (!(buffData is PetStatDataBase petStatDataBase))
			{
				return;
			}
			notice = GlobalAutoBehaviour<PetManager>.instance.IsChangedPetContentsStat(petStatDataBase.Index);
			GetPetContentsStatDesc(petStatDataBase, out optionName, out optionValue);
		}
		else if (buffData is PetBonusDataBase)
		{
			if (!(buffData is PetBonusDataBase petBonusDataBase))
			{
				return;
			}
			notice = GlobalAutoBehaviour<PetManager>.instance.IsChangedPetBonus(petBonusDataBase.BonusIndex);
			GetPetBonusDesc(petBonusDataBase, out optionName, out optionValue);
		}
		UIHelper.SetLabelText(LabelName, optionName);
		UIHelper.SetLabelText(LabelValue, optionValue);
		SetNotice(notice);
	}

	public void SetNotice(bool show)
	{
		UIHelper.EnableGameObject(SpriteNew, show);
	}

	private void GetDefaultStatDesc(DefaultExtraStatData statDataBase, out string optionName, out string optionValue)
	{
		optionName = LocalizationManager.GetString(statDataBase.OptionType);
		optionValue = StatData.GetStatDisplayValue(statDataBase.OptionType, statDataBase.OptionValue, showSign: true, isCommaString: true);
	}

	private void GetPetContentsStatDesc(PetStatDataBase statDataBase, out string optionName, out string optionValue)
	{
		PetOptionData data = GameRoot.Instance.PetOptionDataContainer.GetData(statDataBase.Index);
		if (data == null)
		{
			optionName = string.Empty;
			optionValue = string.Empty;
			return;
		}
		string text = LocalizationManager.GetString(statDataBase.OptionType);
		optionName = LocalizationManager.GetStringDirect(data.ContentsOptionDescKey, text);
		optionValue = StatData.GetStatDisplayValue(statDataBase.OptionType, statDataBase.OptionValue, showSign: true, isCommaString: true);
	}

	private void GetPetBonusDesc(PetBonusDataBase bonusDataBase, out string optionName, out string optionValue)
	{
		PetBonusData data = GameRoot.Instance.PetBonusDataContainer.GetData(bonusDataBase.BonusIndex);
		if (data == null)
		{
			optionName = string.Empty;
			optionValue = string.Empty;
		}
		else
		{
			optionName = LocalizationManager.GetStringDirect(data.BonusNameKey);
			optionValue = LocalizationManager.GetStringDirect(data.BonusDescKey, bonusDataBase.BonusValue);
		}
	}
}
