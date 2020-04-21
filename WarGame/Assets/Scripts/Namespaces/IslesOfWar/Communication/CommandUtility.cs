using Newtonsoft.Json;

namespace IslesOfWar.Communication
{
    public static class CommandUtility
    {
        static JsonSerializerSettings settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        public static string GetSerializedCommand<T>(T actions)
        {
            return JsonConvert.SerializeObject(actions, Formatting.None, settings);
        }
    }
}
