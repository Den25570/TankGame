using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TimeInput
{
    public Input input;
    public long time;

    public TimeInput(Input input)
    {
        this.input = input;
        time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}

public class Input
{
    public int Index;

    public float Movement;
    public float Rotating;

    public float DeltaTime;

    public Input() { }
}

