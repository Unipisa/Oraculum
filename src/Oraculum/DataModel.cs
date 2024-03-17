using OpenAI.Managers;
using OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaviateNET.Modules;
using WeaviateNET;
using Newtonsoft.Json;

namespace Oraculum;

public class Configuration
{
    public static Configuration FromJson(string json)
    {
        return System.Text.Json.JsonSerializer.Deserialize<Configuration>(json)!;
    }

    public OpenAIService CreateOpenAIService()
    {
        if (Provider == ProviderType.OpenAi)
        {
            var opt = LocalProvider == null ? new OpenAiOptions()
            {
                ProviderType = ProviderType.OpenAi,
                ApiKey = OpenAIApiKey!,
                Organization = OpenAIOrgId
            } : new OpenAiOptions()
            {
                BaseDomain = LocalProvider,
                ProviderType = ProviderType.OpenAi,
                ApiKey = OpenAIApiKey!,
                Organization = OpenAIOrgId
            };
            return new OpenAIService(opt);
        }
        else if (Provider == ProviderType.Azure)
        {
            var opt = LocalProvider == null ? new OpenAiOptions()
            {
                ProviderType = ProviderType.Azure,
                ApiKey = AzureOpenAIApiKey!,
                ResourceName = AzureResourceName!,
                DeploymentId = AzureDeploymentId!
            } : new OpenAiOptions()
            {
                BaseDomain = LocalProvider,
                ProviderType = ProviderType.Azure,
                ApiKey = AzureOpenAIApiKey!,
                ResourceName = AzureResourceName!,
                DeploymentId = AzureDeploymentId!
            };
            return new OpenAIService(opt);
        }
        else
            throw new Exception($"Unknown provider {Provider}");
    }

    internal bool IsValid()
    {
        if (WeaviateEndpoint == null)
            return false;
        if (Provider == ProviderType.OpenAi)
            return OpenAIApiKey != null && OpenAIOrgId != null;
        else if (Provider == ProviderType.Azure)
            return AzureOpenAIApiKey != null && AzureResourceName != null && AzureDeploymentId != null;
        else
            return false;
    }

    public string? WeaviateEndpoint { get; set; }
    public string? WeaviateApiKey { get; set; }
    public OpenAI.ProviderType Provider { get; set; } = ProviderType.OpenAi;
    public string? LocalProvider { get; set; } = null;
    public string? OpenAIApiKey { get; set; }
    public string? OpenAIOrgId { get; set; }
    public string? AzureDeploymentId { get; set; }
    public string? AzureResourceName { get; set; }
    public string? AzureOpenAIApiKey { get; set; }
    public string? UserName { get; set; }
}

public class OraculumConfig
{
    public const string ClassName = "OraculumConfig";
    public static readonly Guid ConfigID = Guid.Parse("afc7b344-6c26-4b72-9fbb-0df1acf26c5a");

    public int majorVersion;
    public int minorVersion;
    public DateTime creationDate;
    public int schemaMajorVersion;
    public int schemaMinorVersion;
}

public class GenericObject
{
    [JsonIgnore]
    public const string ClassName = "GenericObject";
    [JsonIgnore]
    public Guid? id { get; set; }
    public string? content { get; set; }
    public DateTime? timestamp { get; set; }
}

[IndexNullState]
public class Fact_1_0
{
    internal const int MajorVersion = 1;
    internal const int MinorVersion = 0;

    [JsonIgnore]
    public const string ClassName = "Facts";

    [JsonIgnore]
    public Guid? id;
    [JsonIgnore]
    public double? distance;

    public string? factType;
    public string? category;
    public string[]? tags;
    public string? title;
    public string? content;
    public string? citation;
    public string? reference;
    public DateTime? expiration;
}

[IndexNullState]
public class Fact_1_1
{
    internal const int MajorVersion = 1;
    internal const int MinorVersion = 1;

    [JsonIgnore]
    public const string ClassName = "Facts";

    [JsonIgnore]
    public Guid? id;
    [JsonIgnore]
    public double? distance;

    public string? factType;
    public string? category;
    public string[]? tags;
    public string? title;
    public string? content;
    public string? citation;
    public string? reference;
    public DateTime? expiration;
    public GeoCoordinates? location;
    public double? locationDistance;
    public string? locationName;
    public string[]? editPrincipals;
    public string? validFrom;
    public string? validTo;
    //public WeaviateRef[]? references;
    public DateTime? factAdded;
}

[IndexNullState]
[OpenAIVectorizerClass(model = "text-embedding-3-large", dimensions = 3072)]
public class Fact
{
    internal const int MajorVersion = 1;
    internal const int MinorVersion = 2;

    [JsonIgnore]
    public const string ClassName = "Facts";

    [JsonIgnore]
    public Guid? id;
    [JsonIgnore]
    public double? distance;

    public string? factType;
    public string? category;
    public string[]? tags;
    public string? title;
    public string? content;
    public string? citation;
    public string? reference;
    public DateTime? expiration;
    public GeoCoordinates? location;
    public double? locationDistance;
    public string? locationName;
    [OpenAIVectorizerProperty(skip = true)]
    public string[]? editPrincipals;
    public string? validFrom;
    public string? validTo;
    //public WeaviateRef[]? references;
    public DateTime? factAdded;
}
