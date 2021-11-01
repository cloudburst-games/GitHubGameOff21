// DataBinary: serializable data container class for data to be saved in a binary format. This is required for the Serializable attribute.
using System;
using System.Collections.Generic;

[Serializable()]
public class DataBinary
{

    public Dictionary<string,object> Data {get; private set;} = new Dictionary<string, object>();

    public void SaveBinary(Dictionary<string,object> dataDict, string filename)
    {
        this.Data = dataDict;
        FileBinary.SaveToFile(filename, this);
    }

}