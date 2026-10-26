namespace CompressionForce.Services.DTOs
{
    public class PlcStatusVm
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public string PlcIp { get; set; }

        public bool LocalDbConnected { get; set; }
        public bool PlcConnected { get; set; }

        // 🔹 NEW FIELDS
        public string UserLoginOn { get; set; }
        public string UserName { get; set; }
        public string UserLevel { get; set; }
        public string AppRunTime { get; set; }
        // UTC instant of the login (ISO 8601); empty when not logged in
        public string LoginTimeIso { get; set; }
    }
}
