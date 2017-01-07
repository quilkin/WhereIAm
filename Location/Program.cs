using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Location
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Int32> savedLocations = new List<Int32>();
            int maxLocations = 20;
            int skips = 0;

            for (Int32 count = 0; count < 100; count++) {
                // miss out some values because storage has been exceeded
                int skip = skips-1;
                while (skip > 0)
                { ++count; --skip;  }

                if (savedLocations.Count < maxLocations)
                {
                    savedLocations.Add(count);
                }
                else
                {
                    // limit size of list for now, first remove any which are at same location
                    //for (int i = savedLocations.Count - 1; i >= 0; i -= 1)
                    //{
                    //    int loc = savedLocations[i];
                    //    //if (loc.same)
                    //    //    savedLocations.RemoveAt(i);
                    //}
                    if (savedLocations.Count >= maxLocations)
                    {
                        // still too many, remove every other one
                        for (int i = savedLocations.Count - 2; i >= 0; i -= 2)
                        {
                            savedLocations.RemoveAt(i);

                        }
                        if (skips == 0)
                            skips = 2;
                        else
                            skips = skips + skips;
                        --count;
                    }

                }
            }
        }
    }
}
