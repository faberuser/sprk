using LitJson;
using NGame2.NUI.NGame3DWidget;
using NGame2.NUI.NWindow;
using NVespa;

namespace NGame2.NCutScene.NScript;

public class PlayScreenEffect : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public string Code = string.Empty;

	public GameUIRoot.ELayerType EffectLayer = GameUIRoot.ELayerType.CutSceneUI;

	public TransformData ExtraTransform = new TransformData(100f);

	public bool LoopPlay;

	public float LoopEndTick;

	public bool ReleaseImmediately;

	public bool UsePlaybackSpeed;

	public int PlaybackSpeed;

	private ILocalTimer _localTimer;

	private IContents _contents;

	private float _duringEndTick;

	private bool _isEnd;

	bool IScript.IsEnd => _isEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		if (IsSkip())
		{
			_localTimer = null;
			_contents = null;
			_isEnd = true;
		}
		else
		{
			_localTimer = localTimer;
			_contents = ScreenEffect.PlayEffect(Code, ExtraTransform, EffectLayer, GetEffectTimer(), null);
			_isEnd = false;
		}
	}

	private ILocalTimer GetEffectTimer()
	{
		if (!UsePlaybackSpeed)
		{
			return null;
		}
		return new UnityCustomLocalTimer
		{
			PlaybackSpeedFunc = () => PlaybackSpeed
		};
	}

	void IScript.Update()
	{
		if (_localTimer == null)
		{
			_isEnd = true;
		}
		else
		{
			if (_contents == null)
			{
				return;
			}
			if (LoopPlay)
			{
				bool flag = false;
				if (LoopEndTick > 0f)
				{
					_duringEndTick += _localTimer.deltaTick;
				}
				if (_duringEndTick > LoopEndTick)
				{
					flag = true;
				}
				if (flag)
				{
					StopContentsInternal();
					_isEnd = true;
				}
				else if (_contents.GetInstanceCount() == 0)
				{
					StopContentsInternal();
					Vespa_Common_Class_Extension_Method.Interface((IScript)this).Begin(_localTimer, isSkip: false);
				}
			}
			else if (_contents.GetInstanceCount() == 0)
			{
				StopContentsInternal();
				_isEnd = true;
			}
		}
	}

	void IScript.End()
	{
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
		_isEnd = true;
		_duringEndTick = 0f;
		if (_localTimer != null)
		{
			StopContentsInternal();
		}
	}

	private void StopContentsInternal()
	{
		if (_contents != null)
		{
			_contents.Stop(ReleaseImmediately);
			_contents = null;
		}
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		Code = json["Code"].GETSTRING(Code);
		EffectLayer = json["EffectLayer"].GETENUM(EffectLayer);
		ExtraTransform.LOADFROMJSON(json["ExtraTransform"]);
		LoopPlay = json["LoopPlay"].GETBOOLEAN(LoopPlay);
		LoopEndTick = json["LoopEndTick"].GETFLOAT(LoopEndTick);
		ReleaseImmediately = json["ReleaseImmediately"].GETBOOLEAN(ReleaseImmediately);
		UsePlaybackSpeed = json["UsePlaybackSpeed"].GETBOOLEAN(UsePlaybackSpeed);
		PlaybackSpeed = json["PlaybackSpeed"].GETINT(PlaybackSpeed);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("Code", Code);
		jsonData.AddFieldEnum("EffectLayer", EffectLayer);
		jsonData.AddFieldJsonMake("ExtraTransform", ExtraTransform);
		jsonData.AddFieldBool("LoopPlay", LoopPlay);
		jsonData.AddFieldDouble("LoopEndTick", LoopEndTick);
		jsonData.AddFieldBool("ReleaseImmediately", ReleaseImmediately);
		jsonData.AddFieldBool("UsePlaybackSpeed", UsePlaybackSpeed);
		jsonData.AddFieldLong("PlaybackSpeed", PlaybackSpeed);
		return jsonData;
	}

	public static bool IsSkip()
	{
		return false;
	}
}
