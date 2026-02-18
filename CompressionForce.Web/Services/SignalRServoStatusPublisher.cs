using Microsoft.AspNetCore.SignalR;
using CompressionForce.Services;
using CompressionForce.Web.Hubs;

namespace CompressionForce.Web.Services;

public class SignalRServoStatusPublisher : IServoStatusPublisher
{
    private readonly IHubContext<ServoHub> _hub;

    public SignalRServoStatusPublisher(IHubContext<ServoHub> hub)
    {
        _hub = hub;
    }

    public Task PublishDigitalInputAsync(string key, bool value)
        => _hub.Clients.All.SendAsync("DigitalInputUpdated", key, value);

    public Task PublishDigitalOutputAsync(string key, bool value)
        => _hub.Clients.All.SendAsync("DigitalOutputUpdated", key, value);

    public Task PublishAnalogInputAsync(string key, double v1, double v2)
        => _hub.Clients.All.SendAsync("AnalogInputUpdated", key, v1, v2);

    public Task PublishServoAsync(string servoCode, bool ready, bool alarm, int torque, int actPos)
        => _hub.Clients.All.SendAsync("ServoStatusUpdated",
            servoCode, ready, alarm, torque, actPos);

    public Task PublishTagAsync(string key, object value)
        => _hub.Clients.All.SendAsync("TagUpdated", key, value);
}
