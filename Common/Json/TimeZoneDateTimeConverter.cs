using System.Text.Json;
using System.Text.Json.Serialization;

namespace PruebaTecnica.Common.Json;

// Todas las fechas se guardan en UTC; solo al serializar la respuesta se expresan en la zona
// configurada con su desfase (ej. "2026-11-10T11:47:31-03:00"): mismo instante, en hora local.
public class TimeZoneDateTimeConverter(TimeZoneInfo timeZone) : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetDateTimeOffset().UtcDateTime;

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utc = new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc));
        writer.WriteStringValue(TimeZoneInfo.ConvertTime(utc, timeZone));
    }
}
