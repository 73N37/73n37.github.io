using System;

namespace AIDA.M365.Services;

public sealed class IntegrationStateContainer
{
    private bool _isM365Connected = true;
    private bool _isDynamicsConnected = true;

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

    public bool IsSystemHealthy => IsM365Connected;

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}
