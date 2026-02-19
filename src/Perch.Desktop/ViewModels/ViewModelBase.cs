using CommunityToolkit.Mvvm.ComponentModel;

namespace Perch.Desktop.ViewModels;

public abstract class ViewModelBase : ObservableObject, IDisposable
{
    private bool _disposed;

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
