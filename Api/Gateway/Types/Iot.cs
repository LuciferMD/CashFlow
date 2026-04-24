using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Gateway.Types;

public class Iot
{
    public List<IotDevice> Devices { get; set; }
};


public class IotDevice
{
    public string Type { get; set; }
    public string Name { get; set; }
    public IotPayload Payload { get; set; }
}

public class IotPayload
{
    public int? Co2 { get; set; }
    public int? Pm25 { get; set; }
    public int? Humidity { get; set; }
    public double? Energy { get; set; }
}