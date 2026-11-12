using System.Collections.Generic;
using JsonBin;
using UnityEngine;

namespace NGameAsset.NCreature;

public class FacialAnimationData : ILoadJson
{
	public enum EType
	{
		Static,
		KeyAnimation
	}

	public class FacialKey : Tuple<string, string>, ILoadJson
	{
		public string FacialName
		{
			get
			{
				return Item1;
			}
			set
			{
				Item1 = value;
			}
		}

		public string BlendShapeName
		{
			get
			{
				return Item2;
			}
			set
			{
				Item2 = value;
			}
		}

		public FacialKey()
			: base(string.Empty, string.Empty)
		{
		}

		public FacialKey(string facialName, string blendShapeName)
			: base(facialName, blendShapeName)
		{
		}

		bool ILoadJson.Load(JsonRead json)
		{
			FacialName = json["FacialName"].GETSTRING(FacialName);
			BlendShapeName = json["BlendShapeName"].GETSTRING(BlendShapeName);
			return true;
		}
	}

	public class FacialBlendData
	{
		public float BlendTime;

		public AnimationCurve BlendCurve;

		private float _weight;

		public float BlendElapsedTime;

		public float Weight
		{
			get
			{
				_weight = BlendElapsedTime / Vespa_Float_Extension_Method.NoneZero(BlendTime);
				if (BlendCurve != null)
				{
					return BlendCurve.Evaluate(_weight);
				}
				if (_weight > 1f)
				{
					_weight = 1f;
				}
				return _weight;
			}
		}

		public void UpdateTick(float tick)
		{
			BlendElapsedTime += tick;
			if (BlendElapsedTime > BlendTime)
			{
				BlendElapsedTime = BlendTime;
			}
			if (BlendElapsedTime < 0f)
			{
				BlendElapsedTime = 0f;
			}
		}
	}

	public FacialBlendData BlendData;

	public EType Type;

	private List<string> _staticShapeList;

	private Dictionary<FacialKey, AnimationCurve> _keyAnimationDic;

	public List<string> StaticShapeList
	{
		get
		{
			if (_staticShapeList == null)
			{
				_staticShapeList = new List<string>();
			}
			return _staticShapeList;
		}
	}

	public Dictionary<FacialKey, AnimationCurve> KeyAnimationDic
	{
		get
		{
			if (_keyAnimationDic == null)
			{
				_keyAnimationDic = new Dictionary<FacialKey, AnimationCurve>();
			}
			return _keyAnimationDic;
		}
	}

	public bool IsEmpty()
	{
		if (Type == EType.Static)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_staticShapeList); i++)
			{
				if (!String_Extension_Method.IsNullOrEmpty(_staticShapeList[i]))
				{
					return false;
				}
			}
		}
		else if (Type == EType.KeyAnimation && !__Extension_Method_ICollection__.IsNullOrEmpty(_keyAnimationDic))
		{
			return false;
		}
		return true;
	}

	bool ILoadJson.Load(JsonRead json)
	{
		Type = json["Type"].GETENUM(Type);
		if (Type == EType.Static)
		{
			StaticShapeList.LoadListFromJson(json["StaticShapeList"]);
		}
		else if (Type == EType.KeyAnimation)
		{
			KeyAnimationDic.LoadDictionaryFromJson(json["KeyAnimationDic"]);
		}
		return true;
	}
}
