using ThomsonReuters.Interop.RTX;

namespace DataProvider.DataLoaders.Status {
    public interface IRunMode {
        RT_RunMode ToAdxMode();
    }
}