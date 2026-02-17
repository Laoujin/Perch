using Perch.Desktop.Models;

namespace Perch.Desktop.Services;

public interface IDotfileDetailService
{
    Task<DotfileDetail> LoadDetailAsync(DotfileCardModel card, CancellationToken cancellationToken = default);
}
