using System;
using System.Text;
using UnityEngine;

public class PlayerIdentityService
{
    private readonly string _clientToken;

    public string ClientToken => _clientToken;

    public PlayerIdentityService()
    {
        _clientToken = Guid.NewGuid().ToString("N");
    }

    public byte[] GetConnectionToken()
    {
        return Encoding.UTF8.GetBytes(_clientToken);
    }

    public bool TryDecodeToken(byte[] rawToken, out string token)
    {
        token = string.Empty;

        if (rawToken == null || rawToken.Length == 0)
            return false;

        try
        {
            token = Encoding.UTF8.GetString(rawToken).Trim();
            return !string.IsNullOrWhiteSpace(token);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Access] Failed to decode connection token: {e.Message}");
            token = string.Empty;
            return false;
        }
    }
}
