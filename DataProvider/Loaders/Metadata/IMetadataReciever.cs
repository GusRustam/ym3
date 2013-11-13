using System;
using System.Collections;

namespace DataProvider.Loaders.Metadata {
    /// <summary>
    /// Allows to register callback without losing type information
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMetadataReciever<T> where T : IMetadataFields {
        IMetadata OnFinished(Action<IMetadataContainer<T>> action);
    }

    public interface IMetadataReciever{
        IMetadata OnFinished(Action<object> action);
    }
}