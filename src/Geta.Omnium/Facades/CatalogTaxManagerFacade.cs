using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog.Managers;

namespace Geta.Omnium.Facades
{
    [ServiceConfiguration(typeof(ICatalogTaxManagerFacade))]
    public class CatalogTaxManagerFacade : ICatalogTaxManagerFacade
    {
        public string GetTaxCategoryNameById(int id)
        {
            return CatalogTaxManager.GetTaxCategoryNameById(id);
        }
    }
}
