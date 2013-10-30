using System;
using System.Collections.Generic;
using DataProvider.Loaders.History.Data;
using LoggingFacility;
using LoggingFacility.LoggingSupport;
using StructureMap;

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

        public IHistory WithCallback(Action<IHistorySnapshot> callback) {
            _setup.Callback = callback;
            return this;
        }

        public IHistory AppendField(IHistoryField field) {
            if (_setup.Fields == null) _setup.Fields = new List<IHistoryField>();
            _setup.Fields.Add(field);
            return this;
        }

        public History(IContainer container) {
            _container = container;
            Logger = container.GetInstance<ILogger>();
        }

        public IHistoryRequest Subscribe(string ric) {
            _setup.Ric = ric;
            _setup.Validate();
            return _container.
                        With("ric").EqualTo(ric).
                        GetInstance<IHistoryRequest>();
        }

        public ILogger Logger { get; private set; }
    }
}