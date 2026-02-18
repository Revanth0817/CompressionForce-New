using System.Threading.Tasks;

namespace CompressionForce.Services
{
    public interface IServoStatusPublisher
    {
        /* ================= SERVO ================= */
        Task PublishServoAsync(
            string servoCode,
            bool ready,
            bool alarm,
            int torque,
            int actPos
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

        /* ================= ANALOG INPUT ================= */
        Task PublishAnalogInputAsync(
            string tagKey,
            double voltage,
            double force
        );
    }
}
