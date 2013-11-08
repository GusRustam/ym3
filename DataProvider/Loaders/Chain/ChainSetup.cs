using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DataProvider.Loaders.Chain {
    public class ChainSetup  {
        public IList<string> Rics = new List<string>();
        public string Feed;
        public string Mode;
        public Action<IChainData> Callback;

        public void Validate() {
            if (string.IsNullOrEmpty(Feed)) 
                throw new ArgumentException("feed");

            Rics = new List<string>(
                (from ric in Rics 
                where !string.IsNullOrEmpty(ric) 
                select ric).Distinct()); // todo a bit ugly

            if (!Rics.Any())
                throw new ArgumentException("rics");
        }

        public ChainSetup Clone() {
            return new ChainSetup {
                Callback = Callback,
                Feed = Feed,
                Rics = new List<string>(Rics)
            };
        }

        public void AddMode(int num) {
            DoAddMode(num.ToString(CultureInfo.InvariantCulture));
        }

        public void AddMode(int @from, int to) {
            DoAddMode(string.Format(CultureInfo.InvariantCulture, "{0}-{1}", @from, to));
        }

        private void DoAddMode(string what) {
            Mode = string.IsNullOrEmpty(Mode) ? what : string.Format("{0} {1}", Mode, what);
        }
    }
}