using RuStore.Internal;
using System;

namespace RuStore.UnityInstallReferrer.Callbacks {

    public class SendFsaEventListener : SimpleResponseListener {

        private const string javaClassName = "ru.rustore.unitysdk.installreferrer.callbacks.SendFsaEventListener";

        public SendFsaEventListener(Action<RuStoreError> onFailure, Action onSuccess) : base(javaClassName, onFailure, onSuccess) {
        }
    }
}
