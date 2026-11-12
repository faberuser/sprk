using UnityEngine;

namespace NVespa;

public class ConstantValue
{
	private static bool _initialized;

	private static AnimationCurve _easeIn0_1;

	private static AnimationCurve _easeOut0_1;

	private static AnimationCurve _easeInOut0_1;

	private static AnimationCurve _easeIn1_0;

	private static AnimationCurve _easeOut1_0;

	private static AnimationCurve _easeInOut1_0;

	public static AnimationCurve EaseIn0_1
	{
		get
		{
			Init();
			return _easeIn0_1;
		}
	}

	public static AnimationCurve EaseOut0_1
	{
		get
		{
			Init();
			return _easeOut0_1;
		}
	}

	public static AnimationCurve EaseInOut0_1
	{
		get
		{
			Init();
			return _easeInOut0_1;
		}
	}

	public static AnimationCurve EaseIn1_0
	{
		get
		{
			Init();
			return _easeIn1_0;
		}
	}

	public static AnimationCurve EaseOut1_0
	{
		get
		{
			Init();
			return _easeOut1_0;
		}
	}

	public static AnimationCurve EaseInOut1_0
	{
		get
		{
			Init();
			return _easeInOut1_0;
		}
	}

	private static void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
			_easeInOut0_1 = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
			_easeInOut1_0 = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			_easeIn0_1 = CreateCurve(new Keyframe
			{
				inTangent = 0f,
				outTangent = 0f,
				tangentMode = 0,
				time = 0f,
				value = 0f
			}, new Keyframe
			{
				inTangent = 2f,
				outTangent = 2f,
				tangentMode = 0,
				time = 1f,
				value = 1f
			});
			_easeOut0_1 = CreateCurve(new Keyframe
			{
				inTangent = 2f,
				outTangent = 2f,
				tangentMode = 0,
				time = 0f,
				value = 0f
			}, new Keyframe
			{
				inTangent = 0f,
				outTangent = 0f,
				tangentMode = 0,
				time = 1f,
				value = 1f
			});
			_easeIn1_0 = CreateCurve(new Keyframe
			{
				inTangent = 0f,
				outTangent = 0f,
				tangentMode = 0,
				time = 0f,
				value = 1f
			}, new Keyframe
			{
				inTangent = -2f,
				outTangent = -2f,
				tangentMode = 0,
				time = 1f,
				value = 0f
			});
			_easeOut1_0 = CreateCurve(new Keyframe
			{
				inTangent = -2f,
				outTangent = -2f,
				tangentMode = 0,
				time = 0f,
				value = 1f
			}, new Keyframe
			{
				inTangent = 0f,
				outTangent = 0f,
				tangentMode = 0,
				time = 1f,
				value = 0f
			});
		}
	}

	private static AnimationCurve CreateCurve(params Keyframe[] keys)
	{
		return new AnimationCurve(keys)
		{
			preWrapMode = WrapMode.ClampForever,
			postWrapMode = WrapMode.ClampForever
		};
	}
}
