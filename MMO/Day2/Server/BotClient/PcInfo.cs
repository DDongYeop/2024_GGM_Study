using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Server;

public class PcInfo
{
    public int Index { get; set; }
    public FLocation Position { get; set; }
    public FLocation Destination { get; set; }
    public float Direction { get; set; }

    public PcInfo()
    {
    }

}
