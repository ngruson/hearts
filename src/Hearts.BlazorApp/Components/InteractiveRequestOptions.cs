using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Hearts.BlazorApp.Components;

[JsonConverter(typeof(Converter))]
public class InteractiveRequestOptions
{    
    public required InteractionType Interaction { get; init; }

    public required string ReturnUrl { get; init; }

    public IEnumerable<string> Scopes { get; init; } = [];

    public Dictionary<string, object>? AdditionalRequestParameters { get; init; }

    //public bool TryAddAdditionalParameter<[DynamicallyAccessedMembers(LinkerFlags.JsonSerialized)] TValue>(string name, TValue value)
    //{
    //    ArgumentNullException.ThrowIfNull(name, nameof(name));
    //    AdditionalRequestParameters ??= new();
    //    return AdditionalRequestParameters.TryAdd(name, value!);
    //}

    //public bool TryRemoveAdditionalParameter(string name)
    //{
    //    ArgumentNullException.ThrowIfNull(name, nameof(name));
    //    return AdditionalRequestParameters != null && AdditionalRequestParameters.Remove(name);
    //}

    internal string ToState() => JsonSerializer.Serialize(this, InteractiveRequestOptionsSerializerContext.Default.InteractiveRequestOptions);

    internal class Converter : JsonConverter<InteractiveRequestOptions>
    {
        public override InteractiveRequestOptions Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            OptionsRecord requestOptions = JsonSerializer.Deserialize(ref reader, InteractiveRequestOptionsSerializerContext.Default.OptionsRecord);

            return new InteractiveRequestOptions
            {
                AdditionalRequestParameters = requestOptions.AdditionalRequestParameters,
                Interaction = requestOptions.Interaction,
                ReturnUrl = requestOptions.ReturnUrl,
                Scopes = requestOptions.Scopes,
            };
        }

        public override void Write(Utf8JsonWriter writer, InteractiveRequestOptions value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(
                writer,
                new OptionsRecord(value.ReturnUrl, value.Scopes, value.Interaction, value.AdditionalRequestParameters),
                InteractiveRequestOptionsSerializerContext.Default.OptionsRecord);
        }

        internal readonly struct OptionsRecord(
            string returnUrl,
            IEnumerable<string> scopes,
            InteractionType interaction,
            Dictionary<string, object>? additionalRequestParameters)
        {
            [JsonInclude]
            public string ReturnUrl { get; init; } = returnUrl;

            [JsonInclude]
            public IEnumerable<string> Scopes { get; init; } = scopes;

            [JsonInclude]
            public InteractionType Interaction { get; init; } = interaction;

            [JsonInclude]
            public Dictionary<string, object>? AdditionalRequestParameters { get; init; } = additionalRequestParameters;
        }

        static class LinkerFlags
        {
            public const DynamicallyAccessedMemberTypes JsonSerialized = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties;
        }
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = false)]
[JsonSerializable(typeof(InteractiveRequestOptions))]
[JsonSerializable(typeof(InteractiveRequestOptions.Converter.OptionsRecord))]
[JsonSerializable(typeof(JsonElement))]
internal partial class InteractiveRequestOptionsSerializerContext : JsonSerializerContext
{
}
