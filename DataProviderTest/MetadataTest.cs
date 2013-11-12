using System;
using Connect;
using ContainerAgent;
using DataProvider.Loaders.Metadata;
using NUnit.Framework;

namespace DataProviderTest {
    [TestFixture]
    public class MetadataTest {
        private readonly string[] _rics = {
            "RU25068RMFS=MM", "RU25071RMFS=MM"
        };

        [TestCase]
        public void SimpleMetadataInit() {
            // todo  1) Red-Green-Refactor my testcases
            // todo     1.1) Develop metacode to import data into *Data classes (maybe - reflection first, then metacoding)
            // todo  2) Get the widest chain list possible, load all rics from these chains. Make sure all data loads and filters out
            // todo  3) Think on which operations should IMetadataObject and IMetadataContainer support
            // todo  4) ?????
            // todo  5) PROFIT
            // todo 
            // todo 
            // todo  Another idea - think about moving asserts into callback (see ChainTests). This will eliminate Timeouts but will
            // todo  add need for additional syncronization and timouting (wait! general timout cud be implemented via [Timeout] or whatever
            // todo 


            var container = Agent.Factory();
            var mtd = container.GetInstance<IMetadata>();
            var cnn = container.GetInstance<IConnection>();

            if (!cnn.ConnectAndWait(10))
                Assert.Inconclusive();

            var request = mtd
                .Header<string>("RIC")
                .Field<string>("EJV.X.ADF_BondStructure")
                .Field<string>("EJV.X.ADF_RateStructure")
                .Field<string>("EJV.C.Description")
                .Field<string>("EJV.C.OriginalAmountIssued")
                .Field<string>("EJV.C.IssuerName")
                .Field<string>("EJV.C.BorrowerName")
                .Field<double>("EJV.X.ADF_Coupon")
                .Field<DateTime?>("EJV.C.IssueDate")
                .Field<DateTime?>("EJV.C.MaturityDate")
                .Field<string>("EJV.C.Currency")
                .Field<string>("EJV.C.ShortName")
                .Field<bool>("EJV.C.IsCallable")
                .Field<bool>("EJV.C.IsPutable")
                .Field<bool>("EJV.C.IsFloater")
                .Field<bool>("EJV.C.IsConvertible")
                .Field<bool>("EJV.C.IsStraight")
                .Field<string>("EJV.C.Ticker")
                .Field<string>("EJV.C.Series")
                .Field<string>("EJV.C.BorrowerCntyCode")
                .Field<string>("EJV.C.IssuerCountry")
                .Field<string>("RI.ID.ISIN")
                .Field<string>("EJV.C.ParentTicker")
                .Field<string>("EJV.C.SeniorityTypeDescription")
                .Field<string>("EJV.C.SPIndustryDescription")
                .Field<string>("EJV.C.SPIndustrySubDescription")
                .Field<string>("EJV.C.InstrumentTypeDescription")
                .DisplayMode("RH:In")
                .Rics(_rics)
                .Reciever<IBondData>()
                .OnFinished(data => Console.WriteLine("Got data!"))
                .Request();

            request
                .WithTimeout(TimeSpan.FromSeconds(5))
                .Request();

            // todo Assertions based on data

            // todo coupons and rics
        }

        [TestCase]
        public void MetadataOnCoupons() {
            // i don't really need coupons, do I?
            var container = Agent.Factory();
            var mtd = container.GetInstance<IMetadata>();
            var cnn = container.GetInstance<IConnection>();

            if (!cnn.ConnectAndWait(10))
                Assert.Inconclusive();

            var request = mtd
                .Header<string>("RIC")
                .Header<DateTime>("Date")
                .Field<string>("EJV.C.CouponRate")
                .DisplayMode("RH:In,D")
                .RequestMode("D:1984;2013")
                .Rics(_rics)
                .Reciever<ICouponData>()
                .OnFinished(data => Console.WriteLine("Got data!"))
                .Request();

            request
                .WithTimeout(TimeSpan.FromSeconds(5))
                .Request();

            // todo Assertions based on data
        }


        [TestCase]
        public void MetadataOnIssueRating() {
            var container = Agent.Factory();
            var mtd = container.GetInstance<IMetadata>();
            var cnn = container.GetInstance<IConnection>();

            if (!cnn.ConnectAndWait(10))
                Assert.Inconclusive();

            var request = mtd
                .Header<string>("RIC")
                .Field<string>("EJV.GR.Rating")
                .Field<string>("EJV.GR.RatingDate")
                .Field<string>("EJV.GR.RatingSourceCode")
                .DisplayMode("RH:In")
                .RequestMode("RTSRC:MDY;S&P;FTC")
                .Rics(_rics)
                .Reciever<IIssueRatingData>()
                .OnFinished(data => Console.WriteLine("Got data!"))
                .Request();

            request
                .WithTimeout(TimeSpan.FromSeconds(5))
                .Request();

            // todo Assertions based on data
        }
    }

    public interface IIssueRatingData : IMetadataObject {
    }

    public interface ICouponData : IMetadataObject {
    }

    public interface IBondData : IMetadataObject {
        string BondStructure { get; set; }
    }
}
