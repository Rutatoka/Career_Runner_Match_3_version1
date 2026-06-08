namespace RuStore.UnityInstallReferrer.Model {

    /// <summary>
    /// Данные события Full Stream Attribution.
    /// </summary>
    public class FsaEvent : BaseFields {

        /// <summary>
        /// Номер телефона.
        /// </summary>
        public PhoneNumber phoneNumber { get; }

        /// <summary>
        /// Токен авторизации.
        /// </summary>
        public string authToken { get; }

        /// <summary>
        /// Тип события.
        /// </summary>
        public FsaEventType eventType { get; }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="phoneNumber">Номер телефона.</param>
        /// <param name="authToken">Токен авторизации.</param>
        /// <param name="eventType">Тип события.</param>
        public FsaEvent(
            PhoneNumber phoneNumber,
            string authToken,
            FsaEventType eventType
        ) {

            this.phoneNumber = phoneNumber;
            this.authToken = authToken;
            this.eventType = eventType;
        }
    }
}
