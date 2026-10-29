using System.Buffers.Binary;
using System.Text;

namespace DllPatcher;

partial class Program
{
    // Edit raw MessagePack fields so container IDs, dictionaries, hash headers
    // and unrelated payloads survive byte-for-byte without a native rebake.
    sealed class RuneTicketJit
    {
        readonly byte[] header;
        readonly byte[][] containers;
        public IReadOnlyList<byte[]> Payloads => containers;
        public readonly string[] Strings;

        public RuneTicketJit(byte[] bytes)
        {
            using var reader = new BinaryReader(new MemoryStream(bytes));
            for (var i = 0; i < 2; i++)
            {
                var length = reader.ReadInt32();
                if (length < 0 || length > reader.BaseStream.Length - reader.BaseStream.Position)
                    throw new InvalidDataException("Invalid JIT hash header.");
                reader.ReadBytes(length);
            }
            header = bytes[..checked((int)reader.BaseStream.Position)];
            containers = Containers(bytes, header.Length);
            Strings = Containers(containers[^1], 0).Select(String).ToArray();
        }

        static byte[][] Containers(byte[] bytes, int start)
        {
            using var reader = new BinaryReader(new MemoryStream(bytes));
            reader.BaseStream.Position = start;
            var count = reader.ReadInt32();
            if (count <= 0 || count > (bytes.Length - start - 4) / 4)
                throw new InvalidDataException("Invalid JIT container count.");
            var offsets = Enumerable.Range(0, count).Select(_ => reader.ReadInt32()).Append(bytes.Length - start - 4 - count * 4).ToArray();
            var origin = checked((int)reader.BaseStream.Position);
            if (offsets[0] != 0 || offsets.Zip(offsets.Skip(1)).Any(p => p.First < 0 || p.First > p.Second))
                throw new InvalidDataException("Invalid JIT container offsets.");
            return Enumerable.Range(0, count).Select(i => bytes[(origin + offsets[i])..(origin + offsets[i + 1])]).ToArray();
        }

        public (int, List<byte[]>) Row(string code)
        {
            var matches = new List<(int, List<byte[]>)>();
            for (var i = 0; i < containers.Length - 1; i++)
            {
                if (containers[i].Length == 0 || !IsArray(containers[i][0])) continue;
                var row = Array(containers[i]);
                if (row.Count == 0) continue;
                var key = Integer(row[0]);
                if (key >= 0 && key < Strings.Length && Strings[key] == code) matches.Add((i, row));
            }
            if (matches.Count != 1) throw new InvalidDataException($"Expected one JIT row for {code}, found {matches.Count}.");
            return matches[0];
        }

        public void ReplaceRow(int index, List<byte[]> fields)
        {
            var original = containers[index];
            var headerLength = original[0] == 0xdc ? 3 : original[0] == 0xdd ? 5 : 1;
            containers[index] = original[..headerLength].Concat(fields.SelectMany(field => field)).ToArray();
        }

        public byte[] Encoded()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write(header);
            writer.Write(containers.Length);
            var offset = 0;
            foreach (var container in containers) { writer.Write(offset); offset = checked(offset + container.Length); }
            foreach (var container in containers) writer.Write(container);
            return stream.ToArray();
        }

