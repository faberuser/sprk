using NGameAsset.NCreature;
using NShared;

namespace NGame2.NUI.NWindow.NCustomizing;

public interface IRenderFacadeController
{
	CachedHeroInfo GetHeroInfo();

	CachedHeroInfo GetLocalHeroInfo();

	void ResetWidgets(WidgetBase butThis);

	void SetupHero(int selectedHeroIndex);

	void ChangeRenderFacadeOnly(CachedHeroInfo heroinfo);

	void ChangeRenderFacadeHair(string hairPrefix);

	void ChangeRenderFacadeWeapon(string weaponPrefix, int star, bool isEquipSoulWeapon, int soulWeaponGrade);

	void ChangeAccessoryCostume(AccessoryPartType partType, AccessoryCostumeData accessoryCostumeData, PositionInfo info = null);

	ITransformInstnace GetAccessoryInstance(AccessoryCostumeData accessoryData);

	void OnSelectCostume();

	void OnCloseDetailView();

	void OnSignatureMotion();
}
