using System;

namespace Geta.Omnium.Jobs
{
    public interface IOmniumImportSettings
    {
        void LogSyncFromOmniumDate(DateTime dateTime);
        DateTime? GetLastSyncFromOmniumDate();
    }
}