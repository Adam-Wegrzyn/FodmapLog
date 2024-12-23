using DataAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Common.DTO
{
    internal static class UnitConverter
    {
        internal static double ConvertToGrams(double quantity, Unit unit)
        {
            return quantity * (double)unit;
        }
    }
}
