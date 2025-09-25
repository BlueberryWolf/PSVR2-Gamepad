namespace PSVR2Gamepad.Communication
{
    internal static class RumbleFlags
    {
        public const byte ValidFlag0_CompatibleVibration = 0x01;
        public const byte ValidFlag0_HapticsSelect = 0x02;
        public const byte ValidFlag2_LightbarSetupControlEnable = 0x02;
        public const byte LightbarSetup_EnableLeds = 0x02;
        public const byte BtTag = 0x10;
        public const byte BtCrcSeed = 0xA2;

        // USB Output Report (0x02) Field Offsets (relative to payload start)
        public const int UsbOffsetLightbarFlag = 39;
        public const int UsbOffsetLightbarSetup = 41;

        // Bluetooth Output Report (0x31) Field Offsets (relative to payload start)
        // The "common" block in BT reports is offset by 2 bytes (seq_tag, tag)
        public const int BtOffsetLightbarFlag = 2 + 39;
        public const int BtOffsetLightbarSetup = 2 + 41;
    }

    public static class RumbleProtocol
    {
        private static uint[]? _crcTable;

        public static byte[] CreateBluetoothRumblePacket(byte strength, ref byte outSeq)
        {
            const byte reportId = 0x31;
            const int payloadLen = 77;
            var buf = new byte[1 + payloadLen];
            buf[0] = reportId;

            // seq_tag
            buf[1 + 0] = (byte)((outSeq & 0x0F) << 4);
            outSeq = (byte)((outSeq + 1) & 0x0F);

            // tag
            buf[1 + 1] = RumbleFlags.BtTag;

            int common = 1 + 2; // start of common block
            buf[common + 0] = RumbleFlags.ValidFlag0_CompatibleVibration | RumbleFlags.ValidFlag0_HapticsSelect; // valid_flag0
            buf[common + 2] = strength; // DualShock 4 compatibility motor

            // Haptics selector bytes (set to 0)
            for (int i = 3; i <= 7; i++)
                buf[common + i] = 0x00;

            buf[RumbleFlags.BtOffsetLightbarFlag] = RumbleFlags.ValidFlag2_LightbarSetupControlEnable;
            buf[RumbleFlags.BtOffsetLightbarSetup] = RumbleFlags.LightbarSetup_EnableLeds;

            FillSenseChecksum(reportId, buf, 1, payloadLen);
            return buf;
        }

        public static byte[] CreateUsbRumblePacket(byte strength)
        {
            const byte reportId = 0x02;
            const int payloadLen = 47;
            var buf = new byte[1 + payloadLen];
            buf[0] = reportId;

            // valid_flag0: COMPATIBLE_VIBRATION
            buf[1 + 0] = RumbleFlags.ValidFlag0_CompatibleVibration | RumbleFlags.ValidFlag0_HapticsSelect;
            buf[1 + 2] = strength; // Right motor (DS4 - strong rumble)
            // buf[1 + 3] is for the left motor (weak rumble), we can leave it as 0 for now.

            // valid_flag2: LIGHTBAR_SETUP_CONTROL_ENABLE is crucial for enabling rumble and extended reports on USB.
            buf[1 + RumbleFlags.UsbOffsetLightbarFlag] = RumbleFlags.ValidFlag2_LightbarSetupControlEnable;
            buf[1 + RumbleFlags.UsbOffsetLightbarSetup] = RumbleFlags.LightbarSetup_EnableLeds;

            return buf;
        }

        private static void FillSenseChecksum(byte reportId, byte[] buffer, int payloadOffset, int payloadLength)
        {
            int dataLen = payloadLength - 4;
            if (dataLen <= 0) return;

            EnsureCrcTable();
            uint crc = 0xFFFFFFFFu;

            // Prefix byte required by HID over BT transport
            crc = (crc >> 8) ^ _crcTable![(crc ^ RumbleFlags.BtCrcSeed) & 0xFF];
            crc = (crc >> 8) ^ _crcTable[(crc ^ reportId) & 0xFF];

            for (int i = 0; i < dataLen; i++)
            {
                crc = (crc >> 8) ^ _crcTable[(crc ^ buffer[payloadOffset + i]) & 0xFF];
            }

            crc ^= 0xFFFFFFFFu;
            int p = payloadOffset + dataLen;
            buffer[p + 0] = (byte)(crc & 0xFF);
            buffer[p + 1] = (byte)((crc >> 8) & 0xFF);
            buffer[p + 2] = (byte)((crc >> 16) & 0xFF);
            buffer[p + 3] = (byte)((crc >> 24) & 0xFF);
        }

        private static void EnsureCrcTable()
        {
            if (_crcTable != null) return;
            _crcTable = new uint[256];
            for (uint n = 0; n < 256; n++)
            {
                uint c = n;
                for (int k = 0; k < 8; k++)
                    c = ((c & 1) != 0) ? (0xEDB88320u ^ (c >> 1)) : (c >> 1);
                _crcTable[n] = c;
            }
        }
    }
}
