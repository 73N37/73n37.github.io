using System;
using Microsoft.JSInterop;

namespace AIDA.M365.Services;

public sealed class IntegrationStateContainer
{
    private readonly IJSInProcessRuntime? _js;

    private bool _isM365Connected = true;
    private bool _isDynamicsConnected = true;
    private bool _isPostgresConnected = true; // Connected by default now!
    private bool _isDarkMode = false; // Exclusively light cream-linen/gold style by default

    // Default to User's provided Supabase Project URL
    private string _postgresHost = "hrkgvifqbjllhhxzfcfa.supabase.co";
    private int _postgresPort = 5432;
    private string _postgresDatabase = "gods_booking_db";
    private string _postgresUsername = "gods_admin";
    private string _postgresPassword = "••••••••••••••••";

    private bool _useSupabase = true; // Default to true now!
    
    // Default to User's provided Supabase Anon Key
    private string _supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imhya2d2aWZxYmpsbGhoeHpmY2ZhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODAyMTc5OTUsImV4cCI6MjA5NTc5Mzk5NX0.bsuHAXhVUpsqB7JE7fcMmmwJpnBZLwc8Eil-dic_890";

    // e-conomic Billing State
    public int? LastInvoiceNumber { get; set; }
    public string LastPaymentLink { get; set; } = string.Empty;

    public IntegrationStateContainer(IJSRuntime js)
    {
        _js = js as IJSInProcessRuntime;
        
        // 1. Initial Defaults
        _postgresHost = "hrkgvifqbjllhhxzfcfa.supabase.co";
        _supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imhya2d2aWZxYmpsbGhoeHpmY2ZhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3ODAyMTc5OTUsImV4cCI6MjA5NTc5Mzk5NX0.bsuHAXhVUpsqB7JE7fcMmmwJpnBZLwc8Eil-dic_890";
        _useSupabase = true;
        _isPostgresConnected = true;
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

    public bool IsSystemHealthy => IsM365Connected;

    private System.Collections.Generic.List<AIDA.M365.Models.KanbanEventCard> _cards = [];
    public System.Collections.Generic.List<AIDA.M365.Models.KanbanEventCard> Cards
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

    public event Action? OnChange;

    public void NotifyStateChanged() => OnChange?.Invoke();
}
