using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AdfinXAnalyticsFunctions;
using Dex2;
using Interop.TSI6;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using ThomsonReuters.Interop.RTX;

namespace DataProvider.Objects {
    public abstract class EikonObjectsBase : IEikonObjects, IDisposable, ISupportsLogging {
        private T CreateT<T>(Func<T> func) where T : class{
            var obj = func();
            if (_keepComObjectsForDisposal && obj != null) ComObjects.Add(obj);
            return obj;
        }

        protected EikonObjectsBase(ILogger logger) {
            Logger = logger;
            _keepComObjectsForDisposal = true;
        }

        public AdxRtChain CreateAdxRtChain() {
            return CreateT(GetAdxRtChain);
        }

        public AdxRtList CreateAdxRtList() {
            return CreateT(GetAdxRtList);
        }

        public AdxRtHistory CreateAdxRtHistory() {
            return CreateT(GetAdxRtHistory);
        }

        public Dex2Mgr CreateDex2Mgr() {
            return CreateT(GetDex2Mgr);
        }

        public AdxBondModule CreateAdxBondModule() {
            return CreateT(GetAdxBondModule);
        }

        public AdxDateModule CreateAdxDateModule() {
            return CreateT(GetAdxDateModule);
        }

        public AdxSwapModule CreateSwapModule() {
            return CreateT(GetAdxSwapModule);
        }

        public AdxConvBondModule CreateAdxConvBondModule() {
            return CreateT(GetAdxConvBondModule);
        }

        public AdxCreditModule CreateAdxCreditModule() {
            return CreateT(GetAdxCreditModule);
        }

        public AdxForexModule CreateAdxForexModule() {
            return CreateT(GetAdxForexModule);
        }

        public TsiSession CreateTsiSession() {
            return CreateT(GetTsiSession);
        }

        public TsiConnectInfo CreateTsiConnectInfo() {
            return CreateT(GetTsiConnectInfo);
        }

        public TsiConnectRequest CreateTsiConnectRequest() {
            return CreateT(GetTsiConnectRequest);
        }

        protected abstract AdxRtChain GetAdxRtChain();
        protected abstract AdxRtList GetAdxRtList();
        protected abstract AdxRtHistory GetAdxRtHistory();
        protected abstract Dex2Mgr GetDex2Mgr();
        protected abstract AdxBondModule GetAdxBondModule();
        protected abstract AdxDateModule GetAdxDateModule();
        protected abstract AdxSwapModule GetAdxSwapModule();
        protected abstract AdxConvBondModule GetAdxConvBondModule();
        protected abstract AdxCreditModule GetAdxCreditModule();
        protected abstract AdxForexModule GetAdxForexModule();
        protected abstract TsiSession GetTsiSession();
        protected abstract TsiConnectInfo GetTsiConnectInfo();
        protected abstract TsiConnectRequest GetTsiConnectRequest();

        private bool _disposed;
        private readonly bool _keepComObjectsForDisposal;
        protected List<object> ComObjects = new List<object>();

        ~EikonObjectsBase() {
            Dispose(false);
        }

        public void Dispose() {
            if (_disposed) return;
            
            Dispose(true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposing) return;
            foreach (var comObject in ComObjects) {
                try {
                    Marshal.FinalReleaseComObject(comObject);
                } catch (Exception ex) {
#if DEBUG
                    this.Warn("Failed to release COM object", ex);
#endif
                }
            }
        }

        public ILogger Logger { get; set; }
    }
}