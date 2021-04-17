using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using UnityEngine;

public class PacketBuffer : IDisposable
{
    List<byte> _bufferlist;
    byte[] _readbuffer;
    int _readpos;
    bool _buffupdate = false;

    public PacketBuffer()
    {
        _bufferlist = new List<byte>();
        _readpos = 0;
    }

    public int GetReadPos()
    {
        return _readpos;
    }
    public byte[] ToArray()
    {
        return _bufferlist.ToArray();
    }
    public int Count()
    {
        return _bufferlist.Count;
    }
    public int Length()
    {
        return Count() - _readpos;
    }
    public void Clear()
    {
        _bufferlist.Clear();
        _readpos = 0;
    }

    //WriteData
    public void WriteBytes(byte[] input)
    {
        _bufferlist.AddRange(input);
        _buffupdate = true;
    }
    public void WriteByte(byte input)
    {
        _bufferlist.Add(input);
        _buffupdate = true;
    }

    public void WriteBoolean(bool input)
    {
        _bufferlist.AddRange(BitConverter.GetBytes(input));
        _buffupdate = true;
    }
    public void WriteInt32(int input)
    {
        _bufferlist.AddRange(BitConverter.GetBytes(input));
        _buffupdate = true;
    }
    internal void WriteInt64(long input)
    {
        _bufferlist.AddRange(BitConverter.GetBytes(input));
        _buffupdate = true;
    }
    public void WriteFloat(float input)
    {
        _bufferlist.AddRange(BitConverter.GetBytes(input));
        _buffupdate = true;
    }
    public void WriteString(string input)
    {
        _bufferlist.AddRange(BitConverter.GetBytes(input.Length));
        _bufferlist.AddRange(Encoding.ASCII.GetBytes(input));
        _buffupdate = true;
    }

    public void WriteVector3(Vector3 input)
    {
        _bufferlist.AddRange(BitConverter.GetBytes(input.x));
        _bufferlist.AddRange(BitConverter.GetBytes(input.y));
        _bufferlist.AddRange(BitConverter.GetBytes(input.z));
        _buffupdate = true;
    }

    public void WriteQuaternion(Quaternion input)
    {
        _bufferlist.AddRange(BitConverter.GetBytes(input.x));
        _bufferlist.AddRange(BitConverter.GetBytes(input.y));
        _bufferlist.AddRange(BitConverter.GetBytes(input.z));
        _bufferlist.AddRange(BitConverter.GetBytes(input.w));
        _buffupdate = true;
    }



    //ReadData
    public bool ReadBoolean(bool peek = true)
    {
        if (_bufferlist.Count > _readpos)
        {
            if (_bufferlist.Count > _readpos)
            {
                _readbuffer = _bufferlist.ToArray();
                _buffupdate = false;
            }

            bool value = BitConverter.ToBoolean(_readbuffer, _readpos);
            if (peek & _bufferlist.Count > _readpos)
            {
                _readpos += sizeof(bool);
            }
            return value;
        }
        else
        {
            throw new Exception("Buffer is past it's Limit!");
        }
    }
    public int ReadInt32(bool peek = true)
    {
        if (_bufferlist.Count > _readpos)
        {
            if (_bufferlist.Count > _readpos)
            {
                _readbuffer = _bufferlist.ToArray();
                _buffupdate = false;
            }

            int value = BitConverter.ToInt32(_readbuffer, _readpos);
            if (peek & _bufferlist.Count > _readpos)
            {
                _readpos += 4;
            }
            return value;
        }
        else
        {
            throw new Exception("Buffer is past it's Limit!");
        }
    }
    public long ReadInt64(bool peek = true)
    {
        if (_bufferlist.Count > _readpos)
        {
            if (_bufferlist.Count > _readpos)
            {
                _readbuffer = _bufferlist.ToArray();
                _buffupdate = false;
            }

            long value = BitConverter.ToInt64(_readbuffer, _readpos);
            if (peek & _bufferlist.Count > _readpos)
            {
                _readpos += 8;
            }
            return value;
        }
        else
        {
            throw new Exception("Buffer is past it's Limit!");
        }
    }
    public float ReadFloat(bool peek = true)
    {
        if (_bufferlist.Count > _readpos)
        {
            if (_bufferlist.Count > _readpos)
            {
                _readbuffer = _readbuffer.ToArray();
                _buffupdate = false;
            }

            float value = BitConverter.ToSingle(_readbuffer, _readpos);
            if (peek & _bufferlist.Count > _readpos)
            {
                _readpos += 4;
            }
            return value;
        }
        else
        {
            throw new Exception("Buffer is past it's Limit!");
        }

    }
    public byte ReadByte(bool peek = true)
    {
        if (_bufferlist.Count > _readpos)
        {
            if (_bufferlist.Count > _readpos)
            {
                _readbuffer = _readbuffer.ToArray();
                _buffupdate = false;
            }

            byte value = _readbuffer[_readpos];
            if (peek & _bufferlist.Count > _readpos)
            {
                _readpos += 1;
            }
            return value;
        }
        else
        {
            throw new Exception("Buffer is past it's Limit!");
        }
    }

    public byte[] ReadBytes(int length, bool peek = true)
    {
        if (_bufferlist.Count > _readpos)
        {
            _readbuffer = _readbuffer.ToArray();
            _buffupdate = false;
        }

        byte[] value = _bufferlist.GetRange(_readpos, length).ToArray();
        if (peek & _bufferlist.Count > _readpos)
        {
            _readpos += length;
        }
        return value;
    }
    public string ReadString(bool peek = true)
    {
        int length = ReadInt32();

        if (_bufferlist.Count > _readpos)
        {
            _readbuffer = _readbuffer.ToArray();
            _buffupdate = false;
        }

        string value = Encoding.ASCII.GetString(_readbuffer, _readpos, length);
        if (peek & _bufferlist.Count > _readpos)
        {
            _readpos += length;
        }
        return value;
    }

    public Vector3 ReadVector3(bool peek = true)
    {
        var value = new Vector3();

        int tmpReadPos = _readpos;

        value.x = ReadFloat();
        value.y = ReadFloat();
        value.z = ReadFloat();

        if (!peek)
            _readpos = tmpReadPos;

        return value;
    }

    public Quaternion ReadQuaternion(bool peek = true)
    {
        var value = new Quaternion();

        int tmpReadPos = _readpos;

        value.x = ReadFloat();
        value.y = ReadFloat();
        value.z = ReadFloat();
        value.w = ReadFloat();

        if (!peek)
            _readpos = tmpReadPos;

        return value;
    }

    //IDisposable
    private bool disposedValue = false;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _bufferlist.Clear();
            }
            _readpos = 0;
        }
        disposedValue = true;
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

}
