using Perch.Desktop.Models;

namespace Perch.Desktop.Services;

public interface IAppDetailService
{
    Task<AppDetail> LoadDetailAsync(AppCardModel card, CancellationToken cancellationToken = default);
}
