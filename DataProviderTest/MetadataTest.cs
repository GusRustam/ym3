using System;
using System.Linq;
using System.Threading;
using Connect;
using ContainerAgent;
using DataProvider.Loaders.Metadata;
using Moq;
using NUnit.Framework;

namespace DataProviderTest {
    [TestFixture]
    public class MetadataTest {
        private readonly string[] _rics = {
            "RU25068RMFS=MM", "RU25071RMFS=MM"
        };

        public class Request<T> where T : IMetadataFields {
            public Func<IMetadataContainer<T>, bool> Checks { get; private set; }
            public string[] Rics { get; private set; }

            public Request(string[] rics, Func<IMetadataContainer<T>, bool> checks) {
                Checks = checks;
                Rics = rics;
            }
        }

        [TestCase]
        public void MockTest() {
            var locker = new object();

            var container = Agent.Factory();

            // erasing any settings related to metadata
            container.EjectAllInstancesOf<IMetadata>();
            container.EjectAllInstancesOf<IMetadataRequest>();
            container.EjectAllInstancesOf<IMetaObjectFactory>();
            container.EjectAllInstancesOf<IMetadataFields>();
            container.EjectAllInstancesOf<IMetadataReciever>();

            // creating mocks
            var mockMetaRequest = new Mock<IMetadataRequest>();
            var mockMetaFactory = new Mock<IMetaObjectFactory>();
            var mockMetaFields = new Mock<IMetadataFields>();
            var mockMetaReciever = new Mock<IMetadataReciever>();

            // configuring container
            container.Configure(x => {
                x.For<IMetadata>().Singleton().Use<Metadata>();
                x.For<IMetadataRequest>().Use(mockMetaRequest.Object);
                x.For<IMetaObjectFactory>().Use(mockMetaFactory.Object);
                x.For<IMetadataFields>().Use(mockMetaFields.Object);
                x.For<IMetadataReciever>().Use(mockMetaReciever.Object);
            });

            // setting up callback
            Action<object> callback = data => {
                Console.WriteLine("Received Callback, into lock 2");
                lock (locker) {
                    Console.WriteLine("Pulse 2");
                    Monitor.Pulse(locker);
                }
            };

            // setting up the factory
            mockMetaFactory.Setup(f => f.CreateRequest(It.IsAny<IMetadata>())).Returns(() => mockMetaRequest.Object);
            mockMetaFactory.Setup(f => f.CreateReciever()).Returns(() => mockMetaReciever.Object);

            // setting up receiver
            mockMetaReciever.Setup(r => r.OnFinished(It.IsAny<Action<object>>())).Returns(container.GetInstance<Metadata>);

            // setting up request
            mockMetaRequest.Setup(r => r.WithTimeout(It.IsAny<TimeSpan>())).Returns(mockMetaRequest.Object);
            mockMetaRequest.Setup(r => r.Request()).Callback(() => new Thread(() => {
                Console.WriteLine("Data requested, thinking");
                Thread.Sleep(TimeSpan.FromSeconds(3));
                callback(new object());
            }).Start());


            // initializing and requesting
            var mtd = container.GetInstance<IMetadata>();
            mtd = mtd.WithRics("XXX");
            var rec = mtd.Reciever(mockMetaFields.Object.GetType());
            mtd = rec.OnFinished(callback);
            var req = mtd.Request();
            var tm = req.WithTimeout(TimeSpan.FromSeconds(5));
            tm.Request();

            Console.WriteLine("Now I will wait for callback, into lock 1");
            lock (locker) {
                Console.WriteLine("Pulse 1");
                Monitor.Pulse(locker);
                Console.WriteLine("Wait 1");
                if (!Monitor.Wait(locker, TimeSpan.FromSeconds(6)))
                    Assert.Fail("Timeout");
            }

            Console.WriteLine("Done");
        }

        public void GenericTest<T>(Request<T> setup) where T : IMetadataFields, new() {
            var container = Agent.Factory();

            var mtd = container.GetInstance<IMetadata>();
            var cnn = container.GetInstance<IConnection>();
            var locker = new object();

            if (!cnn.ConnectAndWait(10))
                Assert.Inconclusive();
             
            mtd.WithRics(setup.Rics)
                .Reciever<T>()
                .OnFinished(data => {
                    Console.WriteLine("Got data!");
                    Assert.IsTrue(setup.Checks(data));
                    lock (locker) Monitor.Pulse(locker);
                })
                .Request()
                .WithTimeout(TimeSpan.FromSeconds(5))
                .Request();

            lock (locker) {
                Monitor.Pulse(locker);
                if (!Monitor.Wait(locker, TimeSpan.FromSeconds(6)))
                    Assert.Fail("Timeout");
            }
        }

        [TestCase]
        public void MetadataOnBonds() {
            // todo  1) Red-Green-Refactor my test cases
            // todo     1.1) Develop metacode to import data into *Data classes (maybe - reflection first, then metacoding)
            // todo  2) Get the widest chain list possible, load all rics from these chains. Make sure all data loads and filters out
            // todo  3) Think on which operations should IMetadataObject and IMetadataContainer support
            // todo  4) ?????
            // todo  5) PROFIT
            // todo 
            // todo  Another idea - think about moving asserts into callback (see ChainTests). This will eliminate Timeouts but will
            // todo  add need for additional synchronization and timeouting (wait! general timeout cud be implemented via [Timeout] or whatever)

            GenericTest(new Request<BondData>(_rics, container => container.Rows.Count() == 2));
            // todo Checks based on data
        }

        [TestCase]
        public void MetadataOnCoupons() {
            // i don't really need coupons, do I?
            // todo Checks based on data
            GenericTest(new Request<CouponData>(_rics, container => true));
        }

