using System;
using Toolbox.Async;

namespace DataProvider.Loaders.Metadata {
    /// <summary>
    /// Fluent API to Dex2
    /// </summary>
    public interface IMetadata {
        IMetadata Header<T>(string alias, int colNum = -1);
        IMetadata Field<T>(string name);
        IMetadata DisplayMode(string displayMode);
        IMetadata RequestMode(string requestMode);
        IMetadata Rics(params string[] rics);

        IMetadataReciever<T> Reciever<T>() where T : IMetadataObject;
        IMetadataRequest Request();
    }

    /// <summary>
    /// Allows to register callback without losing type information
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMetadataReciever<T> where T : IMetadataObject {
        IMetadata OnFinished(Action<IMetadataContainer<T>> action);
    }

    /// <summary> Stub to create type-specific timeout call </summary>
    public interface IMetadataRequest : ITimeoutCall {
    }

    /// <summary>
    /// This class represents a container of metadata objects, retrieved from DB
    /// </summary>
    /// <remarks>
    /// The class which implements this interface has to contain logic to filter out 
    /// invalid entries. For example, bond must have BondStructure, and in case 
    /// this field is empty, it should not be visible.
    /// </remarks>
    public interface IMetadataContainer<T> where T : IMetadataObject {
        
    }

    /// <summary> This class represents an object, into which data from DB is to be loaded </summary>
    /// <remarks>
    /// The class, which will derive from this interface, will have to implement ways to filter and sort, for example
    /// </remarks>
    public interface IMetadataObject {
    }
}