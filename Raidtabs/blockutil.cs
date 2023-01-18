using System.Reflection;
using System.Runtime.Serialization;
using PKHeX.Core;

namespace pk9reader
{
    public static class BlockUtil
    {
        public static SCBlock CreateObjectBlock(uint key, ReadOnlySpan<byte> data)
        {
            var block = (SCBlock)FormatterServices.GetUninitializedObject(typeof(SCBlock));
            var keyInfo = typeof(SCBlock).GetField("Key", BindingFlags.Instance | BindingFlags.Public)!;
            keyInfo.SetValue(block, key);
            var typeInfo = typeof(SCBlock).GetProperty("Type")!;
            typeInfo.SetValue(block, SCTypeCode.Object);
            var dataInfo = typeof(SCBlock).GetField("Data", BindingFlags.Instance | BindingFlags.Public)!;
            dataInfo.SetValue(block, data.ToArray());
            return block;
        }

        public static SCBlock CreateBoolBlock(uint key, SCTypeCode boolean)
        {
            var block = (SCBlock)FormatterServices.GetUninitializedObject(typeof(SCBlock));
            var keyInfo = typeof(SCBlock).GetField("Key", BindingFlags.Instance | BindingFlags.Public)!;
            keyInfo.SetValue(block, key);
            var typeInfo = typeof(SCBlock).GetProperty("Type")!;
            typeInfo.SetValue(block, boolean);
            var dataInfo = typeof(SCBlock).GetField("Data", BindingFlags.Instance | BindingFlags.Public)!;
            dataInfo.SetValue(block, Array.Empty<byte>());
            return block;
        }

        public static void AddBlockToFakeSAV(SAV9SV sav, SCBlock block)
        {
            var list = new List<SCBlock>();
            foreach (var b in sav.Accessor.BlockInfo) list.Add(b);
            list.Add(block);
            var typeInfo = typeof(SAV9SV).GetField("<AllBlocks>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;
            typeInfo.SetValue(sav, list);
            typeInfo = typeof(SaveBlockAccessor9SV).GetField("<BlockInfo>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!;
            typeInfo.SetValue(sav.Blocks, list);
        }

        public static void EditBlock(SCBlock block, SCTypeCode type, ReadOnlySpan<byte> data)
        {
            var typeInfo = typeof(SCBlock).GetProperty("Type")!;
            typeInfo.SetValue(block, type);
            var dataInfo = typeof(SCBlock).GetField("Data", BindingFlags.Instance | BindingFlags.Public)!;
            dataInfo.SetValue(block, data.ToArray());
        }

        public static SCBlock FindOrDefault(this SCBlockAccessor Accessor, uint Key) => Accessor.BlockInfo.FindOrDefault(Key);

        public static SCBlock FindOrDefault(this IReadOnlyList<SCBlock> blocks, uint key)
        {
            var res = blocks.Where(block => block.Key == key).FirstOrDefault();
            return res is not null ? res : CreateBoolBlock(key, SCTypeCode.None);
        }

        public static byte[] DecryptBlock(uint key, byte[] block)
        {
            var rng = new SCXorShift32(key);
            for (int i = 0; i < block.Length; i++)
                block[i] = (byte)(block[i] ^ rng.Next());
            return block;
        }
    }

    public class DataBlock
    {
        public string? Name { get; set; }
        public uint Key { get; set; }
        public SCTypeCode Type { get; set; }
        public SCTypeCode SubType { get; set; }
        public IReadOnlyList<long>? Pointer { get; set; }
    }

    public static class Blocks
    {
        public static DataBlock KTeraRaids = new()
        {
            Name = "KTeraRaids",
            Key = 0xCAAC8800,
            Type = SCTypeCode.Object,
            Pointer = new long[] { 0x43A77C8, 0x160, 0x40 },
        };

        public static DataBlock KMyStatus = new()
        {
            Name = "KMyStatus",
            Key = 0xE3E89BD1,
            Type = SCTypeCode.Object,
            Pointer = new long[] { 0x4385F30, 0x80, 0x8, 0x29F40 }, //Ty Lincoln-LM!
        };

        public static DataBlock KBCATEventRaidIdentifier = new()
        {
            Name = "KBCATEventRaidIdentifier",
            Key = 0x37B99B4D,
            Type = SCTypeCode.Object,
            Pointer = new long[] { 0x4385F30, 0x80, 0x8, 0x0 }, //TODO
        };

        public static DataBlock KBCATFixedRewardItemArray = new()
        {
            Name = "KBCATFixedRewardItemArray",
            Key = 0x7D6C2B82,
            Type = SCTypeCode.Object,
            Pointer = new long[] { 0x4385F30, 0x80, 0x8, 0x16D40 } //Ty Lincoln-LM!
        };

        public static DataBlock KBCATLotteryRewardItemArray = new()
        {
            Name = "KBCATLotteryRewardItemArray",
            Key = 0xA52B4811,
            Type = SCTypeCode.Object,
            Pointer = new long[] { 0x4385F30, 0x80, 0x8, 0x1E6A0 } //Ty Lincoln-LM!
        };

        public static DataBlock KBCATRaidEnemyArray = new()
        {
            Name = "KBCATRaidEnemyArray",
            Key = 0x0520A1B0,
            Type = SCTypeCode.Object,
            Pointer = new long[] { 0x4385F30, 0x80, 0x8, 0x1040 } //Ty Lincoln-LM!
        };

        public static DataBlock KBCATRaidPriorityArray = new()
        {
            Name = "KBCATRaidPriorityArray",
            Key = 0x095451E4,
            Type = SCTypeCode.Object,
            Pointer = new long[] { 0x4385F30, 0x80, 0x8, 0x1860 } //Ty Lincoln-LM!
        };

        public static DataBlock KUnlockedTeraRaidBattles = new()
        {
            Name = "KUnlockedTeraRaidBattles",
            Key = 0x27025EBF,
            Type = SCTypeCode.Bool1,
            Pointer = new long[] { 0x4385F30, 0x80, 0x8, 0x0 } //TODO
        };

        public static DataBlock KUnlockedRaidDifficulty3 = new()
        {
            Name = "KUnlockedRaidDifficulty3",
            Key = 0xEC95D8EF,
            Type = SCTypeCode.Bool1,
            Pointer = new long[] { 0x4385F30, 0x80, 0x8, 0x2BF20 } //Ty Lincoln-LM!
        };

        public static DataBlock KUnlockedRaidDifficulty4 = new()
        {
            Name = "KUnlockedRaidDifficulty4",
            Key = 0xA9428DFE,
            Type = SCTypeCode.Bool1,
            Pointer = new long[] { 0x4385F30, 0x80, 0x8, 0x1F400 } //Ty Lincoln-LM!
        };

        public static DataBlock KUnlockedRaidDifficulty5 = new()
        {
            Name = "KUnlockedRaidDifficulty5",
            Key = 0x9535F471,
            Type = SCTypeCode.Bool1,
            Pointer = new long[] { 0x4385F30, 0x80, 0x8, 0x1B640 } //Ty Lincoln-LM!
        };

        public static DataBlock KUnlockedRaidDifficulty6 = new()
        {
            Name = "KUnlockedRaidDifficulty6",
            Key = 0x6E7F8220,
            Type = SCTypeCode.Bool1,
            Pointer = new long[] { 0x4385F30, 0x80, 0x8, 0x13EC0 } //Ty Lincoln-LM!
        };
    }
}
