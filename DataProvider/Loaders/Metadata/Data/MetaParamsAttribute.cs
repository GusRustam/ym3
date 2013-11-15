using System;

namespace DataProvider.Loaders.Metadata.Data {
    public class MetaParamsAttribute : Attribute {
        private readonly string _request;
        private readonly string _display;

        public string Request {
            get { return _request; }
        }

        public string Display {
            get { return _display; }
        }

// ReSharper disable InconsistentNaming
        public MetaParamsAttribute(string Request = "", string Display = "") {
// ReSharper restore InconsistentNaming
            _request = Request;
            _display = Display;
        }
    }
}