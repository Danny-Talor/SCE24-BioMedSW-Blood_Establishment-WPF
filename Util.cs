using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCE24_BioMedSW_Blood_Establishment_WPF
{
    internal class Util
    {
        public static bool IsValidIsraeliIDNumber(string id)
        {
            if (id.Length > 9) return false;
            if (id.Length < 9) while (id.Length != 9) id = "0" + id;
            int counter = 0, incNum, i;
            for (i = 0; i < 9; i++, counter += incNum)
            {
                incNum = (id[i] - '0') * ((i % 2) + 1);
                if (incNum > 9) incNum -= 9;
            }
            return (counter % 10 == 0);
        }
    }
}
