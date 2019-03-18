using System;
using System.Linq;
using EPiServer.DataAccess;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.Security;
using EPiServer.Web;
using Geta.Omnium.Jobs;

namespace EPiServer.Reference.Commerce.Site.Integrations.Omnium
{
    public class OmniumImportSettings : IOmniumImportSettings
    {
        private readonly IContentRepository _contentRepository;
        private readonly ISiteDefinitionRepository _siteDefinitionRepository;

        public OmniumImportSettings(
            IContentRepository contentRepository,
            ISiteDefinitionRepository siteDefinitionRepository)
        {
            _contentRepository = contentRepository;
            _siteDefinitionRepository = siteDefinitionRepository;
        }

        public void LogSyncFromOmniumDate(DateTime dateTime)
        {
            var settings = GetStartPage().CreateWritableClone() as StartPage;

            settings.LastSyncFromOmniumDate = dateTime;

            _contentRepository.Save(settings, SaveAction.Publish, AccessLevel.NoAccess);
        }

        public DateTime? GetLastSyncFromOmniumDate()
        {
            return GetStartPage()?.LastSyncFromOmniumDate;
        }

        private StartPage GetStartPage()
        {
            var siteDefintion = _siteDefinitionRepository.List().FirstOrDefault();
            if (siteDefintion != null)
            {
                return _contentRepository.Get<StartPage>(siteDefintion.StartPage);
            }
            return null;
        }
    }
}