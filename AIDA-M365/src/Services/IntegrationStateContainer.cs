using System;
using Microsoft.JSInterop;

namespace AIDA.M365.Services;

/// <summary>
/// A central container managing dynamic integration configurations, authentication states,
/// database host targets, and UI preferences (e.g. Dark Obsidian theme status) for the dashboard.
/// Persists and masked-encrypts key data using browser LocalStorage.
/// </summary>
public sealed class IntegrationStateContainer
{
    private readonly IJSInProcessRuntime? _js;

    private bool _isM365Connected = true;
    private bool _isDynamicsConnected = true;
    private bool _isPostgresConnected = true; // Connected by default
    private bool _isDarkMode = false; // Exclusively light cream-linen/gold style by default

    // Default to Local PostgreSQL / PostgREST
    private string _postgresHost = "localhost";
    private int _postgresPort = 3000;
    private string _postgresDatabase = "gods_booking_db";
    private string _postgresUsername = "postgres";
    private string _postgresPassword = "postgres_password";

    private bool _useSupabase = false; // Default to false for local demo!
    
    // Default to empty for local demo
    private string _supabaseAnonKey = "";

    private string _azureSqlConnectionString = "Server=tcp:gods-sql-server.database.windows.net,1433;Initial Catalog=gods_booking_db;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
    private string _azureKeyVaultUrl = "https://gods-keyvault.vault.azure.net/";

    // Microsoft Entra ID / MSAL credentials
    private string _azureClientId = string.Empty;
    private string _azureTenantId = string.Empty;

    // Azure OpenAI credentials
    private string _azureOpenAiEndpoint = string.Empty;
    private string _azureOpenAiKey = string.Empty;
    private string _azureOpenAiDeployment = "gpt-4o-mini";

    // e-conomic Billing State
    public int? LastInvoiceNumber { get; set; }
    public string LastPaymentLink { get; set; } = string.Empty;

    public IntegrationStateContainer(IJSRuntime js)
    {
        _js = js as IJSInProcessRuntime;
        
        // 1. Initial Defaults - Local PostgreSQL
        _postgresHost = "localhost";
        _supabaseAnonKey = "";
        _useSupabase = false;
        _isPostgresConnected = true;
        _azureSqlConnectionString = "Server=tcp:gods-sql-server.database.windows.net,1433;Initial Catalog=gods_booking_db;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        _azureKeyVaultUrl = "https://gods-keyvault.vault.azure.net/";
        _isDarkMode = false;

        // 2. Override with local storage if saved previously
        LoadFromLocalStorage();
    }

