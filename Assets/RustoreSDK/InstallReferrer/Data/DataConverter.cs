using UnityEngine;

namespace RuStore.UnityInstallReferrer.Data
{

    public static class DataConverter
    {

        public static InstallReferrer ConvertInstallReferrer(AndroidJavaObject obj)
        {
            if (obj == null) return null;

            var installAppTimestamp = obj.Get<long>("installAppTimestamp");
            var
                packageName = obj.Get<string>("packageName");
            var receivedTimestamp = obj.Get<long>("receivedTimestamp");
            var referrerId = obj.Get<string>("referrerId");
            var versionCode = obj.Get<AndroidJavaObject>("versionCode")?.Call<long>("longValue");
            var utmCampaign = obj.Get<string>("utmCampaign");
            var utmGroup = obj.Get<string>("utmGroup");
            var utmBanner = obj.Get<string>("utmBanner");

            return new InstallReferrer(
                    installAppTimestamp,
                    packageName,
                    receivedTimestamp,
                    referrerId,
                    versionCode,
                    utmCampaign,
                    utmGroup,
                    utmBanner);
        }
    }
}
