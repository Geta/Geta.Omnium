using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Geta.Omnium.Culture
{
    public class CultureResolver
    {
        // ReSharper disable once InconsistentNaming
        protected const int CustomCultureLCID = 4096;

        protected readonly IEqualityComparer<string> Comparer;
        protected readonly IDictionary<string, string> Languages;
        protected readonly IDictionary<string, string> TwoLetterCountryMappings;
        protected readonly IDictionary<string, string> ThreeLetterCountryMappings;
        protected readonly IDictionary<CultureInfo, CultureInfo> SpecificCultureMappings;

        public CultureResolver()
        {
            Comparer = StringComparer.InvariantCultureIgnoreCase;
            Languages = new Dictionary<string, string>(Comparer);
            TwoLetterCountryMappings = new Dictionary<string, string>(Comparer);
            ThreeLetterCountryMappings = new Dictionary<string, string>(Comparer);
            SpecificCultureMappings = new Dictionary<CultureInfo, CultureInfo>();

            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                                      .OrderBy(x => x.LCID);

            foreach (var culture in cultures)
            {
                if (culture.IsNeutralCulture)
                    continue;
                if (culture.LCID.Equals(CustomCultureLCID))
                    continue;
                if (!TryGetRegionInfo(culture, out var regionInfo))
                    continue;

                var regionIso2 = regionInfo.TwoLetterISORegionName;
                var regionIso3 = regionInfo.ThreeLetterISORegionName;

                if (!TwoLetterCountryMappings.ContainsKey(regionIso2))
                {
                    TwoLetterCountryMappings.Add(regionIso2, regionIso3);
                }

                if (!ThreeLetterCountryMappings.ContainsKey(regionIso3))
                {
                    ThreeLetterCountryMappings.Add(regionIso3, regionIso2);
                }

                if (!Languages.ContainsKey(regionIso2))
                {
                    Languages.Add(regionIso2, culture.TwoLetterISOLanguageName);
                }

                var parentCultures = GetParentCultures(culture);
                foreach (var parentCulture in parentCultures)
                {
                    if (SpecificCultureMappings.ContainsKey(parentCulture))
                        continue;

                    SpecificCultureMappings.Add(parentCulture, culture);
                }
            }
        }

        public virtual string GetTwoLetterCountryCode(string threeLetterCountryCode)
        {
            if (threeLetterCountryCode == null)
                throw new ArgumentNullException(nameof(threeLetterCountryCode));
            if (threeLetterCountryCode.Length == 2)
                return threeLetterCountryCode;
            if (threeLetterCountryCode.Length != 3)
                throw new ArgumentException($"Input '{threeLetterCountryCode}' has an invalid value", nameof(threeLetterCountryCode));
            if (!ThreeLetterCountryMappings.ContainsKey(threeLetterCountryCode))
                throw new ArgumentException($"Input '{threeLetterCountryCode}' wasn't found in the list of country mappings", nameof(threeLetterCountryCode));

            return ThreeLetterCountryMappings[threeLetterCountryCode];
        }

        public virtual string GetThreeLetterCountryCode(string twoLetterCountryCode)
        {
            if (twoLetterCountryCode == null)
                throw new ArgumentNullException(nameof(twoLetterCountryCode));
            if (twoLetterCountryCode.Length == 3)
                return twoLetterCountryCode;
            if (twoLetterCountryCode.Length != 2)
                throw new ArgumentException($"Input '{twoLetterCountryCode}' has an invalid value", nameof(twoLetterCountryCode));
            if (!TwoLetterCountryMappings.ContainsKey(twoLetterCountryCode))
                throw new ArgumentException($"Input '{twoLetterCountryCode}' wasn't found in the list of country mappings", nameof(twoLetterCountryCode));

            return TwoLetterCountryMappings[twoLetterCountryCode];
        }

        private IEnumerable<CultureInfo> GetParentCultures(CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
                throw new ArgumentNullException(nameof(cultureInfo));

            while (!cultureInfo.Parent.Equals(CultureInfo.InvariantCulture))
            {
                cultureInfo = cultureInfo.Parent;
                yield return cultureInfo;
            }
        }

        private bool TryGetRegionInfo(CultureInfo cultureInfo, out RegionInfo regionInfo)
        {
            regionInfo = null;
            if (cultureInfo == null)
                return false;

            try
            {
                regionInfo = new RegionInfo(cultureInfo.LCID);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