        [TestCase]
        public void MetadataOnIssueRating() {
            // todo Checks based on data
            GenericTest(new Request<IssueRatingData>(_rics, container => true));
        }

        [TestCase]
        public void MetadataOnIssuerRating() {
            // todo Checks based on data
            GenericTest(new Request<IssuerRatingData>(_rics, container => true));
        }

        [TestCase]
        public void MetadataOnFrn() {
            // todo Checks based on data
            GenericTest(new Request<FrnData>(_rics, container => true));
        }

        [TestCase]
        public void MetadataOnRics() {
            // todo Checks based on data
            GenericTest(new Request<RicData>(_rics, container => true));
        }

        // todo convertibles
        // todo index-linked
        // todo MBS (am I crazy?)

        // todo repo
        // todo bond futures
    }

    [MetaParams("RH:In")]
    public class BondData : IMetadataFields {
        [MetaColumn(0)]
        public string Ric { get; set; }

        [MetaField("EJV.X.ADF_BondStructure")]
        public string BondStructure { get; set; }

        [MetaField("EJV.X.ADF_STRUCTURE")]
        public string Structure { get; set; }

        [MetaField("EJV.X.ADF_RateStructure")]
        public string RateStructure { get; set; }
        
        [MetaField("EJV.C.Description")]
        public string Description { get; set; }

        [MetaField("EJV.C.OriginalAmountIssued")]
        public double? OriginalAmountIssued { get; set; }

        [MetaField("EJV.C.IssuerName")]
        public string IssuerName { get; set; }

        [MetaField("EJV.C.BorrowerName")]
        public string BorrowerName { get; set; }

        [MetaField("EJV.X.ADF_Coupon")]
        public double Coupon { get; set; }

        [MetaField("EJV.C.IssueDate")]
        public DateTime? IssueDate { get; set; }

        [MetaField("EJV.C.MaturityDate")]
        public DateTime? MaturityDate { get; set; }
        
        [MetaField("EJV.C.Currency")]
        public string Currency { get; set; }
        
        [MetaField("EJV.C.ShortName")]
        public string ShortName { get; set; }
      
        [MetaField("EJV.C.IsCallable")]
        public bool IsCallable { get; set; }
        
        [MetaField("EJV.C.IsPutable")]
        public bool IsPutable { get; set; }

        [MetaField("EJV.C.IsFloater")]
        public bool IsFloater { get; set; }

        [MetaField("EJV.C.IsConvertible")]
        public bool IsConvertible { get; set; }

        [MetaField("EJV.C.IsStraight")]
        public bool IsStraight { get; set; }

        [MetaField("EJV.C.Ticker")]
        public string Ticker { get; set; }

        [MetaField("EJV.C.Series")]
        public string Series { get; set; }

        [MetaField("EJV.C.BorrowerCntyCode")]
        public string BorrowerCountry { get; set; }

        [MetaField("EJV.C.IssuerCountry")]
        public string IssuerCountry { get; set; }

        [MetaField("RI.ID.ISIN")]
        public string Isin { get; set; }

        [MetaField("EJV.C.ParentTicker")]
        public string ParentTicker { get; set; }

        [MetaField("EJV.C.SeniorityTypeDescription")]
        public string SeniorityType { get; set; }

        [MetaField("EJV.C.SPIndustryDescription")]
        public string Industry { get; set; }

        [MetaField("EJV.C.SPIndustrySubDescription")]
        public string SubIndustry { get; set; }

        [MetaField("EJV.C.InstrumentTypeDescription")]
        public string InstrumentType { get; set; }
    }

    [MetaParams("RH:In,D", "D:1984;2013")]
    public class CouponData : IMetadataFields {
        [MetaColumn(0)]
        public string Ric { get; set; }

        [MetaColumn(1)]
        public DateTime Date { get; set; }

        [MetaField("EJV.C.CouponRate")]
        public double Rate { get; set; }
    }

    [MetaParams("RH:In", "RTSRC:MDY;S&P;FTC")]
    public class IssueRatingData : IMetadataFields {
        [MetaColumn(0)]
        public string Ric { get; set; }

        [MetaField("EJV.IR.Rating")]
        public string Rating { get; set; }

        [MetaField("EJV.IR.RatingDate")]
        public string RatingDate { get; set; }

        [MetaField("EJV.IR.RatingSourceCode")]
        public string RatingSourceCode { get; set; }
    }

    [MetaParams("RH:In", "RTS:FDL;SPI;MDL RTSC:FRN")]
    public class IssuerRatingData : IMetadataFields {
        [MetaColumn(0)]
        public string Ric { get; set; }

        [MetaField("EJV.GR.Rating")]
        public string Rating { get; set; }

        [MetaField("EJV.GR.RatingDate")]
        public string RatingDate { get; set; }

        [MetaField("EJV.GR.RatingSourceCode")]
        public string RatingSourceCode { get; set; }
    }

    [MetaParams("RH:In")]
    public class FrnData : IMetadataFields {
        [MetaColumn(0)]
        public string Ric { get; set; }

        [MetaField("EJV.X.FRNFLOOR")]
        public double? Floor { get; set; }

        [MetaField("EJV.X.FRNCAP")]
        public double? Cap { get; set; }

        [MetaField("EJV.X.FREQ")]
        public string Frequency { get; set; }

        [MetaField("EJV.X.ADF_MARGIN")]
        public double? Margin { get; set; }
    }

    [MetaParams("RH:In;Con")]
    public class RicData : IMetadataFields {
        [MetaColumn(0)]
        public string Ric { get; set; }

        [MetaColumn(1)]
        public string Contributor { get; set; }

        [MetaField("EJV.C.RICS")]
        public string ContributedRic { get; set; }
    }
}