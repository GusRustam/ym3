namespace Toolbox.Memento {
    public interface IMemento {
        void RestoreState();
    }

    public interface IOriginator {
        IMemento GetState();
    }
}
