using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using StructureMap;
//using DataManager;

namespace UnitTesting {
    [TestFixture]
    public class TestGlossary {
        [TestCase]
        public void TestSomeGlossary() {
            //ObjectFactory.Configure(x => {
            //    x.For<IGlossary>().Use<Glossary>();
            //    x.For<ISaverLoader>().Use<JsonSaverLoader>();

            //});
            //var glossary = ObjectFactory.GetInstance<IGlossary>();
            //glossary.Load(new FileInfo("..."));
            //glossary.GetChainsWithAny("xxx", "www");
            //glossary.GetListsWithAll("xxx", "www");
        }
    }
}
