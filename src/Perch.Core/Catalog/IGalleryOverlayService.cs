using Perch.Core.Modules;

namespace Perch.Core.Catalog;

public interface IGalleryOverlayService
{
    AppManifest Merge(AppManifest manifest, CatalogEntry gallery);
}
