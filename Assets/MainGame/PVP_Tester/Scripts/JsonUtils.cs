
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using WebConnector;

namespace Utils {
    /// <summary>
    /// JSON 파서
    /// </summary>
    public class JsonUtils
    {
        private static TimestampDateTimeConverter timeConverter = new TimestampDateTimeConverter();
        private static EnumConverter enumConverter = new EnumConverter();
        private static BHAssetConverter bhAssetConverter = new BHAssetConverter();

        /// <summary>
        /// Json String to Object with TimestampDateTimeConverter and EnumConverter
        /// </summary>
        public static T Deserialize<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content, bhAssetConverter, timeConverter, enumConverter);
        }

        public static string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }

    public class TimestampDateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value != null)
            {
                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return dt.AddMilliseconds(Convert.ToDouble(reader.Value)).ToLocalTime();
            }

            return DateTime.MinValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumConverter : StringEnumConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Enum.Parse(objectType, (reader.Value != null) ? Convert.ToString(reader.Value) : "0");
        }
    }

    public class BHAssetConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(BHAsset);
        }
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            if (reader.Value != null) {
                return BHAsset.parseBHAssetExpr(Convert.ToString(reader.Value));
            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}

public class BHAsset
{
    public static object parseBHAssetExpr(string str)
    {
        return null;
    }
}