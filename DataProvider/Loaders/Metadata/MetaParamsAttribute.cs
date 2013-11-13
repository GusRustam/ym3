using System;

namespace DataProvider.Loaders.Metadata {
    public class MetaParamsAttribute : Attribute {
        private readonly string _request;
        private readonly string _display;

        public string Request {
            get { return _request; }
        }

        public string Display {
            get { return _display; }
        }

        public MetaParamsAttribute(string request = "", string display = "") {
            _request = request;
            _display = display;
        }
    }
}