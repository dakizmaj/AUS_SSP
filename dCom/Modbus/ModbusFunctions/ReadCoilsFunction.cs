using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc/>
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

            frame[7] = p.FunctionCode;

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

            
            int coilIndex = 0;

            for (int i = 0; i < byteCount; i++)
            {
                byte currentByte = response[9 + i];

                for (int bit = 0; bit < 8; bit++)
                {
                    if (coilIndex >= p.Quantity)
                        break;

                    ushort value = (ushort)((currentByte >> bit) & 0x01);
                    ushort address = (ushort)(p.StartAddress + coilIndex);

                    result.Add(
                        new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, address),
                        value
                    );

                    coilIndex++;
                }
            }

            return result;
        }
    }
}