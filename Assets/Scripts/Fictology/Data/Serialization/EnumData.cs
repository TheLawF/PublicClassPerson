using System;
using System.Collections.Generic;
using System.IO;

namespace Fictology.Data.Serialization
{
    public class EnumData: INamedData
    {
        private int m_enum_count;
        private int m_enum_value;
        private bool m_has_flag;

        private EnumData(int count, bool hasFlag)
        {
            m_enum_count = count;
            m_has_flag = hasFlag;
            m_enum_value = m_has_flag ? 1 : 0;
        }
        
        private EnumData(Enum e)
        {
            var enumType = e.GetType();
            m_enum_count = Enum.GetValues(enumType).Length;
            m_enum_value = Convert.ToInt32(e);  // 更安全的转换

            // 检查枚举类型是否有[Flags]特性
            m_has_flag = enumType.IsDefined(typeof(FlagsAttribute), false);
        }

        public static EnumData Of(Enum e) => new(e);
        public static EnumData Create(int count, bool hasFlag) => new EnumData(count, hasFlag);


        public List<Enum> ToEnum(Type enumType)
        {
            var list = new List<Enum>();
    
            if (m_has_flag)
            {
                // 处理标志枚举
                for (var i = 0; i < 32; i++)  // int 是32位
                {
                    var mask = 1 << i;
                    if ((m_enum_value & mask) == 0) continue;
                    // 检查这个值是否在枚举中定义
                    if (Enum.IsDefined(enumType, mask))
                    {
                        list.Add((Enum)Enum.ToObject(enumType, mask));
                    }
                }
        
                // 如果没有找到任何标志，检查是否为0
                if (list.Count == 0 && Enum.IsDefined(enumType, 0))
                {
                    list.Add((Enum)Enum.ToObject(enumType, 0));
                }
            }
            else
            {
                // 处理非标志枚举
                if (Enum.IsDefined(enumType, m_enum_value))
                {
                    list.Add((Enum)Enum.ToObject(enumType, m_enum_value));
                }
            }
    
            return list;
        }
        
        public void EnableMask() => m_has_flag = true;

        public void SetMaskOnDigit(int digit, bool value)
        {
            if (!m_has_flag) return;
            // 验证位位置有效性
            if (digit < 0 || digit > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(digit), 
                    "位置必须在0-31之间（包含边界）");
            }
        
            if (value)
            {
                // 将指定位设置为1：使用按位或操作
                m_enum_value |= (1 << digit);
            }
            else
            {
                // 将指定位设置为0：先创建掩码的反码，然后使用按位与操作
                m_enum_value &= ~(1 << digit);
            }
        }
        
        public byte[] ToBytes()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            
            writer.Write(m_enum_count);
            writer.Write(m_enum_value);
            writer.Write(m_has_flag);
            return stream.ToArray();
        }

        public void FromBytes(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            using var reader = new BinaryReader(stream);

            m_enum_count = reader.ReadInt32();
            m_enum_value = reader.ReadInt32();
            m_has_flag = reader.ReadBoolean();
        }

        public SerializationType GetSerializedType()
        {
            return SerializationType.Object;
        }
    }
}