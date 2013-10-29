using ThomsonReuters.Interop.RTX;

namespace DataProvider.Loaders.Status {
    public interface IRunMode {
        RT_RunMode ToAdxMode();
    }
}