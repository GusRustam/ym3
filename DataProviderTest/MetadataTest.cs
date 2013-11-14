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

        public class Request<T> where T : IMetadataItem {
            public Func<IMetadataContainer<T>, Tuple<bool, string>> Checks { get; private set; }
            public string[] Rics { get; private set; }

            public Request(string[] rics, Func<IMetadataContainer<T>, Tuple<bool, string>> checks) {
                Checks = checks;
                Rics = rics;
            }
        }

        public class EmptyData : IMetadataItem {
        }

        [TestCase]
        public void MockTest() {
            var locker = new object();

            var container = Agent.Factory();

            // creating mocks
            var mockMetaRequest = new Mock<IMetadataRequest<EmptyData>>();
            var mockMetaFactory = new Mock<IMetaObjectFactory<EmptyData>>();
            var mockMetaSetup = new Mock<IMetaRequestSetup<EmptyData>>();

            container.Inject(mockMetaSetup.Object);
            container.Inject(mockMetaRequest.Object);
            container.Inject(mockMetaFactory.Object);

            // setting up callback
            Action<IMetadataContainer<EmptyData>> callback = data => {
                Console.WriteLine("Received Callback, into lock 2");
                lock (locker) {
                    Console.WriteLine("Pulse 2");
                    Monitor.Pulse(locker);
                }
            };

            // setting up the factory
            mockMetaFactory
                .Setup(f => f.CreateSetup())
                .Returns(container.GetInstance<IMetaRequestSetup<EmptyData>>);

            mockMetaFactory
                .Setup(f => f.CreateRequest(It.IsAny<IMetaRequestSetup<EmptyData>>()))
                .Returns(container.GetInstance<IMetadataRequest<EmptyData>>);

            // setting up request
            mockMetaRequest
                .Setup(r => r.WithTimeout(It.IsAny<TimeSpan>()))
                .Returns(mockMetaRequest.Object);
            mockMetaRequest
                .Setup(r => r.Request())
                .Callback(
                    () => new Thread(() => {
                        Console.WriteLine("Data requested, thinking");
                        Thread.Sleep(TimeSpan.FromSeconds(3));
                        callback(null);
                    }).Start());

            // initializing and requesting
            var mtd = container.GetInstance<IMetadata<EmptyData>>();
            mtd = mtd.WithRics("XXX");
            mtd = mtd.OnFinished(callback);
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

        public void GenericTest<T>(Request<T> setup) where T : IMetadataItem, new() {
            var container = Agent.Factory();

            var mtd = container.GetInstance<IMetadata<T>>();
            var cnn = container.GetInstance<IConnection>();
            var locker = new object();

            if (!cnn.ConnectAndWait(10))
                Assert.Inconclusive();

            var failed = false;
            var message = "";
            mtd.WithRics(setup.Rics)
                .OnFinished(data => {
                    Console.WriteLine("Got data!");

                    try {
                        var res = setup.Checks(data);
                        failed = !res.Item1;
                        message = res.Item2;

                    } catch (Exception e) {
                        Console.WriteLine("Test fails, error in checking:\n{0}", e);
                        failed = true;
                    } finally {
                        lock (locker) Monitor.Pulse(locker);
                    }

                })
                .Request()
                .WithTimeout(TimeSpan.FromSeconds(5))
                .Request();

            lock (locker) {
                Monitor.Pulse(locker);
                if (!Monitor.Wait(locker, TimeSpan.FromSeconds(6)))
                    Assert.Fail("Test timeout");
                Assert.IsFalse(failed, message);
            }
        }

        // todo  1) Develop metacode to import data into *Data classes (maybe - reflection first, then metacoding)
        // todo  2) Get the widest chain list possible, load all rics from these chains. Make sure all data loads and filters out
        // todo  3) Think on which operations should IMetadataObject and IMetadataContainer support
        // todo  4) ?????
        // todo  5) PROFIT
        // todo 
        // todo  Another idea - think about moving asserts into callback (see ChainTests). This will eliminate Timeouts but will
        // todo  add need for additional synchronization and timeouting (wait! general timeout cud be implemented via [Timeout] or whatever)
        [TestCase]
        public void MetadataOnBonds() {
            GenericTest(new Request<BondData>(_rics, 
                container => Tuple.Create(
                    container != null && container.Rows != null && container.Rows.Count() == 2, 
                    "Null or not two rows")));
        }

        [TestCase]
        public void MetadataOnCoupons() {
            // i don't really need coupons, do I?
            GenericTest(new Request<CouponData>(_rics, container => Tuple.Create(true, "")));
        }

        [TestCase]
        public void MetadataOnIssueRating() {
            GenericTest(new Request<IssueRatingData>(_rics, container => Tuple.Create(true, "")));
        }

        [TestCase]
        public void MetadataOnIssuerRating() {
            GenericTest(new Request<IssuerRatingData>(_rics, container => Tuple.Create(true, "")));
        }

        [TestCase]
        public void MetadataOnFrn() {
            GenericTest(new Request<FrnData>(_rics, container => Tuple.Create(true, "")));
        }

        [TestCase]
        public void MetadataOnRics() {
            GenericTest(new Request<RicData>(_rics, container => Tuple.Create(true, "")));
        }

        // todo convertibles
        // todo index-linked
        // todo MBS (am I crazy?)

        // todo repo
        // todo bond futures
    }

    [MetaParams("RH:In")]
    public class BondData : IMetadataItem {
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
    public class CouponData : IMetadataItem {
        [MetaColumn(0)]
        public string Ric { get; set; }

        [MetaColumn(1)]
        public DateTime Date { get; set; }

        [MetaField("EJV.C.CouponRate")]
        public double Rate { get; set; }
    }

    [MetaParams("RH:In", "RTSRC:MDY;S&P;FTC")]
    public class IssueRatingData : IMetadataItem {
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
    public class IssuerRatingData : IMetadataItem {
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
    public class FrnData : IMetadataItem {
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
    public class RicData : IMetadataItem {
        [MetaColumn(0)]
        public string Ric { get; set; }

        [MetaColumn(1)]
        public string Contributor { get; set; }

        [MetaField("EJV.C.RICS")]
        public string ContributedRic { get; set; }
    }
}