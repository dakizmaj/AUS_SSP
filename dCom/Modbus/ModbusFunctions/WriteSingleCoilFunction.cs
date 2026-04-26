using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write coil functions/requests.
    /// </summary>
    public class WriteSingleCoilFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleCoilFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters p = (ModbusWriteCommandParameters)CommandParameters;

            byte[] frame = new byte[12];

            // Transaction ID
            frame[0] = (byte)(p.TransactionId >> 8);
            frame[1] = (byte)(p.TransactionId & 0xFF);

            // Protocol ID
            frame[2] = 0;
            frame[3] = 0;

            // Length
            frame[4] = 0;
            frame[5] = 6;

            // Unit ID
            frame[6] = p.UnitId;

            // Function code (5)
            frame[7] = p.FunctionCode;

            // Address
            frame[8] = (byte)(p.OutputAddress >> 8);
            frame[9] = (byte)(p.OutputAddress & 0xFF);

            // Value (0xFF00 = ON, 0x0000 = OFF)
            ushort value = p.Value;

            frame[10] = (byte)(value >> 8);
            frame[11] = (byte)(value & 0xFF);

            return frame;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            ushort address = (ushort)((response[8] << 8) | response[9]);
            ushort value = (ushort)((response[10] << 8) | response[11]);

            ushort parsedValue = (value == 0xFF00) ? (ushort)1 : (ushort)0;

            result.Add(
                new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, address),
                parsedValue
            );

            return result;
        }
    }
}