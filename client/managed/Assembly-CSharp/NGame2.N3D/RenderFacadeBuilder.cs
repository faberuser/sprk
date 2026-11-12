using System.Collections.Generic;
using NGame2.NRender;
using NGame2.NUI;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.N3D;

public class RenderFacadeBuilder
{
	public Transform ParentTm;

	public bool IgnoreFlip;

	private TeamType _teamType;

	private Bounds _shapeBounds;

	private CreatureStock _assetStock;

	public bool DontDestroyOnLoad
	{
		get
		{
			return _assetStock.DontDestroyOnLoad;
		}
		set
		{
			_assetStock.DontDestroyOnLoad = value;
		}
	}

	public CreatureStock.ELoadLevel LoadLevel
	{
		get
		{
			return _assetStock.LoadLevel;
		}
		set
		{
			_assetStock.LoadLevel = value;
		}
	}

	public string BodyPrefix
	{
		set
		{
			_assetStock.BodyPrefix = value;
		}
	}

	public string WeaponPrefix
	{
		set
		{
			_assetStock.WeaponStock.WeaponPrefix = value;
		}
	}

	public string HairPrefix
	{
		set
		{
			_assetStock.HairPrefix = value;
		}
	}

	public string Code => _assetStock.Code;

	public static RenderFacadeBuilder CreateRenderFacadeBuliderFrom(CachedUserInfo userInfo)
	{
		RenderFacadeBuilder renderFacadeBuilder = null;
		if (new UIAvatarHelper.AvatarInfo(userInfo.AvatarHeroIndex).HeroIndex != userInfo.HeroIndex)
		{
			return UIAvatarHelper.CreateRenderFacadeBuilder(userInfo.AvatarHeroIndex);
		}
		CachedHeroInfo i_this = new CachedHeroInfo(GameRoot.Instance.CreatureDataContainer.GetData(userInfo.HeroIndex));
		PlayerAvatarHeroInfo playerAvatarHeroInfo = userInfo.CreatePlayerAvatarHeroInfo();
		return new RenderFacadeBuilder(null, TeamType.Left, i_this.CreatePlayerAvatarHeroInfo(playerAvatarHeroInfo));
	}

	public RenderFacadeBuilder(Transform parentTm, TeamType teamType, HeroInfo heroInfo)
	{
		ParentTm = parentTm;
		_teamType = teamType;
		if (heroInfo != null)
		{
			if (heroInfo is CachedHeroInfo cachedHeroInfo)
			{
				InitFromHeroInfo(heroInfo, cachedHeroInfo.GetEquipItemInfoList());
			}
			else
			{
				InitFromHeroInfo(heroInfo, null);
			}
		}
	}

	public RenderFacadeBuilder(Transform parentTm, TeamType teamType, HeroInfo heroInfo, IList<EquipItemInfo> itemList)
	{
		ParentTm = parentTm;
		_teamType = teamType;
		if (heroInfo != null)
		{
			InitFromHeroInfo(heroInfo, itemList);
		}
	}

