﻿namespace MQTT.Api;

public record VehicleLocation
{
    public string? Id { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}