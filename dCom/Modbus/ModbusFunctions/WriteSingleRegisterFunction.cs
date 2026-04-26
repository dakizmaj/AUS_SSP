using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write single register functions/requests.
    /// </summary>
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleRegisterFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters p = (ModbusWriteCommandParameters)CommandParameters;

            byte[] frame = new byte[12];

            frame[0] = (byte)(p.TransactionId >> 8);
            frame[1] = (byte)(p.TransactionId & 0xFF);

            frame[2] = 0;
            frame[3] = 0;

            frame[4] = 0;
            frame[5] = 6;

            frame[6] = p.UnitId;

            frame[7] = p.FunctionCode; // 6

            frame[8] = (byte)(p.OutputAddress >> 8);
            frame[9] = (byte)(p.OutputAddress & 0xFF);

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

            result.Add(
                new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, address),
                value
            );

            return result;
        }
    }
}