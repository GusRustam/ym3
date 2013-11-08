using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataProvider.Loaders.History.Data;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using StructureMap;
using Toolbox;

namespace DataProvider.Loaders.History {
    public class History : IHistory, ISupportsLogging {
        private readonly IContainer _container;
        private HistorySetup _setup;

        public IHistory WithFeed(string feed) {
            _setup.Feed = feed;
            return this;
        }

        public IHistory WithSince(DateTime date) {
            _setup.Since = date;
            return this;
        }

        public IHistory WithTill(DateTime date) {
            _setup.Till = date;
            return this;
        }

        public IHistory WithNumRecords(int num) {
            _setup.Rows = num;
            return this;
        }

        public IHistory WithHistory(Action<IHistoryContainer> callback) {
            _setup.Callback = callback;
            return this;
        }

        public IHistory AppendField(IHistoryField field) {
            if (_setup.Fields == null) _setup.Fields = new List<IHistoryField>();
            _setup.Fields.Add(field);
            return this;
        }

        public IHistory AppendFields(IEnumerable<IHistoryField> fields) {
            if (_setup.Fields == null)
                _setup.Fields = new List<IHistoryField>();
            
            foreach (var field in fields) 
                _setup.Fields.Add(field);
            
            return this;
        }

        public History(IContainer container) {
            _container = container;
            Logger = container.GetInstance<ILogger>();
        }

        public IHistoryRequest Subscribe(string ric) {
            if (string.IsNullOrEmpty(ric))
                throw new ArgumentException("ric");
            
            _setup.Validate();
            return _container
                        .With(typeof(string), ric)
                        .With(_setup)
                        .GetInstance<IHistoryRequest>("single");
        }

        public IHistoryRequest Subscribe(params string[] rics) {
            if (!rics.ToSomeArray().Any()) 
                throw new InvalidDataException("rics");
            
            _setup.Validate();
            return _container
                        .With(typeof(string[]), rics)
                        .With(_setup)
                        .GetInstance<IHistoryRequest>("multi");
        }

        public ILogger Logger { get; private set; }
    }
}