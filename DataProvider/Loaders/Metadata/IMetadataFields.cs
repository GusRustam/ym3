namespace DataProvider.Loaders.Metadata {
    /// <summary> This class represents an object, into which data from DB is to be loaded </summary>
    /// <remarks>
    /// The class, which will derive from this interface, will have to implement ways to filter and sort, for example
    /// </remarks>
    public interface IMetadataFields {
        // todo maybe - fuck it and make it dynamic?
        // todo the reason to keep it static is that I could attach attributes to each column
        // todo but attributes will be better used in final data ensemble
        // todo hence, fuck it? No, wait, how do I fuck it??? I will use attributes to bind EJV.* fields to exact fields
        // todo and NOTE! what?
    }
}