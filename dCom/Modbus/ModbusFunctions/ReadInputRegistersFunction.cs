using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read input registers functions/requests.
    /// </summary>
    public class ReadInputRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadInputRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadInputRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters p = (ModbusReadCommandParameters)CommandParameters;

            byte[] frame = new byte[12];

            frame[0] = (byte)(p.TransactionId >> 8);
            frame[1] = (byte)(p.TransactionId & 0xFF);

            frame[2] = 0;
            frame[3] = 0;

            frame[4] = 0;
            frame[5] = 6;

            frame[6] = p.UnitId;

            frame[7] = p.FunctionCode; // READ_INPUT_REGISTERS = 4

            frame[8] = (byte)(p.StartAddress >> 8);
            frame[9] = (byte)(p.StartAddress & 0xFF);

            frame[10] = (byte)(p.Quantity >> 8);
            frame[11] = (byte)(p.Quantity & 0xFF);

            return frame;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            ModbusReadCommandParameters p = (ModbusReadCommandParameters)CommandParameters;

            byte byteCount = response[8];

            int registerIndex = 0;

            for (int i = 0; i < byteCount; i += 2)
            {
                ushort value = (ushort)((response[9 + i] << 8) | response[9 + i + 1]);

                ushort address = (ushort)(p.StartAddress + registerIndex);

                result.Add(
                    new Tuple<PointType, ushort>(PointType.ANALOG_INPUT, address),
                    value
                );

                registerIndex++;
            }

            return result;
        }
    }
}