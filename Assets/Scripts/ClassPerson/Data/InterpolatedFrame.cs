using Fictology.Data.Serialization;
using UnityEngine;

namespace ClassPerson.Data
{
    public record InterpolatedFrame(int FrameIndex, Spline Spline, INamedData First, INamedData Second): ISynchronizable
    {
        public byte[] ToBytes()
        {
            throw new System.NotImplementedException();
        }

        public void FromBytes(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }
    }

    public enum Spline
    {
        Saw,
        Sine,
        Triangular
    }
}