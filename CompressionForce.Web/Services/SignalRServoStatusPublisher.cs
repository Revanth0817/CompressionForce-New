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

    /* ================= SERVO ================= */
    public async Task PublishServoAsync(
        string servoCode,
        bool ready,
        bool alarm,
        int torque)
    {
        await _hub.Clients.All.SendAsync(
            "ServoStatusUpdated",
            servoCode,
            ready,
            alarm,
            torque
        );
    }

    /* ================= DIGITAL INPUT ================= */
    public async Task PublishDigitalInputAsync(
        string tagKey,
        bool value)
    {
        await _hub.Clients.All.SendAsync(
            "DigitalInputUpdated",
            tagKey,
            value
        );
    }

    /* ================= DIGITAL OUTPUT ================= */
    public async Task PublishDigitalOutputAsync(
        string tagKey,
        bool value)
    {
        await _hub.Clients.All.SendAsync(
            "DigitalOutputUpdated",
            tagKey,
            value
        );
    }
}
