using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreDht.Utils
{
    public interface IRandomNumberGenerator
    {
        int Next();
        int Next(int minValue, int maxValue);
    }
}