    private string EncryptString(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;
        try
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ 0x5A); // Symmetric XOR masking
            }
            return Convert.ToBase64String(bytes);
        }
        catch { return plainText; }
    }

    private string DecryptString(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;
        try
        {
            byte[] bytes = Convert.FromBase64String(cipherText);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ 0x5A);
            }
            return System.Text.Encoding.UTF8.GetString(bytes);
        }
        catch { return cipherText; }
    }

    private void LoadFromLocalStorage()
    {
        if (_js == null) return;
        try
        {
            var useSupabaseStr = _js.Invoke<string>("localStorage.getItem", "UseSupabase");
            if (!string.IsNullOrEmpty(useSupabaseStr))
            {
                _useSupabase = bool.Parse(useSupabaseStr);
            }

            var supabaseAnonKeyStr = _js.Invoke<string>("localStorage.getItem", "SupabaseAnonKey_Secured");
            if (supabaseAnonKeyStr != null)
            {
                _supabaseAnonKey = DecryptString(supabaseAnonKeyStr);
            }

            var postgresHostStr = _js.Invoke<string>("localStorage.getItem", "PostgresHost_Secured");
            if (postgresHostStr != null)
            {
                _postgresHost = DecryptString(postgresHostStr);
            }

            var azureSqlStr = _js.Invoke<string>("localStorage.getItem", "AzureSqlConnectionString_Secured");
            if (azureSqlStr != null)
            {
                _azureSqlConnectionString = DecryptString(azureSqlStr);
            }

            var keyVaultStr = _js.Invoke<string>("localStorage.getItem", "AzureKeyVaultUrl_Secured");
            if (keyVaultStr != null)
            {
                _azureKeyVaultUrl = DecryptString(keyVaultStr);
            }

            var pgConnectedStr = _js.Invoke<string>("localStorage.getItem", "IsPostgresConnected");
            if (!string.IsNullOrEmpty(pgConnectedStr))
            {
                _isPostgresConnected = bool.Parse(pgConnectedStr);
            }

            var isDarkModeStr = _js.Invoke<string>("localStorage.getItem", "IsDarkMode");
            if (!string.IsNullOrEmpty(isDarkModeStr))
            {
                _isDarkMode = bool.Parse(isDarkModeStr);
            }

            var clientIdStr = _js.Invoke<string>("localStorage.getItem", "AzureClientId_Secured");
            if (clientIdStr != null)
            {
                _azureClientId = DecryptString(clientIdStr);
            }

            var tenantIdStr = _js.Invoke<string>("localStorage.getItem", "AzureTenantId_Secured");
            if (tenantIdStr != null)
            {
                _azureTenantId = DecryptString(tenantIdStr);
            }

            var aoaiEndpointStr = _js.Invoke<string>("localStorage.getItem", "AzureOpenAiEndpoint_Secured");
            if (aoaiEndpointStr != null)
            {
                _azureOpenAiEndpoint = DecryptString(aoaiEndpointStr);
            }

            var aoaiKeyStr = _js.Invoke<string>("localStorage.getItem", "AzureOpenAiKey_Secured");
            if (aoaiKeyStr != null)
            {
                _azureOpenAiKey = DecryptString(aoaiKeyStr);
            }

            var aoaiDeploymentStr = _js.Invoke<string>("localStorage.getItem", "AzureOpenAiDeployment");
            if (!string.IsNullOrEmpty(aoaiDeploymentStr))
            {
                _azureOpenAiDeployment = aoaiDeploymentStr;
            }
        }
        catch
        {
            // Fallback gracefully if localStorage is restricted
        }
    }

    private void SaveToLocalStorage(string key, string value)
    {
        if (_js == null) return;
        try
        {
            _js.InvokeVoid("localStorage.setItem", key, value);
        }
        catch {}
    }

    public bool IsM365Connected
    {
        get => _isM365Connected;
        set
        {
            if (_isM365Connected != value)
            {
                _isM365Connected = value;
                NotifyStateChanged();
            }
        }
    }

    public bool IsDynamicsConnected
    {
        get => _isDynamicsConnected;
        set
        {
            if (_isDynamicsConnected != value)
            {
                _isDynamicsConnected = value;
                NotifyStateChanged();
            }
        }
    }

    public bool IsPostgresConnected
    {
        get => _isPostgresConnected;
        set
        {
            if (_isPostgresConnected != value)
            {
                _isPostgresConnected = value;
                SaveToLocalStorage("IsPostgresConnected", value.ToString());
                NotifyStateChanged();
            }
        }
    }

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (_isDarkMode != value)
            {
                _isDarkMode = value;
                SaveToLocalStorage("IsDarkMode", value.ToString());
                NotifyStateChanged();
            }
        }
    }

    public string PostgresHost
    {
        get => _postgresHost;
        set
        {
            if (_postgresHost != value)
            {
                _postgresHost = value;
                SaveToLocalStorage("PostgresHost_Secured", EncryptString(value));
                NotifyStateChanged();
            }
        }
    }

    public int PostgresPort
    {
        get => _postgresPort;
        set
        {
            if (_postgresPort != value)
            {
                _postgresPort = value;
                NotifyStateChanged();
            }
        }
    }

    public string PostgresDatabase
    {
        get => _postgresDatabase;
        set
        {
            if (_postgresDatabase != value)
            {
                _postgresDatabase = value;
                NotifyStateChanged();
            }
        }
    }

    public string PostgresUsername
    {
        get => _postgresUsername;
        set
        {
            if (_postgresUsername != value)
            {
                _postgresUsername = value;
                NotifyStateChanged();
            }
        }
    }

    public string PostgresPassword
    {
        get => _postgresPassword;
        set
        {
            if (_postgresPassword != value)
            {
                _postgresPassword = value;
                NotifyStateChanged();
            }
        }
    }

    private bool _didLastFetchSucceed = true;
    public bool DidLastFetchSucceed
    {
        get => _didLastFetchSucceed;
        set
        {
            if (_didLastFetchSucceed != value)
            {
                _didLastFetchSucceed = value;
                NotifyStateChanged();
            }
        }
    }

    public bool UseSupabase
    {
        get => _useSupabase;
        set
        {
            if (_useSupabase != value)
            {
                _useSupabase = value;
                SaveToLocalStorage("UseSupabase", value.ToString());
                NotifyStateChanged();
            }
        }
    }

    public string SupabaseAnonKey
    {
        get => _supabaseAnonKey;
        set
        {
            if (_supabaseAnonKey != value)
            {
                _supabaseAnonKey = value;
                SaveToLocalStorage("SupabaseAnonKey_Secured", EncryptString(value));
                NotifyStateChanged();
            }
        }
    }

    public string AzureSqlConnectionString
    {
        get => _azureSqlConnectionString;
        set
        {
            if (_azureSqlConnectionString != value)
            {
                _azureSqlConnectionString = value;
                SaveToLocalStorage("AzureSqlConnectionString_Secured", EncryptString(value));
                NotifyStateChanged();
            }
        }
    }

    public string AzureKeyVaultUrl
    {
        get => _azureKeyVaultUrl;
        set
        {
            if (_azureKeyVaultUrl != value)
            {
                _azureKeyVaultUrl = value;
                SaveToLocalStorage("AzureKeyVaultUrl_Secured", EncryptString(value));
                NotifyStateChanged();
            }
        }
    }

    public string AzureClientId
    {
        get => _azureClientId;
        set
        {
            if (_azureClientId != value)
            {
                _azureClientId = value;
                SaveToLocalStorage("AzureClientId_Secured", EncryptString(value));
                NotifyStateChanged();
            }
        }
    }

    public string AzureTenantId
    {
        get => _azureTenantId;
        set
        {
            if (_azureTenantId != value)
            {
                _azureTenantId = value;
                SaveToLocalStorage("AzureTenantId_Secured", EncryptString(value));
                NotifyStateChanged();
            }
        }
    }

    public string AzureOpenAiEndpoint
    {
        get => _azureOpenAiEndpoint;
        set
        {
            if (_azureOpenAiEndpoint != value)
            {
                _azureOpenAiEndpoint = value;
                SaveToLocalStorage("AzureOpenAiEndpoint_Secured", EncryptString(value));
                NotifyStateChanged();
            }
        }
    }

    public string AzureOpenAiKey
    {
        get => _azureOpenAiKey;
        set
        {
            if (_azureOpenAiKey != value)
            {
                _azureOpenAiKey = value;
                SaveToLocalStorage("AzureOpenAiKey_Secured", EncryptString(value));
                NotifyStateChanged();
            }
        }
    }

    public string AzureOpenAiDeployment
    {
        get => _azureOpenAiDeployment;
        set
        {
            if (_azureOpenAiDeployment != value)
            {
                _azureOpenAiDeployment = value;
                SaveToLocalStorage("AzureOpenAiDeployment", value);
                NotifyStateChanged();
            }
        }
    }

    public bool IsSystemHealthy => IsM365Connected;

    private System.Collections.Generic.List<AIDA.M365.Models.GodsEventCard> _cards = [];
    public System.Collections.Generic.List<AIDA.M365.Models.GodsEventCard> Cards
    {
        get => _cards;
        set
        {
            var newList = value ?? [];
            if (ReferenceEquals(_cards, newList)) return;
            _cards = newList;
            NotifyStateChanged();
        }
    }

    /// <summary>True when the user is logged in with Microsoft and Outlook events are live-synced.</summary>
    private bool _isOutlookConnected = false;
    public bool IsOutlookConnected
    {
        get => _isOutlookConnected;
        set
        {
            if (_isOutlookConnected != value)
            {
                _isOutlookConnected = value;
                NotifyStateChanged();
            }
        }
    }

    /// <summary>Display name of the currently logged-in Microsoft account (e.g. 'Xod Arap · Outlook Live').</summary>
    public string? LoggedInUser { get; set; }

    public event Action? OnChange;

    public void NotifyStateChanged() => OnChange?.Invoke();
}
