using System;
using System.Collections.Generic;

namespace NShared;

public static class SkillTargetType_Extension_Method
{
	private static readonly Dictionary<SkillTargetType, string> _skillTargetTypes = new Dictionary<SkillTargetType, string>();

	private static readonly HashSet<SkillTargetType> _friendlyTargetTypes = new HashSet<SkillTargetType>();

	private static readonly HashSet<SkillTargetType> _randomTargetTypes = new HashSet<SkillTargetType>();

	private static readonly HashSet<SkillTargetType> _excludeMeTypes = new HashSet<SkillTargetType>();

	private static readonly HashSet<SkillTargetType> _checkDeadTargetTypes = new HashSet<SkillTargetType>();

	private static bool _initialized = false;

	private static object _locker = new object();

	private static void Init()
	{
		if (_initialized)
		{
			return;
		}
		lock (_locker)
		{
			if (!_initialized)
			{
				InitTargetTypes();
				_initialized = true;
			}
		}
	}

	private static void InitTargetTypes()
	{
		foreach (SkillTargetType value in Enum.GetValues(typeof(SkillTargetType)))
		{
			string text = value.ToString();
			if (text.EndsWith("Ally") || text.EndsWith("Friend"))
			{
				_friendlyTargetTypes.Add(value);
			}
			_skillTargetTypes[value] = text;
		}
		EnumAttributeContainer<SkillTargetType, SkillTargetAttribute>.ForEach(delegate(SkillTargetType type, SkillTargetAttribute attr)
		{
			if (attr != null && attr.IsFriendly)
			{
				_friendlyTargetTypes.Add(type);
			}
			if (attr != null && attr.IsRandom)
			{
				_randomTargetTypes.Add(type);
			}
			if (attr != null && attr.ExcludeMe)
			{
				_excludeMeTypes.Add(type);
			}
			if (attr != null && attr.CheckDeadTarget)
			{
				_checkDeadTargetTypes.Add(type);
			}
		});
	}

	public static bool IsFriendly(this SkillTargetType type)
	{
		Init();
		return _friendlyTargetTypes.Contains(type);
	}

	public static bool IsHostile(this SkillTargetType type)
	{
		return !type.IsFriendly();
	}

	public static bool IsRandom(this SkillTargetType type)
	{
		Init();
		return _randomTargetTypes.Contains(type);
	}

	public static bool IsExcludeMe(this SkillTargetType type)
	{
		Init();
		return _excludeMeTypes.Contains(type);
	}

	public static bool NeedCheckDeadTarget(this SkillTargetType type)
	{
		Init();
		return _checkDeadTargetTypes.Contains(type);
	}

	public static string GetString(this SkillTargetType type)
	{
		Init();
		return _skillTargetTypes[type];
	}
}
