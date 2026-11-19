using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NGame2.NParty;
using NGame2.NScene;
using NShared;
using NShared.NParty;
using NVespa.NSingleton;
using UnityEngine;

// Discord Rich Presence through Discord's local IPC pipe. It needs no SDK or
// native plugin: the pipe is opened with Win32 calls on a background thread,
// and the game thread only decides what text to show. When Discord is not
// running the pipe is missing, so nothing happens and the game is unaffected.
public sealed class SprkDiscordPresence : MonoBehaviour
{
	private const string ClientId = "1572087957152596120";

	private const float PollSeconds = 2f;

	private static SprkDiscordPresence runner;

	private static int battleChapter;

	private static int battleDungeon;

	private static bool battleKnown;

	private float nextPoll;

	private string shownKey;

	// The elapsed timer counts from game start, not from the current activity.
	private readonly long shownSince =DateTimeOffset.UtcNow.ToUnixTimeSeconds();

	public static void Ensure()
	{
		if (runner != null || Application.platform != RuntimePlatform.WindowsPlayer)
		{
			return;
		}
		// Automated test runs and players who opt out never touch Discord.
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_DIR")) || Environment.GetEnvironmentVariable("SPRK_DISCORD_PRESENCE") == "0")
		{
			return;
		}
		GameObject obj = new GameObject("SprkDiscordPresence");
		UnityEngine.Object.DontDestroyOnLoad(obj);
		runner = obj.AddComponent<SprkDiscordPresence>();
		Pipe.Start(ClientId);
	}

	// The scene type switches to Battle2 as soon as loading starts, but the new
	// battle is only created later during the load. Forget the previous battle
	// here so it is never shown for the new one.
	public static void SceneMoving()
	{
		battleKnown = false;
	}

	public static void BattleCreated(int chapterIndex, int dungeonIndex)
	{
		battleChapter = chapterIndex;
		battleDungeon = dungeonIndex;
		battleKnown = true;
		if (runner != null)
		{
			runner.nextPoll = 0f;
		}
	}

	private void Update()
	{
		if (Time.realtimeSinceStartup < nextPoll)
		{
			return;
		}
		nextPoll = Time.realtimeSinceStartup + PollSeconds;
		try
		{
			Describe(out var details, out var state);
			if (details == null)
			{
				return;
			}
			string key = details + "\n" + state;
			if (key == shownKey)
			{
				return;
			}
			shownKey = key;
			Pipe.SetActivity(BuildActivity(details, state, shownSince));
		}
		catch (Exception)
		{
			// Presence is cosmetic; never let it disturb the game loop.
		}
	}

	private void OnApplicationQuit()
	{
		Pipe.Stop();
	}

	private static void Describe(out string details, out string state)
	{
		details = null;
		state = null;
		if (!GlobalAutoBehaviour<LoadingManager>.isValidInstance)
		{
			return;
		}
		int partySize = PartySize();
		switch (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType)
		{
		case LoadingManager.ESceneType.Logo:
		case LoadingManager.ESceneType.Login:
			details = "At Title Screen";
			break;
		case LoadingManager.ESceneType.Lobby:
			if (partySize > 0)
			{
				details = "In Party Room";
				state = PartyText(partySize);
			}
			else
			{
				details = "In Lobby";
			}
			break;
		case LoadingManager.ESceneType.Battle2:
		{
			if (!battleKnown)
			{
				// Still loading: keep showing the previous activity.
				return;
			}
			CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(battleChapter, battleDungeon);
			BattleType type = data?.BattleType ?? BattleType.None;
			details = ModeName(type);
			if (type == BattleType.Campaign)
			{
				state = "Chapter " + battleChapter + "-" + battleDungeon;
			}
			else if (partySize > 0)
			{
				state = PartyText(partySize);
			}
			break;
		}
		}
	}

	private static int PartySize()
	{
		if (!GlobalAutoBehaviour<PartyManager>.isValidInstance)
		{
			return 0;
		}
		PartyInfo party = GlobalAutoBehaviour<PartyManager>.instance.PartyInfo;
		if (party == null || party.PartyRoom == null)
		{
			return 0;
		}
		return Math.Max(1, party.PartyMembers?.Count ?? party.PartyRoom.CurPartyMember);
	}

	private static string PartyText(int size)
	{
		return size == 1 ? "Waiting for players" : "Party of " + size;
	}

