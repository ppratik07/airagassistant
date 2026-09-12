namespace EnterpriseChat.Infrastructure.AI;

//Sealed - Nobody is allowed to inherit from this class
public sealed class AzureOpenAIOptions
{
    //When this object is created, start ApiKey with an empty string and others also.
    public string ApiKey { get; set; } = string.Empty;

    public string Endpoint { get; set; } = string.Empty;

    public string Deployment { get; set; } = string.Empty;

    public string ApiVersion { get; set; } = string.Empty;
}
