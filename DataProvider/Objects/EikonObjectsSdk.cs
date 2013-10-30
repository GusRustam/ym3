using AdfinXAnalyticsFunctions;
using Connect;
using Dex2;
using EikonDesktopDataAPILib;
using Interop.TSI6;
using LoggingFacility;
using ThomsonReuters.Interop.RTX;

namespace DataProvider.Objects {
    public class EikonObjectsSdk : EikonObjectsBase {
        private static IEikonDesktopDataAPI _sdk;

        public EikonObjectsSdk(IConnection sdk, ILogger logger)
            : base(logger) {
            _sdk = sdk.Sdk;
        }

        protected override AdxRtChain GetAdxRtChain() {
            return _sdk.CreateAdxRtChain();
        }

        protected override AdxRtList GetAdxRtList() {
            return _sdk.CreateAdxRtList();
        }

        protected override AdxRtHistory GetAdxRtHistory() {
            return _sdk.CreateAdxRtHistory();
        }

        protected override Dex2Mgr GetDex2Mgr() {
            return _sdk.CreateDex2Mgr();
        }

        protected override AdxBondModule GetAdxBondModule() {
            return _sdk.CreateAdxBondModule();
        }

        protected override AdxDateModule GetAdxDateModule() {
            return _sdk.CreateAdxDateModule();
        }

        protected override AdxSwapModule GetAdxSwapModule() {
            return _sdk.CreateAdxSwapModule();
        }

        protected override AdxConvBondModule GetAdxConvBondModule() {
            return _sdk.CreateAdxConvBondModule();
        }

        protected override AdxCreditModule GetAdxCreditModule() {
            return _sdk.CreateAdxCreditModule();
        }

        protected override AdxForexModule GetAdxForexModule() {
            return _sdk.CreateAdxForexModule();
        }

        protected override TsiSession GetTsiSession() {
            return null;
        }

        protected override TsiConnectInfo GetTsiConnectInfo() {
            return null;
        }

        protected override TsiConnectRequest GetTsiConnectRequest() {
            return null;
        }

        protected override TsiReqInfo GetTsiReqInfo() {
            return null;
        }

        protected override TsiGetDataRequest GetTsiGetDataRequest() {
            return null;
        }
    }
}