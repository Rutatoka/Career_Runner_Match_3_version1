#nullable enable

namespace RuStore.UnityInstallReferrer {

    /// <summary>
    /// Класс, представляющий информацию о реферере установки приложения из RuStore.
    /// </summary>
    public class InstallReferrer {

        /// <summary>
        /// Время установки приложения в виде метки времени.
        /// </summary>
        public long installAppTimestamp { get; }

        /// <summary>
        /// Имя пакета приложения.
        /// </summary>
        public string packageName { get; }

        /// <summary>
        /// Время получения данных о реферере в виде метки времени.
        /// </summary>
        public long receivedTimestamp { get; }

        /// <summary>
        /// Идентификатор реферера.
        /// </summary>
        public string referrerId { get; }

        /// <summary>
        /// Версия кода приложения, если доступна.
        /// </summary>
        public long? versionCode { get; }

        /// <summary>
        /// Название кампании.
        /// </summary>
        public string? utmCampaign { get; }

        /// <summary>
        /// Название группы объявлений.
        /// </summary>
        public string? utmGroup { get; }

        /// <summary>
        /// Идентификатор баннера/объявления.
        /// </summary>
        public string? utmBanner { get; }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="InstallReferrer"/>.
        /// </summary>
        /// <param name="installAppTimestamp">Время установки приложения.</param>
        /// <param name="packageName">Имя пакета приложения.</param>
        /// <param name="receivedTimestamp">Время получения данных о реферере.</param>
        /// <param name="referrerId">Идентификатор реферера.</param>
        /// <param name="versionCode">Версия кода приложения.</param>
        /// <param name="utmCampaign">Название кампании.</param>
        /// <param name="utmGroup">Название группы объявлений.</param>
        /// <param name="utmBanner">Идентификатор баннера/объявления.</param>
        public InstallReferrer(
            long installAppTimestamp,
            string packageName,
            long receivedTimestamp,
            string referrerId,
            long? versionCode,
            string? utmCampaign = null,
            string? utmGroup = null,
            string? utmBanner = null) {

            this.packageName = packageName;
            this.referrerId = referrerId;
            this.receivedTimestamp = receivedTimestamp;
            this.installAppTimestamp = installAppTimestamp;
            this.versionCode = versionCode;
            this.utmCampaign = utmCampaign;
            this.utmGroup = utmGroup;
            this.utmBanner = utmBanner;
        }

        /// <summary>
        /// Сравнивает текущий объект с другим объектом того же типа.
        /// </summary>
        /// <param name="obj">Объект для сравнения с текущим объектом.</param>
        /// <returns>true, если объекты равны; в противном случае false.</returns>
        public override bool Equals(object obj) {
            if (obj is InstallReferrer other) {
                return packageName == other.packageName &&
                       referrerId == other.referrerId &&
                       receivedTimestamp == other.receivedTimestamp &&
                       installAppTimestamp == other.installAppTimestamp &&
                       versionCode == other.versionCode &&
                       utmCampaign == other.utmCampaign &&
                       utmGroup == other.utmGroup &&
                       utmBanner == other.utmBanner;
            }

            return false;
        }

        /// <summary>
        /// Возвращает хэш-код для текущего объекта.
        /// </summary>
        /// <returns>Хэш-код для текущего объекта.</returns>
        public override int GetHashCode() {
            unchecked {
                int hash = packageName.GetHashCode();
                hash = (hash * 31) + referrerId.GetHashCode();
                hash = (hash * 31) + receivedTimestamp.GetHashCode();
                hash = (hash * 31) + installAppTimestamp.GetHashCode();
                hash = (hash * 31) + (versionCode?.GetHashCode() ?? 0);
                hash = (hash * 31) + (utmCampaign?.GetHashCode() ?? 0);
                hash = (hash * 31) + (utmGroup?.GetHashCode() ?? 0);
                hash = (hash * 31) + (utmBanner?.GetHashCode() ?? 0);

                return hash;
            }
        }
    }
}