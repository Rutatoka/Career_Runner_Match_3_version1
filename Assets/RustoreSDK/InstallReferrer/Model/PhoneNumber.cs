namespace RuStore.UnityInstallReferrer.Model {

    /// <summary>
    /// Номер телефона. 
    /// </summary>
    public class PhoneNumber : BaseValue<string> {

        /// <summary>
        /// Номер телефона.
        /// </summary>
        public string number => value;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="value">Значение.</param>
        public PhoneNumber(string value) : base(value) { }
    }
}
