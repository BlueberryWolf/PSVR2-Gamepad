namespace PSVR2Gamepad.Communication
{
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
            buf[1 + 1] = 0x10;

            int common = 1 + 2; // start of common block
            buf[common + 0] = 0x03; // valid_flag0: COMPATIBLE_VIBRATION|HAPTICS_SELECT
            buf[common + 2] = strength; // DualShock 4 compatibility motor

            // Haptics selector bytes (set to 0)
            for (int i = 3; i <= 7; i++)
                buf[common + i] = 0x00;

            buf[common + 39] = 0x02; // valid_flag2
            buf[common + 41] = 0x02; // lightbar enable

            FillSenseChecksum(reportId, buf, 1, payloadLen);
            return buf;
        }

        public static byte[] CreateUsbRumblePacket(byte strength)
        {
            const byte reportId = 0x02;
            const int payloadLen = 47;
            var buf = new byte[1 + payloadLen];
            buf[0] = reportId;

            int common = 1 + 0; // start of common block
            buf[common + 0] = 0x03; // valid_flag0: COMPATIBLE_VIBRATION|HAPTICS_SELECT
            buf[common + 2] = strength; // DualShock 4 compatibility motor

            // Haptics selector bytes (set to 0)
            for (int i = 3; i <= 7; i++)
                buf[common + i] = 0x00;

            buf[common + 39] = 0x02; // valid_flag2
            buf[common + 41] = 0x02; // lightbar enable

            return buf;
        }

        private static void FillSenseChecksum(byte reportId, byte[] buffer, int payloadOffset, int payloadLength)
        {
            int dataLen = payloadLength - 4;
            if (dataLen <= 0) return;

            EnsureCrcTable();
            uint crc = 0xFFFFFFFFu;

            // Prefix bytes required by HID over BT transport: 0xA2, reportId
            crc = (crc >> 8) ^ _crcTable![(crc ^ 0xA2) & 0xFF];
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
