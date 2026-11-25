using System;
using Unity.Services.Core;
using UnityEngine;
#if ENABLE_UNITY_CONSENT
using UnityEngine.UnityConsent;
#else
using Unity.Services.Analytics;
#endif

namespace FinalCoat{
    public static class AnalyticsSetup{
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static async void Init(){
            try{
#if ENABLE_UNITY_CONSENT
                EndUserConsent.SetConsentState(new ConsentState{AnalyticsIntent = ConsentStatus.Granted});
#endif
                await UnityServices.InitializeAsync();
#if !ENABLE_UNITY_CONSENT
                AnalyticsService.Instance.StartDataCollection();
#endif
            }
            catch(Exception e){
                Debug.LogWarning("analytics not started: " + e.Message);
            }
        }
    }
}
