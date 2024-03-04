public static class AuthorizationPolicyNameRetriever
{
    public static string GetName(string PolicyPrefix, string TenantId)
    {
        return $"{PolicyPrefix}_{TenantId}";
    }
}