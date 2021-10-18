using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppIndustryProtocolTestTool.Model.Enum
{
    public enum ModbusMasterFunctionEnum
    {
        Fn01_ReadCoils_0x,
        Fn02_ReadDiscreteInputs_1x,
        Fn03_ReadHoldingRegisters_4x,
        Fn04_ReadInputRegisters_3x,
        Fn05_WriteSingleCoil_0x,
        Fn06_WriteSingleRegister_4x,
        Fn15_WriteMultipleCoils_0x,
        Fn16_WriteMultipleRegisters_4x

    }
}
