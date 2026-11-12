using System;
using System.Collections.Generic;
using JsonBin;
using LitJson;
using NVespa;

namespace NGameAsset.NCreature;

public struct AnimationKeyData
{
	public class Comparer : IEqualityComparer<AnimationKeyData>
	{
		public static Comparer Default = new Comparer();

		public bool Equals(AnimationKeyData x, AnimationKeyData y)
		{
			if (x.MotionType == y.MotionType)
			{
				return string.Equals(x.Name, y.Name);
			}
			return false;
		}

		public int GetHashCode(AnimationKeyData obj)
		{
			return obj.HashCode;
		}
	}

	public static readonly HashSet<AnimationKeyData> EmptyPreloadSet = new HashSet<AnimationKeyData>();

	public DefineCreature.EMotion MotionType { get; private set; }

	public string Name { get; private set; }

	public string PlayName { get; private set; }

	public int HashCode { get; private set; }

	public AnimationKeyData(DefineCreature.EMotion motionType, string name)
	{
		this = default(AnimationKeyData);
		MotionType = motionType;
		if (MotionType >= DefineCreature.EMotion.NamedAction)
		{
			Name = name;
			PlayName = Name;
		}
		else
		{
			Name = string.Empty;
			PlayName = EnumTools.GetName(motionType);
		}
		HashCode = EnumTools.GetHashCode(MotionType);
		if (!String_Extension_Method.IsNullOrEmpty(Name))
		{
			HashCode ^= Name.GetHashCode();
		}
	}

	public override int GetHashCode()
	{
		return HashCode;
	}

	public static AnimationKeyData CreateMotionType(DefineCreature.EMotion motionType)
	{
		return new AnimationKeyData(motionType, string.Empty);
	}

	public static AnimationKeyData CreateEmotiMotion(string name)
	{
		return new AnimationKeyData(DefineCreature.EMotion.EmotiMotion, name);
	}

	public static AnimationKeyData CreateNamedAction(string name)
	{
		return new AnimationKeyData(DefineCreature.EMotion.NamedAction, name);
	}

	public static AnimationKeyData CreateFromJson(JsonRead json)
	{
		if (!json.IsObject())
		{
			return default(AnimationKeyData);
		}
		DefineCreature.EMotion eMotion = json["MotionType"].GETENUM(DefineCreature.EMotion.BattleIdle);
		string text = string.Empty;
		if (eMotion >= DefineCreature.EMotion.NamedAction)
		{
			text = json["Name"].GETSTRING(text);
		}
		if (eMotion == DefineCreature.EMotion.NamedAction && string.Equals(text, "Down", StringComparison.OrdinalIgnoreCase))
		{
			eMotion = DefineCreature.EMotion.Down;
			text = string.Empty;
		}
		else if (eMotion == DefineCreature.EMotion.NamedAction && string.Equals(text, "Getup", StringComparison.OrdinalIgnoreCase))
		{
			eMotion = DefineCreature.EMotion.Getup;
			text = string.Empty;
		}
		return new AnimationKeyData(eMotion, text);
	}

	public static AnimationKeyData CreateFromJson(JsonData json)
	{
		if (!json.IsObject())
		{
			return default(AnimationKeyData);
		}
		DefineCreature.EMotion eMotion = json["MotionType"].GETENUM(DefineCreature.EMotion.BattleIdle);
		string text = string.Empty;
		if (eMotion >= DefineCreature.EMotion.NamedAction)
		{
			text = json["Name"].GETSTRING(text);
		}
		if (eMotion == DefineCreature.EMotion.NamedAction && string.Equals(text, "Down", StringComparison.OrdinalIgnoreCase))
		{
			eMotion = DefineCreature.EMotion.Down;
			text = string.Empty;
		}
		else if (eMotion == DefineCreature.EMotion.NamedAction && string.Equals(text, "Getup", StringComparison.OrdinalIgnoreCase))
		{
			eMotion = DefineCreature.EMotion.Getup;
			text = string.Empty;
		}
		return new AnimationKeyData(eMotion, text);
	}

	public static JsonData MakeJson(AnimationKeyData key)
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldEnum("MotionType", key.MotionType);
		if (key.MotionType >= DefineCreature.EMotion.NamedAction)
		{
			jsonData.AddFieldString("Name", key.Name);
		}
		return jsonData;
	}

	public JsonData MakeJson()
	{
		return MakeJson(this);
	}
}
