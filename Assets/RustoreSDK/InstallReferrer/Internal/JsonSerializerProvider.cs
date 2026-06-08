namespace RuStore.UnityInstallReferrer.Internal {

    public static class JsonSerializerProvider {
        private static IJsonSerializer _defaultSerializer;

        public static IJsonSerializer Default {
            get => _defaultSerializer ??= CreateDefault();
            set => _defaultSerializer = value;
        }

        public static IJsonSerializer CreateDefault() =>
            new BaseJsonSerializer();

        public static string Serialize<T>(T value, bool prettyPrint = false, IJsonSerializer customSerializer = null) =>
            (customSerializer ?? Default).Serialize(value, prettyPrint);
    }
}
