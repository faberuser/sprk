using JsonBin;

public static class __Extension_Method_Creator_from_JsonRead__
{
	public interface ICreator_from_JsonRead<T>
	{
		T CreateFromJson(JsonRead json);
	}

	public class Creator_from_JsonRead_string : ICreator_from_JsonRead<string>
	{
		public static readonly Creator_from_JsonRead_string Default = new Creator_from_JsonRead_string();

		string ICreator_from_JsonRead<string>.CreateFromJson(JsonRead json)
		{
			return json.GETSTRING(string.Empty);
		}
	}

	public class Creator_from_JsonRead_ILoadJson<T> : ICreator_from_JsonRead<T> where T : ILoadJson, new()
	{
		public static readonly Creator_from_JsonRead_ILoadJson<T> Default = new Creator_from_JsonRead_ILoadJson<T>();

		T ICreator_from_JsonRead<T>.CreateFromJson(JsonRead json)
		{
			T result = new T();
			result.Load(json);
			return result;
		}
	}
}
