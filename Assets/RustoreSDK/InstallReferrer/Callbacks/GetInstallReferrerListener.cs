using RuStore.Internal;
using RuStore.UnityInstallReferrer.Data;
using System;
using UnityEngine;

namespace RuStore.UnityInstallReferrer.Callbacks {

    public class GetInstallReferrerListener : ResponseListener<InstallReferrer> {

        private const string javaClassName = "ru.rustore.unitysdk.installreferrer.callbacks.GetInstallReferrerListener";

        public GetInstallReferrerListener(Action<RuStoreError> onFailure, Action<InstallReferrer> onSuccess) : base(javaClassName, onFailure, onSuccess) {
        }

        protected override InstallReferrer ConvertResponse(AndroidJavaObject responseObject) {
            return DataConverter.ConvertInstallReferrer(responseObject);
        }
    }
}
