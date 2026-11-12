using System;
using NGame2;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NContents.NEffect;
using NGame2.NNodeTree.NContents.NExecute;
using NGame2.NNodeTree.NContents.NRoot;
using NGame2.NNodeTree.NObject.NMotionTrail;
using NGame2.NNodeTree.NTypeValue.NModifier;
using NGame2.NNodeTree.NUtil;
using NGame2.NNodeTree.NUtil.NMaterial;
using NGame2.NScene;
using NGame2.NUI;
using NGameAsset;
using NGameAsset.NCreature;
using NGameAsset.NEffect;
using NGameAsset.NSound;
using NShared;
using NVespa.NEnumLookup;
using NVespa.NGui;
using UnityEngine;

namespace NVespa;

public class EnumTools
{
	public static bool IsFlags(Type type)
	{
		return Attribute.GetCustomAttribute(type, typeof(FlagsAttribute)) != null;
	}

	public static bool IsFlags<T>()
	{
		return EnumLookup<T>.IsFlags;
	}

	public static string GetName<T>(T enumValue)
	{
		if (IsFlags<T>())
		{
			return enumValue.ToString();
		}
		EnumLookup<T>.ValueData byEnum = EnumLookup<T>.GetByEnum(enumValue, null);
		if (byEnum != null)
		{
			return byEnum.Name;
		}
		return string.Empty;
	}

	public static bool TryParse<T>(string str, out T o_value)
	{
		return EnumLookup<T>.TryParse(str, out o_value);
	}

	public static T Parse<T>(string str, T defaultT)
	{
		if (!EnumLookup<T>.TryParse(str, out var o_value))
		{
			return defaultT;
		}
		return o_value;
	}

	public static int GetHashCode<T>(T enumValue)
	{
		if (IsFlags<T>())
		{
			return enumValue.GetHashCode();
		}
		return EnumLookup<T>.GetByEnum(enumValue, null)?.HashCode ?? enumValue.GetHashCode();
	}

	public static T IngamePopup<T>(string label, T value, params GUILayoutOption[] options)
	{
		return IngamePopup(label, value, wideLayout: true, null, options);
	}

	public static T IngamePopup<T>(string label, T value, bool wideLayout, params GUILayoutOption[] options)
	{
		return IngamePopup(label, value, wideLayout, null, options);
	}

	public static T IngamePopup<T>(string label, T value, bool wideLayout, GUIStyle style, params GUILayoutOption[] options)
	{
		using (new BlockManager().Horizontal())
		{
			if (!String_Extension_Method.IsNullOrEmpty(label))
			{
				GUILayout.Label(label);
			}
			using (BlockManager blockManager = new BlockManager().Vertical())
			{
				if (wideLayout)
				{
					blockManager.Horizontal();
				}
				EnumLookup<T>.ValueData[] valueArray = EnumLookup<T>.ValueArray;
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(valueArray); i++)
				{
					if (i % 4 == 0 && i > 1 && wideLayout)
					{
						GUILayout.EndHorizontal();
						GUILayout.BeginHorizontal();
					}
					bool flag = object.Equals(valueArray[i].EnumValue, value);
					bool flag2 = ((style != null) ? GUILayout.Toggle(flag, valueArray[i].Name, style, options) : GUILayout.Toggle(flag, valueArray[i].Name, options));
					if (!flag && flag2)
					{
						return valueArray[i].EnumValue;
					}
				}
			}
			return value;
		}
	}

	public static void ResetEnum()
	{
		SingletonAuto<EnumContainer>.instance.ClearEnumLookup();
		EnumLookup<Game3DRoot.ELayerType>.Initialize();
		EnumLookup<GameUIRoot.ELayerType>.Initialize();
		EnumLookup<GlobalUIRoot.ELayerType>.Initialize();
		EnumLookup<ContentsBase.EStopContentsType>.Initialize();
		EnumLookup<EVariableType>.Initialize();
		EnumLookup<CreateAssetSound.ELifetime>.Initialize();
		EnumLookup<CreateModel.ELifetimeType>.Initialize();
		EnumLookup<OnBeginMoveCallback.EDirectionType>.Initialize();
		EnumLookup<NGame2.NNodeTree.NContents.NRoot.OnDamage.EBlockType>.Initialize();
		EnumLookup<NGame2.NNodeTree.NContents.NRoot.OnDamage.ECriticalType>.Initialize();
		EnumLookup<NGame2.NNodeTree.NContents.NRoot.OnDamage.EDamageType>.Initialize();
		EnumLookup<NGame2.NNodeTree.NContents.NRoot.OnDamage.EDodgeType>.Initialize();
		EnumLookup<NGame2.NNodeTree.NContents.NRoot.OnDamage.EDotType>.Initialize();
		EnumLookup<CreateData.EKeyType>.Initialize();
		EnumLookup<TransformModifier.EModifierType>.Initialize();
		EnumLookup<TransformModifier.ERotationType>.Initialize();
		EnumLookup<MaterialData.EUsingFlag>.Initialize();
		EnumLookup<TextureTransformData.EMethodType>.Initialize();
		EnumLookup<LoadingManager.ESceneType>.Initialize();
		EnumLookup<EDepthOrder>.Initialize();
		EnumLookup<DefineCreature.EMotion>.Initialize();
		EnumLookup<DefineCreature.EMotionEnd>.Initialize();
		EnumLookup<EBundleType>.Initialize();
		EnumLookup<EPivotType>.Initialize();
		EnumLookup<ESpecType>.Initialize();
		EnumLookup<CreatureBaseData.ERotationType>.Initialize();
		EnumLookup<CommonData.EAttachMethod>.Initialize();
		EnumLookup<PlaySequenceData.EPlayType>.Initialize();
		EnumLookup<RendererExtraData.CoordinateData.ECoordinateType>.Initialize();
		EnumLookup<ClipData.EPlayType>.Initialize();
		EnumLookup<LoginMethodType>.Initialize();
		EnumLookup<CcMotionType>.Initialize();
		EnumLookup<EPurchaseStatus>.Initialize();
	}
}
