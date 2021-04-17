using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class WorldSnapshot
{
    public int RoundNumber;
    public List<TankData> TankData;

    public long createTime;

    public WorldSnapshot()
    {
        TankData = new List<TankData>();

        createTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}

