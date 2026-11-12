using LitJson;
using NGame2.N3D;
using NGame2.NAccount;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NCutScene.NScript;

public class CreatureEnter : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public int SlotIndex;

	public string CreatureCode = string.Empty;

	public int Star = 1;

	public string BodyPrefix = string.Empty;

	public string WeaponPrefix = string.Empty;

	public string HairPrefix = string.Empty;

	public bool CreateFromMyHero = true;

	public Vector3 Position;

	public float Scale = 1f;

	public float DirectionAngle;

	public Vector3 Rotation;

	bool IScript.IsEnd => true;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		RenderFacadeBuilder builder = GetBuilder();
		SceneAutoBehaviour<CutSceneManager>.instance.EnterCreature(SlotIndex, builder, Position, Scale, DirectionAngle, Rotation);
	}

	public RenderFacadeBuilder GetBuilder()
	{
		RenderFacadeBuilder renderFacadeBuilder = null;
		if (SprkUsePlayerCostume() && !IsWearLegendCostume(CreatureCode))
		{
			renderFacadeBuilder = GetBuilderFromMyHero(CreatureCode);
		}
		if (renderFacadeBuilder == null)
		{
			renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, CreatureCode, Star);
			renderFacadeBuilder.BodyPrefix = BodyPrefix;
			renderFacadeBuilder.WeaponPrefix = WeaponPrefix;
			renderFacadeBuilder.HairPrefix = HairPrefix;
		}
		renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
		return renderFacadeBuilder;
	}

	public static RenderFacadeBuilder GetBuilderFromMyHero(string creatureCode)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return null;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager == null)
		{
			return null;
		}
		CreatureData dataByCode = GameRoot.Instance.CreatureDataContainer.GetDataByCode(creatureCode);
		if (dataByCode == null)
		{
			return null;
		}
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(dataByCode.Index);
		if (cachedHeroInfo == null)
		{
			return null;
		}
		if (!SystemOptionData.Costume_AllowCutSceneAccessory)
		{
			cachedHeroInfo = CreateUnsetAccessoryHeroInfo(cachedHeroInfo);
		}
		return new RenderFacadeBuilder(null, TeamType.Left, cachedHeroInfo);
	}

	public static bool IsWearLegendCostume(string creatureCode)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.HeroManager == null)
		{
			return false;
		}
		CreatureData dataByCode = GameRoot.Instance.CreatureDataContainer.GetDataByCode(creatureCode);
		if (dataByCode == null)
		{
			return false;
		}
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(dataByCode.Index);
		if (cachedHeroInfo == null)
		{
			return false;
		}
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(cachedHeroInfo.CostumeIndex);
		if (data == null)
		{
			return false;
		}
		if (data.CostumeType != EHeroCostumeType.Legend)
		{
			return false;
		}
		return true;
	}

	private static CachedHeroInfo CreateUnsetAccessoryHeroInfo(CachedHeroInfo heroInfo)
	{
		CachedHeroInfo cachedHeroInfo = JsonMarshaler.Clone(heroInfo);
		cachedHeroInfo.ClearAccessoryCostumes();
		return cachedHeroInfo;
	}

	void IScript.Update()
	{
	}

	void IScript.End()
	{
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		SlotIndex = json["SlotIndex"].GETINT(SlotIndex);
		CreatureCode = json["CreatureCode"].GETSTRING(CreatureCode);
		Star = json["Star"].GETINT(Star);
		BodyPrefix = json["BodyPrefix"].GETSTRING(BodyPrefix);
		WeaponPrefix = json["WeaponPrefix"].GETSTRING(WeaponPrefix);
		HairPrefix = json["HairPrefix"].GETSTRING(HairPrefix);
		CreateFromMyHero = json["CreateFromMyHero"].GETBOOLEAN(CreateFromMyHero);
		Position = json["Position"].GETVECTOR3(Position);
		Scale = json["Scale"].GETFLOAT(Scale);
		DirectionAngle = json["DirectionAngle"].GETFLOAT(DirectionAngle);
		Rotation = json["Rotation"].GETVECTOR3(Rotation);
		if (json.Contains("Prefix"))
		{
			HairPrefix = (WeaponPrefix = (BodyPrefix = json["Prefix"].GETSTRING(string.Empty)));
		}
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldLong("SlotIndex", SlotIndex);
		jsonData.AddFieldString("CreatureCode", CreatureCode);
		jsonData.AddFieldLong("Star", Star);
		jsonData.AddFieldString("BodyPrefix", BodyPrefix);
		jsonData.AddFieldString("WeaponPrefix", WeaponPrefix);
		jsonData.AddFieldString("HairPrefix", HairPrefix);
		jsonData.AddFieldBool("CreateFromMyHero", CreateFromMyHero);
		jsonData.AddFieldVector3("Position", Position);
		jsonData.AddFieldDouble("Scale", Scale);
		jsonData.AddFieldDouble("DirectionAngle", DirectionAngle);
		jsonData.AddFieldVector3("Rotation", Rotation);
		return jsonData;
	}

	private bool SprkUsePlayerCostume()
	{
		if (!CreateFromMyHero)
		{
			if (!string.IsNullOrEmpty(BodyPrefix) || !string.IsNullOrEmpty(HairPrefix))
			{
				return false;
			}
			if (!string.IsNullOrEmpty(WeaponPrefix))
			{
				return WeaponPrefix == "Unique01";
			}
		}
		return true;
	}
}
