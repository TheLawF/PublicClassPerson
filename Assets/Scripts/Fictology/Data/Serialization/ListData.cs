using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Fictology.Data.Serialization
{
    [Serializable]
    public class ListData: INamedData, ICollection<INamedData>
    {
        public SerializationType serializationType = SerializationType.Null;
        public string name;
        [SerializeReference] private List<INamedData> dataList;

        public ListData()
        {
            
        }
        public ListData(string name)
        {
            this.name = name;
            dataList = new List<INamedData>();
        }

        public void Add(INamedData data)
        {
            var type = data.GetSerializedType();
            serializationType = type switch
            {
                SerializationType.Integer => SerializationType.IntList,
                SerializationType.Float => SerializationType.FloatList,
                SerializationType.String => SerializationType.StringList,
                SerializationType.Object => SerializationType.ObjectList,
                _ => serializationType
            };
            dataList.Add(data);
        }

        public void Clear() => dataList.Clear();

        public bool Contains(INamedData item) => dataList.Contains(item);
        public void CopyTo(INamedData[] array, int arrayIndex) => dataList.CopyTo(array, arrayIndex);

        public bool Remove(INamedData item) => dataList.Remove(item);

        public int Count => dataList.Count;
        public bool IsReadOnly => false;

        public void AddRange<D>(List<D> listData) where D: INamedData => listData.ForEach(data => Add(data));
        
        public List<INamedData> Get() => dataList;
        
        public INamedData this[int index] => dataList[index];
        public byte[] ToBytes()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            
            writer.Write(dataList.Count);
            foreach (var data in dataList)
            {
                writer.Write((int)data.GetSerializedType());
                writer.Write(data.ToBytes().Length);
                writer.Write(data.ToBytes());
            }
            return stream.ToArray();
        }

        public void FromBytes(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);
            var count = reader.ReadInt32();
            for (var i = 0; i < count - 1; i++)
            {
                var type = reader.ReadInt32();
                var length = reader.ReadInt32();
                var dataBytes = reader.ReadBytes(length);
                var value = INamedData.Factory.Create((SerializationType)type);
                
                value.FromBytes(dataBytes);
                dataList.Add(value);
            }
        }

        public SerializationType GetSerializedType()
        {
            return serializationType;
        }

        public IEnumerator<INamedData> GetEnumerator() => dataList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}