	public static string ModeName(BattleType type)
	{
		switch (type)
		{
		case BattleType.Campaign: return "Campaign";
		case BattleType.CustomCampaign: return "Custom Campaign";
		case BattleType.Trial: return "Trials";
		case BattleType.Arena: return "League of Victory";
		case BattleType.Tower: return "Tower";
		case BattleType.Pit: return "Pit";
		case BattleType.Labyrinth: return "Labyrinth";
		case BattleType.Event: return "Event";
		case BattleType.Party: return "Party Dungeon";
		case BattleType.Boss: return "Boss";
		case BattleType.Raid: return "Dragon Raid";
		case BattleType.OrdealTower: return "Tower of Ordeals";
		case BattleType.ChallengeTower: return "Tower of Challenge";
		case BattleType.WorldBoss: return "World Boss";
		case BattleType.GuildRaid: return "Guild Raid";
		case BattleType.Hideout: return "Hideout";
		case BattleType.EventDungeon: return "Event Dungeon";
		case BattleType.EventWorldBoss: return "Event World Boss";
		case BattleType.ArenaBanPick: return "League of Honor";
		case BattleType.RaidSingle: return "Solo Raid";
		case BattleType.Maze: return "Royal Underground Labyrinth";
		case BattleType.SubStory: return "Side Story";
		case BattleType.SubStoryBoss: return "Side Story Boss";
		case BattleType.ChallengeRaidSingle: return "Solo Challenge Raid";
		case BattleType.GuildSuppress: return "Guild Conquest";
		case BattleType.TreasureHouse: return "Treasure House";
		case BattleType.SquareSideQuest: return "Square Side Quest";
		case BattleType.HardRaid: return "Dragon Raid Hard";
		case BattleType.ChallengeRaid: return "Challenge Raid";
		case BattleType.Conquest: return "Conquest";
		case BattleType.GuildArena: return "Guild Arena";
		case BattleType.FieldRaid: return "Field Raid";
		case BattleType.GatewaytoTrial: return "Gateway to Trial";
		case BattleType.GodkingTrial: return "Trial of the God King";
		case BattleType.Eclipse: return "Eclipse";
		case BattleType.UnderPrison: return "Stockade Vault";
		case BattleType.Shakmeh: return "Shakmeh";
		case BattleType.PurchaseDungeon: return "Central Neighborhood";
		case BattleType.GargoriaRaid: return "Gargoria Raid";
		case BattleType.SeigfriedRaid: return "Seigfried Raid";
		case BattleType.AscalonRaid: return "Ascalon Raid";
		case BattleType.OrdealArena: return "Battlefield of Ordeals";
		case BattleType.LuckyArena: return "Lucky Arena";
		case BattleType.ArcdimRaid: return "ARCDIM Raid";
		case BattleType.ArcdimHardRaid: return "ARCDIM X Hard";
		case BattleType.PunishmentRaid: return "Divine Punishment Raid";
		case BattleType.KarmaRaid: return "Karma Raid";
		default: return "In Battle";
		}
	}

	private static string BuildActivity(string details, string state, long since)
	{
		StringBuilder sb = new StringBuilder();
		sb.Append("{\"details\":").Append(Json(details));
		if (!string.IsNullOrEmpty(state))
		{
			sb.Append(",\"state\":").Append(Json(state));
		}
		sb.Append(",\"timestamps\":{\"start\":").Append(since).Append("}}");
		return sb.ToString();
	}

	private static string Json(string value)
	{
		StringBuilder sb = new StringBuilder("\"");
		foreach (char c in value)
		{
			switch (c)
			{
			case '"': sb.Append("\\\""); break;
			case '\\': sb.Append("\\\\"); break;
			default:
				if (c < ' ')
				{
					sb.Append("\\u").Append(((int)c).ToString("x4"));
				}
				else
				{
					sb.Append(c);
				}
				break;
			}
		}
		return sb.Append('"').ToString();
	}

	// Discord IPC: frames of [int32 opcode][int32 length][UTF-8 JSON] on
	// \\.\pipe\discord-ipc-N. Opcodes: 0 handshake, 1 frame, 2 close, 3 ping, 4 pong.
	private static class Pipe
	{
		private const uint GenericRead = 0x80000000;

		private const uint GenericWrite = 0x40000000;

		private const uint OpenExisting = 3;

		private static readonly IntPtr InvalidHandle = new IntPtr(-1);

		private static readonly object gate = new object();

		private static Thread thread;

		private static volatile bool running;

		private static string clientId;

		private static string pendingActivity;

		private static int pendingVersion;

		private static IntPtr handle = InvalidHandle;

