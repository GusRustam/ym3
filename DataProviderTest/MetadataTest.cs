using System;
using System.Linq;
using System.Threading;
using Connect;
using ContainerAgent;
using DataProvider.Loaders.Metadata;
using DataProvider.Loaders.Metadata.Data;
using Moq;
using NUnit.Framework;

// todo Convertibles, index-linked, MBS (am I crazy?), repo, bond futures
// todo Get the widest chain list possible, load all rics from these chains. Make sure all data loads and filters out
namespace DataProviderTest {
    [TestFixture]
    public class MetadataTest {
        private readonly string[] _rics = {
            "RU25068RMFS=MM", "RU25071RMFS=MM"
        };

        public class Request<T> where T : IMetadataItem, new() {
            public Func<IMetadataContainer<T>, string> Checks { get; private set; }
            public string[] Rics { get; private set; }

            public Request(string[] rics, Func<IMetadataContainer<T>, string> checks) {
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
            var mockMetaSetup = new Mock<IRequestSetup<EmptyData>>();

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
                .Returns(container.GetInstance<IRequestSetup<EmptyData>>);

            mockMetaFactory
                .Setup(f => f.CreateRequest(It.IsAny<IRequestSetup<EmptyData>>()))
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

            var message = "";
            mtd.WithRics(setup.Rics)
                .OnFinished(data => {
                    Console.WriteLine("Got data!");
                    try {
                        message = setup.Checks(data);
                    } catch (Exception e) {
                        message = e.ToString();
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
                Assert.IsTrue(message == null , message);
            }
        }

        [TestCase]
        public void MetadataOnBonds() {
            GenericTest(new Request<BondData>(_rics, 
                container => {
                    if (container == null || container.Rows == null || container.Rows.Count() != 2)
                        return "Null or not two rows";
                    return null;
                }));
        }

        [TestCase]
        public void MetadataOnCoupons() {
            // i don't really need coupons, do I?
            GenericTest(new Request<CouponData>(_rics, container => null));
        }

        [TestCase]
        public void MetadataOnIssueRating() {
            GenericTest(new Request<IssueRatingData>(_rics, container => null));
        }

        [TestCase]
        public void MetadataOnIssuerRating() {
            GenericTest(new Request<IssuerRatingData>(_rics, container => null));
        }

        [TestCase]
        public void MetadataOnFrn() {
            GenericTest(new Request<FrnData>(_rics, container => null));
        }

        [TestCase]
        public void MetadataOnRics() {
            GenericTest(new Request<RicData>(_rics, container => null));
        }
    }

    [MetaParams(Display : "RH:In")]
    public class BondData : IMetadataItem {
        [MetaField(0)]
        public string Ric { get; set; }

        [MetaField(1, name: "EJV.X.ADF_BondStructure")]
        public string BondStructure { get; set; }

        [MetaField(2, name: "EJV.X.ADF_STRUCTURE")]
        public string Structure { get; set; }

        [MetaField(3, name: "EJV.X.ADF_RateStructure")]
        public string RateStructure { get; set; }
        
        [MetaField(4, name: "EJV.C.Description")]
        public string Description { get; set; }

        [MetaField(5, name: "EJV.C.OriginalAmountIssued")]
        public double? OriginalAmountIssued { get; set; }

        [MetaField(6, name: "EJV.C.IssuerName")]
        public string IssuerName { get; set; }

        [MetaField(7, name: "EJV.C.BorrowerName")]
        public string BorrowerName { get; set; }

        [MetaField(8, name: "EJV.X.ADF_Coupon")]
        public double Coupon { get; set; }

        [MetaField(9, name: "EJV.C.IssueDate")]
        public DateTime? IssueDate { get; set; }

        [MetaField(10, name: "EJV.C.MaturityDate")]
        public DateTime? MaturityDate { get; set; }
        
        [MetaField(11, name: "EJV.C.Currency")]
        public string Currency { get; set; }
        
        [MetaField(12, name: "EJV.C.ShortName")]
        public string ShortName { get; set; }
      
        [MetaField(13, "EJV.C.IsCallable", typeof(ReutersBooleanConverter))]
        public bool IsCallable { get; set; }

        [MetaField(14, "EJV.C.IsPutable", typeof(ReutersBooleanConverter))]
        public bool IsPutable { get; set; }

        [MetaField(15, "EJV.C.IsFloater", typeof(ReutersBooleanConverter))]
        public bool IsFloater { get; set; }

        [MetaField(16,  "EJV.C.IsConvertible", typeof(ReutersBooleanConverter))]
        public bool IsConvertible {  get; set; }

        [MetaField(17,  "EJV.C.IsStraight", typeof(ReutersBooleanConverter))]
        public bool IsStraight { get; set; }

        [MetaField(18, name: "EJV.C.Ticker")]
        public string Ticker { get; set; }

        [MetaField(19, name: "EJV.C.Series")]
        public string Series { get; set; }

        [MetaField(20, name: "EJV.C.BorrowerCntyCode")]
        public string BorrowerCountry { get; set; }

        [MetaField(21, name: "EJV.C.IssuerCountry")]
        public string IssuerCountry { get; set; }

        [MetaField(22, name: "RI.ID.ISIN")]
        public string Isin { get; set; }

        [MetaField(23, name: "EJV.C.ParentTicker")]
        public string ParentTicker { get; set; }

        [MetaField(24, name: "EJV.C.SeniorityTypeDescription")]
        public string SeniorityType { get; set; }

        [MetaField(25, name: "EJV.C.SPIndustryDescription")]
        public string Industry { get; set; }

        [MetaField(26, name: "EJV.C.SPIndustrySubDescription")]
        public string SubIndustry { get; set; }

        [MetaField(27, name: "EJV.C.InstrumentTypeDescription")]
        public string InstrumentType { get; set; }
    }

    [MetaParams(Display: "RH:In,D", Request: "D:1984;2013")]
    public class CouponData : IMetadataItem {
        [MetaField(0)]
        public string Ric { get; set; }

        [MetaField(1)]
        public DateTime Date { get; set; }

        [MetaField(2, name: "EJV.C.CouponRate")]
        public double Rate { get; set; }
    }

    [MetaParams(Display: "RH:In", Request: "RTSRC:MDY;S&P;FTC")]
    public class IssueRatingData : IMetadataItem {
        [MetaField(0)]
        public string Ric { get; set; }

        [MetaField(1, name: "EJV.IR.Rating")]
        public string Rating { get; set; }

        [MetaField(2, name: "EJV.IR.RatingDate")]
        public string RatingDate { get; set; }

        [MetaField(3, name: "EJV.IR.RatingSourceCode")]
        public string RatingSourceCode { get; set; }
    }

    [MetaParams(Display: "RH:In", Request: "RTS:FDL;SPI;MDL RTSC:FRN")]
    public class IssuerRatingData : IMetadataItem {
        [MetaField(0)]
        public string Ric { get; set; }

        [MetaField(1, name: "EJV.GR.Rating")]
        public string Rating { get; set; }

        [MetaField(2, name: "EJV.GR.RatingDate")]
        public string RatingDate { get; set; }

        [MetaField(3, name: "EJV.GR.RatingSourceCode")]
        public string RatingSourceCode { get; set; }
    }

    [MetaParams(Display: "RH:In")]
    public class FrnData : IMetadataItem {
        [MetaField(0)]
        public string Ric { get; set; }

        [MetaField(1, name: "EJV.X.FRNFLOOR")]
        public double? Floor { get; set; }

        [MetaField(2, name: "EJV.X.FRNCAP")]
        public double? Cap { get; set; }

        [MetaField(3, name: "EJV.X.FREQ")]
        public string Frequency { get; set; }

        [MetaField(4, name: "EJV.X.ADF_MARGIN")]
        public double? Margin { get; set; }
    }

    [MetaParams(Display: "RH:In;Con")]
    public class RicData : IMetadataItem {
        [MetaField(0)]
        public string Ric { get; set; }

        [MetaField(1)]
        public string Contributor { get; set; }

        [MetaField(2, name: "EJV.C.RICS")]
        public string ContributedRic { get; set; }
    }
}