using AdfinXAnalyticsFunctions;
using Dex2;
using Interop.TSI6;
using LoggingFacility;
using ThomsonReuters.Interop.RTX;

namespace DataProvider.Objects {
    public class EikonObjectsPlain : EikonObjectsBase {
        public EikonObjectsPlain(ILogger logger) : base(logger) {
        }

        protected override AdxRtChain GetAdxRtChain() {
            return new AdxRtChain();
        }

        protected override AdxRtList GetAdxRtList() {
            return new AdxRtList();
        }

        protected override AdxRtHistory GetAdxRtHistory() {
            return new AdxRtHistory();
        }

        protected override Dex2Mgr GetDex2Mgr() {
            return new Dex2Mgr();
        }

        protected override AdxBondModule GetAdxBondModule() {
            return new AdxBondModule();
        }

        protected override AdxDateModule GetAdxDateModule() {
            return new AdxDateModule();
        }

        protected override AdxSwapModule GetAdxSwapModule() {
            return new AdxSwapModule();
        }

        protected override AdxConvBondModule GetAdxConvBondModule() {
            return new AdxConvBondModule();
        }

        protected override AdxCreditModule GetAdxCreditModule() {
            return new AdxCreditModule();
        }

        protected override AdxForexModule GetAdxForexModule() {
            return new AdxForexModule();
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
    }
}