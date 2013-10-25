using AdfinXAnalyticsFunctions;
using Dex2;
using Interop.TSI6;
using ThomsonReuters.Interop.RTX;

namespace DataProvider.RawData {
    public interface IEikonObjects  {
        AdxRtChain CreateAdxRtChain();
        AdxRtList CreateAdxRtList();
        AdxRtHistory CreateAdxRtHistory();

        Dex2Mgr CreateDex2Mgr();

        AdxBondModule CreateAdxBondModule();
        AdxDateModule CreateAdxDateModule();
        AdxSwapModule CreateSwapModule();
        AdxConvBondModule CreateAdxConvBondModule();
        AdxCreditModule CreateAdxCreditModule();
        AdxForexModule CreateAdxForexModule();

        TsiSession CreateTsiSession();
        TsiConnectInfo CreateTsiConnectInfo();
        TsiConnectRequest CreateTsiConnectRequest();
    }
}