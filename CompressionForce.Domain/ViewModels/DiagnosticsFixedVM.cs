using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Domain.ViewModels
{
        public class DiagnosticsFixedVM
        {
            /* =========================
             * DIGITAL INPUTS – CARD 1
             * ========================= */

            public bool EmergencyControl { get; set; }
            public bool DoorSafety { get; set; }
            public bool TurretDriveTrip { get; set; }
            public bool LHSFeederDriveTrip { get; set; }
            public bool RHSFeederDriveTrip { get; set; }
            public bool LubeMotorTrip { get; set; }

        public bool S1UpperCam { get; set; }
        public bool S1LowerCam { get; set; }
        public bool S2UpperCam { get; set; }
        public bool S2LowerCam { get; set; }

        // RIGHT COLUMN DIGITAL INPUTS
        public bool S1MainPenRollerPos { get; set; }
        public bool S1PrePenRollerPos { get; set; }
        public bool S2MainPenRollerPos { get; set; }
        public bool S2PrePenRollerPos { get; set; }
        public bool TurretInPosition { get; set; }
        public bool S1ScrapperPos { get; set; }
        public bool S2ScrapperPos { get; set; }
        public bool RhsPowderLevel { get; set; }
        public bool LhsPowderLevel { get; set; }
        public bool LubeOilLow { get; set; }




        public bool S1InitialRej { get; set; }
        public bool S2InitialRej { get; set; }
        public bool S1Sample { get; set; }
        public bool S2Sample { get; set; }
        public bool S1Reject { get; set; }
        public bool S2Reject { get; set; }
        public bool Lubrication { get; set; }
        public bool RedLamp { get; set; }
        public bool YellowLamp { get; set; }
        public bool GreenLamp { get; set; }



            /* =========================
             * COUNTERS – CARD 4
             * ========================= */

            public long RevolutionCount { get; set; }
            public long EncoderActCount { get; set; }


    }

}
