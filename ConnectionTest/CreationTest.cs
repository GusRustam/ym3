using Connect;
using ContainerAgent;
using DataProvider.Objects;
using NUnit.Framework;

namespace ConnectionTest {
    [TestFixture]
    public class CreationTest {
        
        [TestCase]
        public void TryCreateSimpeObjects() {
            var factory = Agent.Factory();
            var conn = factory.GetInstance<IConnection>();
            
            if (!conn.ConnectAndWait(10)) Assert.Inconclusive();
            var objects = factory.GetInstance<IEikonObjects>("sdk"); // via sdk

            var adxBondModule = objects.CreateAdxBondModule();
            Assert.NotNull(adxBondModule);
            Assert.NotNull(objects.CreateAdxConvBondModule());
            Assert.NotNull(objects.CreateAdxCreditModule());
            Assert.NotNull(objects.CreateAdxDateModule());
            Assert.NotNull(objects.CreateAdxForexModule());
            Assert.NotNull(objects.CreateAdxRtChain());
            Assert.NotNull(objects.CreateAdxRtHistory());
            Assert.NotNull(objects.CreateAdxRtList());
            Assert.NotNull(objects.CreateDex2Mgr());
            Assert.NotNull(objects.CreateSwapModule());

            objects = factory.GetInstance<IEikonObjects>("plain"); // via new

            var bondModule = objects.CreateAdxBondModule();
            Assert.NotNull(bondModule);
            Assert.NotNull(objects.CreateAdxConvBondModule());
            Assert.NotNull(objects.CreateAdxCreditModule());
            Assert.NotNull(objects.CreateAdxDateModule());
            Assert.NotNull(objects.CreateAdxForexModule());
            Assert.NotNull(objects.CreateAdxRtChain());
            Assert.NotNull(objects.CreateAdxRtHistory());
            Assert.NotNull(objects.CreateAdxRtList());
            Assert.NotNull(objects.CreateDex2Mgr());
            Assert.NotNull(objects.CreateSwapModule());

            Assert.False(ReferenceEquals(adxBondModule, bondModule));
            
            conn.Disconnect();
            
            //objects = factory.GetInstance<IEikonObjects>("plvba"); // via plvba

            //Assert.NotNull(objects.CreateAdxBondModule());
            //Assert.NotNull(objects.CreateAdxConvBondModule());
            //Assert.NotNull(objects.CreateAdxCreditModule());
            //Assert.NotNull(objects.CreateAdxDateModule());
            //Assert.NotNull(objects.CreateAdxForexModule());
            //Assert.NotNull(objects.CreateAdxRtChain());
            //Assert.NotNull(objects.CreateAdxRtHistory());
            //Assert.NotNull(objects.CreateAdxRtList());
            //Assert.NotNull(objects.CreateDex2Mgr());
            //Assert.NotNull(objects.CreateSwapModule());
            //Assert.NotNull(objects.CreateTsiConnectInfo());
            //Assert.NotNull(objects.CreateTsiConnectRequest());
            //Assert.NotNull(objects.CreateTsiSession());
        }
    }
}
