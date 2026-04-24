namespace Gateway.Types;

[QueryType]
public static class IotQuery
{
	
	public static Iot GetIot()
		 => new Iot //Mocking for now
		 {
			 Devices = new List<IotDevice>
			{
				new IotDevice
				{
					Type = "air_quality",
					Name = "Kitchen",
					Payload = new IotPayload
					{
						Co2 = 528,
						Pm25 = 7,
						Humidity = 65
					}
				},
				new IotDevice
				{
					Type = "air_quality",
					Name = "Bedroom",
					Payload = new IotPayload
					{
						Co2 = 913,
						Pm25 = 47,
						Humidity = 30
					}
				},
				new IotDevice
				{
					Type = "energy",
					Name = "Garage",
					Payload = new IotPayload
					{
						Energy = 293.61
					}
				}
			}
		 };
}
