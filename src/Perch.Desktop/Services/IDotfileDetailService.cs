using Perch.Desktop.Models;

namespace Perch.Desktop.Services;

public interface IDotfileDetailService
{
    Task<DotfileDetail> LoadDetailAsync(DotfileGroupCardModel group, CancellationToken cancellationToken = default);
}
