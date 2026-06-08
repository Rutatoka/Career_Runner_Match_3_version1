namespace RuStore.UnityInstallReferrer.Internal {

    public interface IJsonSerializer {
        string Serialize<T>(T value, bool prettyPrint = false);
    }
}
