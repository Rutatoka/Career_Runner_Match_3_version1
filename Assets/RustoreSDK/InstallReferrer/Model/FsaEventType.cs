namespace RuStore.UnityInstallReferrer.Model {
    /// <summary>
    /// Тип события Full Stream Attribution.
    /// </summary>
    public abstract class FsaEventType {
        public static string NameOf<T>() where T : FsaEventType => typeof(T).Name;

        private FsaEventType() { }

        /// <summary>
        /// Установка приложения / первая авторизация.
        /// </summary>
        public sealed class Install : FsaEventType {
            public Install() { }
        }

        /// <summary>
        /// Просмотр предложения.
        /// </summary>
        public sealed class ViewOffer : FsaEventType {
            /// <summary>
            /// Идентификатор товара.
            /// </summary>
            public string productId { get; }

            public ViewOffer(string productId) {
                this.productId = productId;
            }
        }

        /// <summary>
        /// Добавление в корзину.
        /// </summary>
        public sealed class AddToCart : FsaEventType {
            /// <summary>
            /// Идентификатор товара.
            /// </summary>
            public string productId { get; }

            public AddToCart(string productId) {
                this.productId = productId;
            }
        }

        /// <summary>
        /// Покупка.
        /// </summary>
        public sealed class Purchase : FsaEventType {
            /// <summary>
            /// Идентификатор товара.
            /// </summary>
            public string productId { get; }

            public Purchase(string productId) {
                this.productId = productId;
            }
        }

        /// <summary>
        /// Добавление в список желаний.
        /// </summary>
        public sealed class AddToWishlist : FsaEventType {
            /// <summary>
            /// Идентификатор товара.
            /// </summary>
            public string productId { get; }

            public AddToWishlist(string productId) {
                this.productId = productId;
            }
        }
    }
}
