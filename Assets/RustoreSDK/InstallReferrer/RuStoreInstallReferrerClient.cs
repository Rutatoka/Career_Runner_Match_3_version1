using UnityEngine;
using System;
using RuStore.UnityInstallReferrer.Callbacks;
using RuStore.UnityInstallReferrer.Internal;
using RuStore.UnityInstallReferrer.Model;

namespace RuStore.UnityInstallReferrer {

    /// <summary>
    /// SDK Install Referrer — инструмент атрибуции для рекламных и аналитических систем.
    /// Он позволяет отслеживать количество установок вашего приложения, загруженных из RuStore по рекламным ссылкам.
    /// </summary>
    public class InstallReferrerClient {

        /// <summary>
        /// Версия плагина.
        /// </summary>
        public static string PluginVersion = "10.3.1";

        private static InstallReferrerClient _instance;
        private static bool _isInstanceInitialized;

        private AndroidJavaObject _clientWrapper;
        private bool _isInitialized;

        /// <summary>
        /// Возвращает true, если синглтон инициализирован, в противном случае — false.
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Возвращает единственный экземпляр InstallReferrerClient (реализация паттерна Singleton).
        /// Если экземпляр еще не создан, создает его.
        /// </summary>
        /// <example>@include public_static_InstallReferrerClient_Instance.cs</example>
        public static InstallReferrerClient Instance {
            get {
                if (!_isInstanceInitialized) {
                    _isInstanceInitialized = true;
                    _instance = new InstallReferrerClient();
                }

                return _instance;
            }
        }

        private InstallReferrerClient() {
        }

        /// <summary>
        /// Выполняет инициализацию синглтона RuStoreInstallReferrerClient.
        /// </summary>
        /// <returns>Возвращает true, если инициализация была успешно выполнена, в противном случае — false.</returns>
        /// <example>@include public_bool_Init.cs</example>
        public bool Init() {
            if (Application.platform != RuntimePlatform.Android) {
                return false;
            }

            if (_isInitialized) {
                Debug.LogError("Error: RuStore Install Referrer is already initialized");
                return false;
            }

            CallbackHandler.InitInstance();
            ActivityInstaller.Instance.Install();

            using (var clientJavaClass = new AndroidJavaClass("ru.rustore.unitysdk.installreferrer.RuStoreUnityInstallReferrerClient")) {
                _clientWrapper = clientJavaClass.GetStatic<AndroidJavaObject>("INSTANCE");
            }

            _isInitialized = true;

            return true;
        }

        /// <summary>
        /// Получение RuStoreInstallReferrer.
        /// </summary>
        /// <param name="onFailure">
        /// Действие, выполняемое в случае ошибки.
        /// Возвращает объект RuStore.RuStoreError с информацией об ошибке.
        /// </param>
        /// <param name="onSuccess">
        /// Действие, выполняемое при успешном завершении операции.
        /// Возвращает объект RuStore.InstallReferrer.RuStoreInstallReferrer.
        /// </param>
        /// <example>@include public_void_GetInstallReferrer.cs</example>
        public void GetInstallReferrer(Action<RuStoreError> onFailure, Action<InstallReferrer> onSuccess) {
            if (!IsPlatformSupported(onFailure)) return;

            var listener = new GetInstallReferrerListener(onFailure, onSuccess);
            _clientWrapper.Call("getInstallReferrer", listener);
        }

        /// <summary>
        /// Передача события Full Stream Attribution в ВК Реклама.
        /// </summary>
        /// <param name="fsaEvent">Данные события FSA.</param>
        /// <param name="onFailure">
        /// Действие, выполняемое в случае ошибки.
        /// Возвращает объект RuStore.RuStoreError с информацией об ошибке.
        /// </param>
        /// <param name="onSuccess">
        /// Действие, выполняемое при успешном завершении операции.
        /// </param>
        /// <example>@include public_void_SendFsaEvent.cs</example>
        public void SendFsaEvent(FsaEvent fsaEvent, Action<RuStoreError> onFailure, Action onSuccess) {
            if (!IsPlatformSupported(onFailure)) return;

            var json = JsonSerializerProvider.Serialize(fsaEvent);

            var listener = new SendFsaEventListener(onFailure, onSuccess);
            _clientWrapper.Call("sendFsaEvent", json, listener);
        }

        private bool IsPlatformSupported(Action<RuStoreError> onFailure) {
            if (Application.platform != RuntimePlatform.Android) {
                onFailure?.Invoke(new RuStoreError() {
                    name = "InstallReferrerClientError",
                    description = "Unsupported platform"
                });

                return false;
            }

            return true;
        }
    }
}