        static bool IsArray(byte tag) => (tag & 0xf0) == 0x90 || tag is 0xdc or 0xdd;
        static uint Unsigned(BinaryReader reader, int count)
        {
            uint result = 0;
            for (var i = 0; i < count; i++) result = (result << 8) | reader.ReadByte();
            return result;
        }
        static int ArrayCount(BinaryReader reader)
        {
            var tag = reader.ReadByte();
            return (tag & 0xf0) == 0x90 ? tag & 15 : tag switch {
                0xdc => checked((int)Unsigned(reader, 2)), 0xdd => checked((int)Unsigned(reader, 4)),
                _ => throw new InvalidDataException("Expected MessagePack array.") };
        }
        public static List<byte[]> Array(byte[] bytes)
        {
            using var reader = new BinaryReader(new MemoryStream(bytes));
            var count = ArrayCount(reader);
            var fields = new List<byte[]>();
            for (var i = 0; i < count; i++)
            {
                var start = checked((int)reader.BaseStream.Position);
                Skip(reader, 0);
                fields.Add(bytes[start..checked((int)reader.BaseStream.Position)]);
            }
            if (reader.BaseStream.Position != bytes.Length) throw new InvalidDataException("Trailing MessagePack row data.");
            return fields;
        }
        static void Skip(BinaryReader reader, int depth)
        {
            if (depth > 64) throw new InvalidDataException("MessagePack nesting too deep.");
            var tag = reader.ReadByte();
            long size = 0;
            var count = 0;
            if (tag <= 0x7f || tag >= 0xe0 || tag is 0xc0 or 0xc2 or 0xc3) return;
            if ((tag & 0xe0) == 0xa0) size = tag & 31;
            else if ((tag & 0xf0) == 0x90) count = tag & 15;
            else if ((tag & 0xf0) == 0x80) count = (tag & 15) * 2;
            else switch (tag)
            {
                case 0xcc: case 0xd0: size = 1; break;
                case 0xcd: case 0xd1: size = 2; break;
                case 0xce: case 0xd2: case 0xca: size = 4; break;
                case 0xcf: case 0xd3: case 0xcb: size = 8; break;
                case 0xc4: case 0xd9: size = Unsigned(reader, 1); break;
                case 0xc5: case 0xda: size = Unsigned(reader, 2); break;
                case 0xc6: case 0xdb: size = Unsigned(reader, 4); break;
                case 0xdc: count = checked((int)Unsigned(reader, 2)); break;
                case 0xdd: count = checked((int)Unsigned(reader, 4)); break;
                case 0xde: count = checked((int)Unsigned(reader, 2) * 2); break;
                case 0xdf: count = checked((int)Unsigned(reader, 4) * 2); break;
                case 0xd4: size = 2; break;
                case 0xd5: size = 3; break;
                case 0xd6: size = 5; break;
                case 0xd7: size = 9; break;
                case 0xd8: size = 17; break;
                case 0xc7: size = (long)Unsigned(reader, 1) + 1; break;
                case 0xc8: size = (long)Unsigned(reader, 2) + 1; break;
                case 0xc9: size = (long)Unsigned(reader, 4) + 1; break;
                default: throw new InvalidDataException("Unsupported MessagePack token.");
            }
            if (size > reader.BaseStream.Length - reader.BaseStream.Position)
                throw new EndOfStreamException("Truncated MessagePack value.");
            reader.BaseStream.Position += size;
            for (var i = 0; i < count; i++) Skip(reader, depth + 1);
        }
        public static long Integer(byte[] bytes)
        {
            var tag = bytes[0];
            var data = bytes.AsSpan(1);
            return tag <= 0x7f ? tag : tag >= 0xe0 ? (sbyte)tag : tag switch {
                0xcc => data[0], 0xcd => BinaryPrimitives.ReadUInt16BigEndian(data),
                0xce => BinaryPrimitives.ReadUInt32BigEndian(data), 0xcf => checked((long)BinaryPrimitives.ReadUInt64BigEndian(data)),
                0xd0 => (sbyte)data[0], 0xd1 => BinaryPrimitives.ReadInt16BigEndian(data),
                0xd2 => BinaryPrimitives.ReadInt32BigEndian(data), 0xd3 => BinaryPrimitives.ReadInt64BigEndian(data),
                _ => throw new InvalidDataException("Expected MessagePack integer.") };
        }
        static string String(byte[] bytes)
        {
            using var reader = new BinaryReader(new MemoryStream(bytes));
            var tag = reader.ReadByte();
            var count = (tag & 0xe0) == 0xa0 ? tag & 31 : tag switch {
                0xd9 => checked((int)Unsigned(reader, 1)), 0xda => checked((int)Unsigned(reader, 2)),
                0xdb => checked((int)Unsigned(reader, 4)), _ => throw new InvalidDataException("Expected MessagePack string.") };
            if (count > bytes.Length - reader.BaseStream.Position) throw new InvalidDataException("Truncated JIT string.");
            return Encoding.UTF8.GetString(reader.ReadBytes(count));
        }
    }
}
