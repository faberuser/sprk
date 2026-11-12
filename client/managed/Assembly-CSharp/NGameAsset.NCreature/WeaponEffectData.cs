using JsonBin;
using NVespa;

namespace NGameAsset.NCreature;

public class WeaponEffectData : ILoadJson, IWeaponEffectData
{
	public struct WeaponEffectCheckStock
	{
		public static WeaponEffectCheckStock Nullable;

		public EWeaponEffectCheckType Type;

		public int ValueInt;
	}

	public string Code = string.Empty;

	public EWeaponEffectCheckType EffectCheckType = EWeaponEffectCheckType.WeaponStar;

	public int ValueInt;

	public string TargetPivot = string.Empty;

	public TransformData ExtraTransform = new TransformData();

	string IWeaponEffectData.Code => Code;

	string IWeaponEffectData.TargetPivot => TargetPivot;

	TransformData IWeaponEffectData.ExtraTransform => ExtraTransform;

	bool ILoadJson.Load(JsonRead json)
	{
		Code = json["Code"].GETSTRING(Code);
		ValueInt = json["ValueInt"].GETINT(ValueInt);
		TargetPivot = json["TargetPivot"].GETSTRING(TargetPivot);
		EffectCheckType = json["EffectCheckType"].GETENUM(EffectCheckType);
		ExtraTransform.LOADFROMJSON(json["ExtraTransform"]);
		return true;
	}

	public bool EnableEffect(CreatureStock stock)
	{
		if (EffectCheckType == EWeaponEffectCheckType.None)
		{
			return false;
		}
		switch (EffectCheckType)
		{
		case EWeaponEffectCheckType.WeaponGrade:
			if (stock.WeaponStock.IsEquipSoulWeapon)
			{
				return ValueInt == stock.WeaponStock.SoulWeaponGrade;
			}
			return false;
		case EWeaponEffectCheckType.WeaponStar:
			return ValueInt == stock.Star;
		default:
			VespaDebug.LogError("Not Yet Define ESoulWeaponCheckType" + EffectCheckType);
			return false;
		}
	}
}
