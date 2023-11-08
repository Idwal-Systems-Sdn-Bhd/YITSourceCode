using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YIT._DataAccess.Services
{
    public static class SessionExtension
    {
        public static void SetJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value, (Formatting)ReferenceLoopHandling.Ignore));
        }
        public static T? GetJson<T>(this ISession session, string key)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Formatting = Formatting.None,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Converters = new List<JsonConverter> { new DecimalConverter() }
            };
            var sessionData = session.GetString(key);
            return sessionData == null
            ? default(T) : JsonConvert.DeserializeObject<T>(sessionData, settings);
        }

        class DecimalConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(decimal) || objectType == typeof(decimal?));
            }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                JToken token = JToken.Load(reader);
                if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
                {
                    return token.ToObject<decimal>();
                }
                if (token.Type == JTokenType.String)
                {
                    // customize this to suit your needs
                    return Decimal.Parse(token.ToString(),
                           System.Globalization.CultureInfo.GetCultureInfo("es-ES"));
                }
                if (token.Type == JTokenType.Null && objectType == typeof(decimal?))
                {
                    return null;
                }
                throw new JsonSerializationException("Unexpected token type: " +
                                                      token.Type.ToString());
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