		private static int nonce;

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr CreateFileW(string name, uint access, uint share, IntPtr security, uint creation, uint flags, IntPtr template);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool ReadFile(IntPtr file, byte[] buffer, int count, out int read, IntPtr overlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool WriteFile(IntPtr file, byte[] buffer, int count, out int written, IntPtr overlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool PeekNamedPipe(IntPtr pipe, IntPtr buffer, int size, IntPtr read, out int available, IntPtr left);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool CloseHandle(IntPtr handle);

		public static void Start(string id)
		{
			clientId = id;
			running = true;
			thread = new Thread(Run)
			{
				IsBackground = true,
				Name = "SprkDiscordPresence"
			};
			thread.Start();
		}

		public static void Stop()
		{
			running = false;
			Close();
		}

		public static void SetActivity(string activityJson)
		{
			lock (gate)
			{
				pendingActivity = activityJson;
				pendingVersion++;
			}
		}

		private static void Run()
		{
			int sentVersion = 0;
			while (running)
			{
				try
				{
					if (handle == InvalidHandle)
					{
						if (!Connect())
						{
							Sleep(10000);
							continue;
						}
						sentVersion = 0;
					}
					Drain();
					string activity = null;
					int version;
					lock (gate)
					{
						version = pendingVersion;
						if (version != sentVersion)
						{
							activity = pendingActivity;
						}
					}
					if (activity != null)
					{
						int pid = Process.GetCurrentProcess().Id;
						Write(1, "{\"cmd\":\"SET_ACTIVITY\",\"args\":{\"pid\":" + pid + ",\"activity\":" + activity + "},\"nonce\":\"" + ++nonce + "\"}");
						sentVersion = version;
					}
				}
				catch (Exception)
				{
					// Discord closed or restarted: reconnect later and resend.
					Close();
				}
				Sleep(500);
			}
		}

		private static bool Connect()
		{
			for (int i = 0; i < 10; i++)
			{
				IntPtr h = CreateFileW("\\\\.\\pipe\\discord-ipc-" + i, GenericRead | GenericWrite, 0, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
				if (h == InvalidHandle || h == IntPtr.Zero)
				{
					continue;
				}
				handle = h;
				try
				{
					Write(0, "{\"v\":1,\"client_id\":\"" + clientId + "\"}");
					// Discord answers the handshake with a READY frame, or closes on a bad client id.
					for (int waited = 0; waited < 5000; waited += 100)
					{
						if (Available() >= 8)
						{
							if (ReadFrame() == 1)
							{
								return true;
							}
							break;
						}
						Thread.Sleep(100);
					}
				}
				catch (Exception)
				{
				}
				Close();
			}
			return false;
		}

		// Discord replies to every command; read replies as they arrive so the
		// pipe never fills, without blocking on an empty pipe.
		private static void Drain()
		{
			while (handle != InvalidHandle && Available() >= 8)
			{
				ReadFrame();
			}
		}

		private static int Available()
		{
			if (!PeekNamedPipe(handle, IntPtr.Zero, 0, IntPtr.Zero, out var available, IntPtr.Zero))
			{
				throw new InvalidOperationException("Discord pipe closed");
			}
			return available;
		}

		private static int ReadFrame()
		{
			byte[] header = ReadExactly(8);
			int op = BitConverter.ToInt32(header, 0);
			int length = BitConverter.ToInt32(header, 4);
			if (length < 0 || length > 1 << 20)
			{
				throw new InvalidOperationException("Bad Discord frame");
			}
			byte[] body = ReadExactly(length);
			switch (op)
			{
			case 2:
				throw new InvalidOperationException("Discord closed the connection");
			case 3:
				Write(4, Encoding.UTF8.GetString(body));
				break;
			}
			return op;
		}

		private static byte[] ReadExactly(int count)
		{
			byte[] buffer = new byte[count];
			int offset = 0;
			while (offset < count)
			{
				byte[] chunk = offset == 0 ? buffer : new byte[count - offset];
				if (!ReadFile(handle, chunk, count - offset, out var read, IntPtr.Zero) || read <= 0)
				{
					throw new InvalidOperationException("Discord pipe read failed");
				}
				if (chunk != buffer)
				{
					Buffer.BlockCopy(chunk, 0, buffer, offset, read);
				}
				offset += read;
			}
			return buffer;
		}

		private static void Write(int op, string json)
		{
			byte[] body = Encoding.UTF8.GetBytes(json);
			byte[] frame = new byte[8 + body.Length];
			BitConverter.GetBytes(op).CopyTo(frame, 0);
			BitConverter.GetBytes(body.Length).CopyTo(frame, 4);
			body.CopyTo(frame, 8);
			if (!WriteFile(handle, frame, frame.Length, out var written, IntPtr.Zero) || written != frame.Length)
			{
				throw new InvalidOperationException("Discord pipe write failed");
			}
		}

		private static void Close()
		{
			IntPtr h = handle;
			handle = InvalidHandle;
			if (h != InvalidHandle)
			{
				CloseHandle(h);
			}
		}

		private static void Sleep(int milliseconds)
		{
			for (int i = 0; i < milliseconds && running; i += 250)
			{
				Thread.Sleep(250);
			}
		}
	}
}
