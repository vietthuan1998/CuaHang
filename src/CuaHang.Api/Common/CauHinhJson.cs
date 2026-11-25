using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CuaHang.Api.Common;

public static class CauHinhJson
{
    public static void ApDung(JsonSerializerSettings settings)
    {
        settings.NullValueHandling = NullValueHandling.Ignore;
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        settings.DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
        settings.ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        };
    }

    public static JsonSerializerSettings TaoSerializerSettings()
    {
        var settings = new JsonSerializerSettings();
        ApDung(settings);
        return settings;
    }
}
