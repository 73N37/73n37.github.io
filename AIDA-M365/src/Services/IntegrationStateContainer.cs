using System;

namespace AIDA.M365.Services;

public sealed class IntegrationStateContainer
{
    private bool _isM365Connected = true;
    private bool _isDynamicsConnected = true;
    private bool _isPostgresConnected = false;

    private string _postgresHost = "104.248.47.45";
    private int _postgresPort = 5432;
    private string _postgresDatabase = "gods_booking_db";
    private string _postgresUsername = "gods_admin";
    private string _postgresPassword = "••••••••••••••••";

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

    private bool _useSupabase = false;
    public bool UseSupabase
    {
        get => _useSupabase;
        set
        {
            if (_useSupabase != value)
            {
                _useSupabase = value;
                NotifyStateChanged();
            }
        }
    }

    private string _supabaseAnonKey = "";
    public string SupabaseAnonKey
    {
        get => _supabaseAnonKey;
        set
        {
            if (_supabaseAnonKey != value)
            {
                _supabaseAnonKey = value;
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
            _cards = value ?? [];
            NotifyStateChanged();
        }
    }

    public event Action? OnChange;

    public void NotifyStateChanged() => OnChange?.Invoke();
}
