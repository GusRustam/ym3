using System;
using System.Runtime.InteropServices;
using AdfinXAnalyticsFunctions;
using Dex2;
using Interop.TSI6;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using ThomsonReuters.Interop.RTX;

namespace DataProvider.Objects {
    //[LoggerName(typeof(EikonObjectsPlVba))]
    public class EikonObjectsPlVba : EikonObjectsBase {
        public EikonObjectsPlVba(ILogger logger)
            : base(logger) {
        }

        [DllImport("PLVbaApis.dll", EntryPoint = "CreateReutersObject")]
        private static extern IntPtr CreateReutersObject(string name);

        private T GetObject<T>(string name) where T : class {
            object obj;
            try {
                obj = Marshal.PtrToStructure(CreateReutersObject(name), typeof(T));
            } catch (Exception ex) {
                this.Error("Failed to cast pointer to structure", ex);
                return null;
            }
            if (obj == null) return null;
            return obj as T;
        }

        protected override AdxRtChain GetAdxRtChain() {
            return GetObject<AdxRtChain>("AdfinXRtLib.AdxRtChain");
        }

        protected override AdxRtList GetAdxRtList() {
            return GetObject<AdxRtList>("AdfinXRtLib.AdxRtList");
        }

        protected override AdxRtHistory GetAdxRtHistory() {
            return GetObject<AdxRtHistory>("AdfinXRtLib.AdxRtHistory");
        }

        protected override Dex2Mgr GetDex2Mgr() {
            return GetObject<Dex2Mgr>("Dex2.Dex2Mgr");
        }

        protected override AdxBondModule GetAdxBondModule() {
            return GetObject<AdxBondModule>("AdfinXAnalyticsFunctions.AdxBondModule");
        }

        protected override AdxDateModule GetAdxDateModule() {
            return GetObject<AdxDateModule>("AdfinXAnalyticsFunctions.AdxDateModule");
        }

        protected override AdxSwapModule GetAdxSwapModule() {
            return GetObject<AdxSwapModule>("AdfinXAnalyticsFunctions.AdxSwapModule");
        }

        protected override AdxConvBondModule GetAdxConvBondModule() {
            return GetObject<AdxConvBondModule>("AdfinXAnalyticsFunctions.AdxConvBondModule");
        }

        protected override AdxCreditModule GetAdxCreditModule() {
            return GetObject<AdxCreditModule>("AdfinXAnalyticsFunctions.AdxCreditModule");
        }

        protected override AdxForexModule GetAdxForexModule() {
            return GetObject<AdxForexModule>("AdfinXAnalyticsFunctions.AdxForexModule");
        }

        protected override TsiSession GetTsiSession() {
            return GetObject<TsiSession>("TSI6.TsiSession");
        }

        protected override TsiConnectInfo GetTsiConnectInfo() {
            return GetObject<TsiConnectInfo>("TSI6.TsiConnectInfo");
        }

        protected override TsiConnectRequest GetTsiConnectRequest() {
            return GetObject<TsiConnectRequest>("TSI6.TsiConnectRequest");
        }

        protected override TsiReqInfo GetTsiReqInfo() {
            return GetObject<TsiReqInfo>("TSI6.TsiReqInfo");
        }

        protected override TsiGetDataRequest GetTsiGetDataRequest() {
            return GetObject<TsiGetDataRequest>("TSI6.TsiGetDataRequest");
        }
    }
}