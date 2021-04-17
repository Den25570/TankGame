using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InputBuffer
{
    private TimeInput[] buffer;
    private int writePos;

    public InputBuffer(int size = 2048)
    {
        writePos = 0;
        buffer = new TimeInput[size];
    }
    public void Write(Input input)
    {
        TimeInput timeInput = new TimeInput(input);
        buffer[writePos] = timeInput;
        writePos = (writePos + 1) % buffer.Length;
    }

    public void Write(TimeInput input)
    {
        buffer[writePos] = input;
        writePos = (writePos + 1) % buffer.Length;
    }

    public void InterpolateSnapshot(int clientIndex, ref WorldSnapshot snapshot)
    {
        long actualTime = snapshot.createTime - (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - snapshot.createTime);
        int i = writePos - 1 < 0 ? buffer.Length - 1 : writePos - 1;
        
        while (buffer[i] != null && actualTime <= buffer[i].time)
        {
            i--;
            if (i < 0)
                i = buffer.Length - 1;
        }

        int k = 0;
        var clientTank = snapshot.TankData[clientIndex];
        for (int j = (i + 1) % buffer.Length; j != writePos; j = (j+1) % buffer.Length)
        {
            k++;
            clientTank.Position += clientTank.Forward * buffer[j].input.Movement * clientTank.Speed * buffer[j].time;

            var turnRotation = Quaternion.Euler(0f, buffer[j].input.Rotating * clientTank.TurnSpeed * buffer[j].time, 0f);
            clientTank.Rotation *= turnRotation;
        }
    }
}

