using System;
using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NScene;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class PlaySoundDirect : ExecuteBase, IKey
{
	public static string ContentsTypeName = "Effect/" + typeof(PlaySoundDirect).Name;

	public ChainCall ChainCall;

	public PrefabResAuto<AudioClip> AudioAuto = new PrefabResAuto<AudioClip>();

	public float Volume = 1f;

	public PlaySoundDirect(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
	}

	public override void Wakeup(IVariable variable)
	{
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		if (!SystemOptionData.Sound_EnableGameSound)
		{
			return variable;
		}
		SingletonUpdater<AudioManager>.instance.Play(AudioAuto.Res, Volume, EAudioMixerFlag.None);
		return variable;
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void ReserveCache(IResourcePreloader preloader)
	{
		Action action = delegate
		{
			AudioAuto.ReservePrefab();
		};
		preloader.AddAction(action, string.Empty);
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		AudioAuto.LOADFROMJSON(json.GetJsonAny("AudioAuto", "Audio"));
		Volume = json["Volume"].GETFLOAT(Volume);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("ChainCall", ChainCall);
		jsonData.AddFieldJsonMake("AudioAuto", AudioAuto);
		jsonData.AddFieldDouble("Volume", Volume);
		return jsonData;
	}
}
