namespace CompressionForce.Services;

public interface IServoStatusPublisher
{
    /* ================= SERVO ================= */
    Task PublishServoAsync(
        string servoCode,
        bool ready,
        bool alarm,
        int torque
    );

    /* ================= DIGITAL INPUT ================= */
    Task PublishDigitalInputAsync(
        string tagKey,
        bool value
    );

    /* ================= DIGITAL OUTPUT ================= */
    Task PublishDigitalOutputAsync(
        string tagKey,
        bool value
    );
}
