using System;
using System.Collections.Generic;

[Serializable()]
public class DataBinary
{

    public Dictionary<string,object> Data {get; set;} = new Dictionary<string, object>();

    public void SaveBinary(Dictionary<string,object> dataDict, string filename)
    {
        this.Data = dataDict;
        FileBinary.SaveToFile(filename, this);
    }

}