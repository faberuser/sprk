using NGame2.NAccount;
using NGame2.NUI.NDescription;
using NGame2.NUI.NWindow.NSoulWeapon;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2.NSoulWeapon;

public class SoulWeaponSkillUpgradeComponent : UIScrollItemBehaviour
{
	public static string PrefabPath = "Resources/Prefab2/Component/SoulWeapon/SoulWeaponSkillUpgradeComponent";

	public static PrefabAuto<SoulWeaponSkillUpgradeComponent> PrefabAuto = new PrefabAuto<SoulWeaponSkillUpgradeComponent>(PrefabPath);

	public SoulWeaponGradeInfo SoulWeaponGradeInfo;

	public UILabel TitleLabel;

	public UILabel DescLabel;

	public UISprite EnableBgSprite;

	public UISprite DisableBgSprite;

	public UIWidgetResizer Resizer;

	public void Open(SoulWeaponInfo soulWeaponInfo, int grade)
	{
		Init(soulWeaponInfo, grade);
	}

	private void Init(SoulWeaponInfo soulWeaponInfo, int grade)
	{
		SoulWeaponGradeInfo.Open(grade, soulWeaponInfo?.ReinforceLevel ?? 0, isLocalize: false, soulWeaponInfo.Grade < grade);
		InitTitleLabel(grade);
		InitDescLable(soulWeaponInfo, grade);
		InitBg(soulWeaponInfo, grade);
		if (Resizer != null)
		{
			Resizer.Resize();
		}
	}

	private void InitTitleLabel(int grade)
	{
		string stringDirect = LocalizationManager.GetStringDirect("SOULWEAPON_INFO_UPGRADESTEP", grade);
		UIHelper.SetLabelText(TitleLabel, stringDirect);
	}

	private void InitDescLable(SoulWeaponInfo soulWeaponInfo, int grade)
	{
		UIHelper.SetLabelText(DescLabel, "-");
		if (soulWeaponInfo != null)
		{
			SoulWeaponSkillData soulWeaponSkillData = soulWeaponInfo.GetSoulWeaponSkillData();
			if (soulWeaponSkillData != null)
			{
				int addSkillIndex = soulWeaponSkillData.GetAddSkillIndex(grade);
				int soulWeaponEquipHeroIndex = UIHelper.GetSoulWeaponEquipHeroIndex(soulWeaponInfo);
				CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(soulWeaponEquipHeroIndex);
				string byHeroInfo = SingletonAuto<SkillDescContainer>.instance.GetByHeroInfo(addSkillIndex, heroInfo);
				UIHelper.SetLabelText(DescLabel, byHeroInfo);
			}
		}
	}

	private void InitBg(SoulWeaponInfo soulWeaponInfo, int grade)
	{
		bool flag = soulWeaponInfo.Grade >= grade;
		UIHelper.EnableGameObject(EnableBgSprite, flag);
		UIHelper.EnableGameObject(DisableBgSprite, !flag);
	}
}