	private void InitFromHeroInfo(HeroInfo heroInfo, IList<EquipItemInfo> itemList)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
		if (data != null)
		{
			float num = (float)data.SizeMm * 0.001f;
			float num2 = (float)data.HeightMm * 0.001f;
			GetHeroModelCode(data, out _assetStock.Code, out var modelPrefix);
			_assetStock.BodyPrefix = modelPrefix;
			_assetStock.WeaponStock.WeaponPrefix = modelPrefix;
			_assetStock.Size = num;
			_shapeBounds = new Bounds(Vector3.up * num2 * 0.5f, new Vector3(num * 2f, num2, num * 2f));
		}
		_assetStock.Star = heroInfo.Star;
		ChangeAssetStockByCostume(heroInfo.CostumeIndex, ref _assetStock);
		ChangeAssetStockByHairCostume(heroInfo.HairCostumeIndex, ref _assetStock);
		ChangeAssetStockByWeaponIndex(heroInfo.WeaponCostumeIndex, ref _assetStock);
		ChangeAssetStockByAccessoryIndex(heroInfo, ref _assetStock);
		if (heroInfo.HideUniqueWeapon == 0)
		{
			ChangeAssetStockByItem(itemList, ref _assetStock);
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(itemList); i++)
		{
			EquipItemInfo equipItemInfo = itemList[i];
			if (equipItemInfo != null && equipItemInfo.IsUniqueWeapon && equipItemInfo.IsSoulWeapon())
			{
				_assetStock.WeaponStock.IsEquipSoulWeapon = true;
				_assetStock.WeaponStock.SoulWeaponGrade = equipItemInfo.SoulWeaponInfo.Grade;
			}
		}
	}

	public RenderFacadeBuilder(Transform parentTm, TeamType teamType, PetInfo petInfo)
	{
		ParentTm = parentTm;
		_teamType = teamType;
		InitFromPetInfo(petInfo);
	}

	private void InitFromPetInfo(PetInfo petInfo)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(petInfo.PetIndex);
		if (data != null)
		{
			float num = (float)data.SizeMm * 0.001f;
			float num2 = (float)data.HeightMm * 0.001f;
			GetHeroModelCode(data, out _assetStock.Code, out var modelPrefix);
			_assetStock.BodyPrefix = modelPrefix;
			_assetStock.WeaponStock.WeaponPrefix = modelPrefix;
			_assetStock.Size = num;
			_shapeBounds = new Bounds(Vector3.up * num2 * 0.5f, new Vector3(num * 2f, num2, num * 2f));
		}
		_assetStock.Star = petInfo.Star;
	}

	public RenderFacadeBuilder(Transform parentTm, TeamType teamType, string code, int star, bool isEquipSoulWeapon = false, int soulWeaponGrade = -1)
	{
		ParentTm = parentTm;
		_teamType = teamType;
		CreatureData dataByCode = GameRoot.Instance.CreatureDataContainer.GetDataByCode(code);
		if (dataByCode != null)
		{
			float num = (float)dataByCode.SizeMm * 0.001f;
			float num2 = (float)dataByCode.HeightMm * 0.001f;
			GetHeroModelCode(dataByCode, out _assetStock.Code, out var modelPrefix);
			_assetStock.BodyPrefix = modelPrefix;
			_assetStock.WeaponStock.WeaponPrefix = modelPrefix;
			_assetStock.Size = num;
			_shapeBounds = new Bounds(Vector3.up * num2 * 0.5f, new Vector3(num * 2f, num2, num * 2f));
		}
		else
		{
			float num3 = 0.5f;
			float num4 = 2f;
			_assetStock.Code = code;
			_assetStock.BodyPrefix = string.Empty;
			_assetStock.WeaponStock.WeaponPrefix = string.Empty;
			_assetStock.Size = num3;
			_shapeBounds = new Bounds(Vector3.up * num4 * 0.5f, new Vector3(num3 * 2f, num4, num3 * 2f));
		}
		_assetStock.WeaponStock.IsEquipSoulWeapon = isEquipSoulWeapon;
		_assetStock.WeaponStock.SoulWeaponGrade = soulWeaponGrade;
		_assetStock.Star = star;
	}

	public RenderFacadeBuilder(Transform parentTm, TeamType teamType, CreatureStock assetStock)
	{
		ParentTm = parentTm;
		_teamType = teamType;
		_assetStock = assetStock;
		CreatureData dataByCode = GameRoot.Instance.CreatureDataContainer.GetDataByCode(_assetStock.Code);
		if (dataByCode != null)
		{
			float num = (float)dataByCode.SizeMm * 0.001f;
			float num2 = (float)dataByCode.HeightMm * 0.001f;
			_shapeBounds = new Bounds(Vector3.up * num2 * 0.5f, new Vector3(num * 2f, num2, num * 2f));
		}
		else
		{
			float num3 = 0.5f;
			float num4 = 2f;
			_shapeBounds = new Bounds(Vector3.up * num4 * 0.5f, new Vector3(num3 * 2f, num4, num3 * 2f));
		}
	}

	public void AddPreload(AnimationKeyData animationKey)
	{
		if (_assetStock.PreloadSet == null)
		{
			_assetStock.PreloadSet = new HashSet<AnimationKeyData>();
		}
		_assetStock.PreloadSet.Add(animationKey);
	}

	public void SetPreload(HashSet<AnimationKeyData> preloadSet)
	{
		_assetStock.PreloadSet = preloadSet;
	}

	public void SetModelPrefix(string modelPrefix)
	{
		if (!String_Extension_Method.IsNullOrEmpty(modelPrefix))
		{
			_assetStock.BodyPrefix = modelPrefix;
			_assetStock.WeaponStock.WeaponPrefix = modelPrefix;
		}
	}

	public void SetModelPrefix(string bodyPrefix, string weaponPrefix)
	{
		if (!String_Extension_Method.IsNullOrEmpty(bodyPrefix))
		{
			_assetStock.BodyPrefix = bodyPrefix;
		}
		if (!String_Extension_Method.IsNullOrEmpty(weaponPrefix))
		{
			_assetStock.WeaponStock.WeaponPrefix = weaponPrefix;
		}
	}

	public void SetWeaponCostume(int weaponCostumeIndex)
	{
		ChangeAssetStockByWeaponIndex(weaponCostumeIndex, ref _assetStock);
	}

	public void SetCostume(int costumeIndex)
	{
		ChangeAssetStockByCostume(costumeIndex, ref _assetStock);
	}

	public void SetAccessory(int accessoryIndex, int heroIndex, PositionInfo pos)
	{
		if (heroIndex != 0 && accessoryIndex != 0)
		{
			ChangeAssetStockByAccessoryIndex(accessoryIndex, heroIndex, pos, ref _assetStock);
		}
	}

	public CreatureStock GetStock()
	{
		return _assetStock;
	}

	public RenderFacade Build()
	{
		if (String_Extension_Method.IsNullOrEmpty(_assetStock.Code))
		{
			return null;
		}
		ICreatureLoad assetCreatureAsync = SingletonAuto<CreatureContainer>.instance.CreateAsync(_assetStock);
		GameObject gameObject = new GameObject("RenderFacade");
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject.transform, ParentTm);
		RenderFacade renderFacade = gameObject.AddComponent<RenderFacade>();
		if (!renderFacade.Open(_teamType, assetCreatureAsync, _shapeBounds, IgnoreFlip))
		{
			ReleaseRenderFacade(renderFacade);
			return null;
		}
		Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(gameObject, ParentTm.gameObject.layer);
		return renderFacade;
	}

	public static void ReleaseRenderFacade(RenderFacade renderFacade)
	{
		if (!(renderFacade == null))
		{
			renderFacade.Close();
			VespaObject.DestroyGo(renderFacade);
		}
	}

	public static void GetHeroModelCode(CreatureData creatureData, out string modelCode, out string modelPrefix)
	{
		if (String_Extension_Method.IsNullOrEmpty(creatureData.ModelCode))
		{
			modelCode = creatureData.CodeName;
		}
		else
		{
			modelCode = creatureData.ModelCode;
		}
		if (String_Extension_Method.IsNullOrEmpty(creatureData.ModelPrefix))
		{
			modelPrefix = string.Empty;
		}
		else
		{
			modelPrefix = creatureData.ModelPrefix;
		}
	}

	private static void ChangeAssetStockByItem(IList<EquipItemInfo> itemList, ref CreatureStock assetStock)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(itemList); i++)
		{
			EquipItemInfo equipItemInfo = itemList[i];
			if (equipItemInfo == null)
			{
				continue;
			}
			ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(equipItemInfo.ItemIndex);
			if (data != null)
			{
				if (!String_Extension_Method.IsNullOrEmpty(data.SubData.BodyPrefix))
				{
					assetStock.BodyPrefix = data.SubData.BodyPrefix;
				}
				if (!String_Extension_Method.IsNullOrEmpty(data.SubData.WeaponPrefix))
				{
					assetStock.WeaponStock.WeaponPrefix = data.SubData.WeaponPrefix;
				}
			}
		}
	}

	private static void ChangeAssetStockByAccessoryIndex(HeroInfo heroInfo, ref CreatureStock assetStock)
	{
		SprkAccessories.Build(heroInfo, ref assetStock);
	}

	private static void ChangeAssetStockByAccessoryIndex(int accessoryIndex, int heroIndex, PositionInfo positionInfo, ref CreatureStock assetStock)
	{
		AccessoryCostumeData data = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(accessoryIndex);
		if (data != null)
		{
			CustomizeStock stock = default(CustomizeStock);
			if (TryAssignCustomizeStock(ref stock, heroIndex, data, positionInfo))
			{
				assetStock.SetCustomizeStock(stock);
			}
		}
	}

	private static PositionInfo GetPositionInfoOf(AccessoryPartType partType, HeroInfo heroInfo)
	{
		if (!(heroInfo is CachedHeroInfo cachedHeroInfo))
		{
			return null;
		}
		return cachedHeroInfo.GetAccessoryPositionInfo(partType);
	}

	private static bool TrySetCustomizeStock(ref CustomizeStock stock, HeroInfo heroInfo, AccessoryPartType partType)
	{
		int heroIndex = heroInfo.HeroIndex;
		int accessoryIndex = heroInfo.GetAccessoryIndex(partType);
		if (heroIndex == 0 || accessoryIndex == 0)
		{
			return false;
		}
		AccessoryCostumeData data = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(accessoryIndex);
		if (data == null)
		{
			VespaDebug.LogError($"accessoryData is null :: AccessoryIndex {accessoryIndex}");
			return false;
		}
		PositionInfo positionInfoOf = GetPositionInfoOf(partType, heroInfo);
		return TryAssignCustomizeStock(ref stock, heroIndex, data, positionInfoOf);
	}

	public static bool TryAssignCustomizeStock(ref CustomizeStock stock, int heroIndex, AccessoryCostumeData accessoryData, PositionInfo positionInfo)
	{
		if (accessoryData == null)
		{
			VespaDebug.LogError("accessoryData == null");
			return false;
		}
		ClientCustomizingPivotData data = GameRoot.Instance.ClientCustomizingPivotDataContainer.GetData(heroIndex, accessoryData.PivotType);
		if (data == null)
		{
			VespaDebug.LogError($"pivotData is null :: accessoryData.PivotType {accessoryData.PivotType}");
			return false;
		}
		stock.Assign(accessoryData);
		stock.Assign(data);
		stock.Assign(positionInfo);
		return stock.IsValid();
	}

	private static void ChangeAssetStockByWeaponIndex(int weaponIndex, ref CreatureStock assetStock)
	{
		if (weaponIndex != 0)
		{
			WeaponCostumeData data = GameRoot.Instance.WeaponCostumeDataContainer.GetData(weaponIndex);
			if (data == null)
			{
				Debug.LogError($"weaponCostumeData is null :: weaponInxex {weaponIndex}");
			}
			else if (!String_Extension_Method.IsNullOrEmpty(data.WeaponPrefix))
			{
				assetStock.WeaponStock.WeaponPrefix = data.WeaponPrefix;
			}
			else if (data.Star > 0)
			{
				assetStock.WeaponStock.WeaponPrefix = string.Empty;
				assetStock.WeaponStock.WeaponStar = data.Star;
			}
		}
	}

	private static void ChangeAssetStockByCostume(int costumeIndex, ref CreatureStock assetStock)
	{
		if (costumeIndex == 0)
		{
			return;
		}
		assetStock.CostumeIndex = costumeIndex;
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex);
		if (data != null)
		{
			if (data.HeroStar != 0)
			{
				assetStock.Star = data.HeroStar;
			}
			if (!String_Extension_Method.IsNullOrEmpty(data.BodyPrefix))
			{
				assetStock.BodyPrefix = data.BodyPrefix;
			}
			if (!String_Extension_Method.IsNullOrEmpty(data.WeaponPrefix))
			{
				assetStock.WeaponStock.WeaponPrefix = data.WeaponPrefix;
			}
		}
	}

	private static void ChangeAssetStockByHairCostume(int hairCostumeIndex, ref CreatureStock assetStock)
	{
		if (hairCostumeIndex != 0)
		{
			HairCostumeData data = GameRoot.Instance.HairCostumeDataContainer.GetData(hairCostumeIndex);
			if (data != null && !String_Extension_Method.IsNullOrEmpty(data.HairPrefix))
			{
				assetStock.HairPrefix = data.HairPrefix;
			}
		}
	}
}
