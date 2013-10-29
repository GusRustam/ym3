namespace DataProvider.Loaders.History {
    public interface IHistory {
        IHistoryRequest Subscribe(string ric); // it's a nice way to parametrize constructor
    }
}
