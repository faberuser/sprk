using LitJson;
using NGameAsset;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NScript;

public class PlayAssetSound : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public string Code = string.Empty;

	public int AddVolume;

	public SystemLanguage Language = SystemLanguage.Unknown;

	private ILocalTimer _localTimer;

	private int _elapsedTick;

	private bool _isEnd;

	private ISound _sound;

	bool IScript.IsEnd => _isEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		_localTimer = localTimer;
		_elapsedTick = 0;
		SoundStock stock = new SoundStock
		{
			Code = Code,
			AddVolume = AddVolume,
			LocalTimer = _localTimer,
			Language = Language
		};
		_sound = SingletonAuto<SoundContainer>.instance.CreateSound(stock);
		_isEnd = false;
	}

	void IScript.Update()
	{
		if (_localTimer == null)
		{
			_isEnd = true;
			return;
		}
		_elapsedTick += _localTimer.deltaTick;
		if (_sound != null)
		{
			_sound.Update();
			if (_sound.IsEnd())
			{
				_isEnd = true;
				SoundContainer.ReleaseSound(_sound);
				_sound = null;
			}
		}
	}

	void IScript.End()
	{
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
		_isEnd = true;
		if (_localTimer != null && _sound != null)
		{
			SoundContainer.ReleaseSound(_sound);
			_sound = null;
		}
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		Code = json["Code"].GETSTRING(Code);
		AddVolume = json["AddVolume"].GETINT(AddVolume);
		Language = json["Language"].GETENUM(Language);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("Code", Code);
		jsonData.AddFieldLong("AddVolume", AddVolume);
		if (Language == SystemLanguage.Korean || Language == SystemLanguage.English || Language == SystemLanguage.Japanese)
		{
			jsonData.AddFieldEnum("Language", Language);
		}
		else
		{
			jsonData.AddFieldEnum("Language", SystemLanguage.Unknown);
		}
		return jsonData;
	}
}
