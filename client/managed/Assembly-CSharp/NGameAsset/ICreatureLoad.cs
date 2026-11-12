using System.Collections.Generic;
using NGameAsset.NCreature;

namespace NGameAsset;

public interface ICreatureLoad : IAsyncLoad<ICreature>, IAsyncLoadBase
{
	Builder Builder { get; }

	CreatureStock Stock { get; }

	void ChangeHairPrefix(string hairPrefix);

	void ChangeStock(CreatureStock stock);

	void ChangeWeaponStock(WeaponStock weaponStock);

	void ChangeCustomizeStock(CustomizeStock costumeStock);

	void OpenFacialModel();

	IAsyncLoadBase LoadExtraAnimation(DefineCreature.EMotion motionType);

	IAsyncLoadBase LoadExtraAnimation(List<AnimationKeyData> keyList);

	IAsyncLoadBase LoadFacialModelAsync();
}
