using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Buffers.Binary.BinaryPrimitives;

namespace pk9reader
{
    public class PK9 : PKM

    {
        public byte[] pkdata;
        public PK9(byte[] data) : base(DecryptParty(data))
        {
            pkdata = data;
        }
        private static byte[] DecryptParty(byte[] data)
        {
            PokeCrypto.DecryptIfEncrypted8(ref data);
            Array.Resize(ref data, 0x158);
            return data;
        }
        private static readonly ushort[] Unused =
{
        // Alignment bytes
        0x17, 0x1A, 0x1B, 0x23, 0x33, 0x3E, 0x3F,
        0x4C, 0x4D, 0x4E, 0x4F,
        0x52, 0x53, 0x54, 0x55, 0x56, 0x57,

        0x91, 0x92, 0x93,
        0x9C, 0x9D, 0x9E, 0x9F, 0xA0, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7,

        0xC5,
        0xCE, 0xCF, 0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5, 0xD6, 0xD7, 0xD8, 0xD9, 0xDA, 0xDB,
        0xE0, 0xE1, // Old Console Region / Region
        0xE9, 0xEA, 0xEB, 0xEC, 0xED, 0xEE, 0xEF, 0xF0, 0xF1, 0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7,
        0x115, 0x11F, // Alignment

        0x13D, 0x13E, 0x13F,
        0x140, 0x141, 0x142, 0x143, 0x144, 0x145, 0x146, 0x147,
    };
        private ushort CalculateChecksum()
        {
            ushort chk = 0;
            for (int i = 8; i < 0x148; i += 2)
                chk += ReadUInt16LittleEndian(Data.AsSpan(i));
            return chk;
        }
        public override int SIZE_PARTY => 0x158;
        public override int SIZE_STORED => 0x148;
        public sealed override bool ChecksumValid => CalculateChecksum() == Checksum;
        public sealed override void RefreshChecksum() => Checksum = CalculateChecksum();
        public sealed override bool Valid { get => Sanity == 0 && ChecksumValid; set { if (!value) return; Sanity = 0; RefreshChecksum(); } }
        public override int MaxIV => 31;
        public override int MaxEV => 252;
        public override int OTLength => 12;
        public override int NickLength => 12;

        public override int PSV => (int)(((PID >> 16) ^ (PID & 0xFFFF)) >> 4);
        public override int TSV => (TID ^ SID) >> 4;
        public override bool IsUntraded => Data[0xA8] == 0 && Data[0xA8 + 1] == 0 && Format == Generation; // immediately terminated HT_Name data (\0)

        // Complex Generated Attributes
        public override int Characteristic
        {
            get
            {
                int pm6 = (int)(EncryptionConstant % 6);
                int maxIV = MaximumIV;
                int pm6stat = 0;
                for (int i = 0; i < 6; i++)
                {
                    pm6stat = (pm6 + i) % 6;
                    if (GetIV(pm6stat) == maxIV)
                        break;
                }
                return (pm6stat * 5) + (maxIV % 5);
            }
        }

        // Methods
        protected override byte[] Encrypt()
        {
            RefreshChecksum();
            return PokeCrypto.EncryptArray8(Data);
        }
        public override int CurrentFriendship
        {
            get => CurrentHandler == 0 ? OT_Friendship : HT_Friendship;
            set { if (CurrentHandler == 0) OT_Friendship = value; else HT_Friendship = value; }
        }

        public override Span<byte> Nickname_Trash => pkdata.AsSpan(0x58, 26);
        public override Span<byte> HT_Trash => pkdata.AsSpan(0xA8, 26);
        public override Span<byte> OT_Trash => pkdata.AsSpan(0xF8, 26);
        public override uint EncryptionConstant { get => ReadUInt32LittleEndian(pkdata.AsSpan(0x00)); set => WriteUInt32LittleEndian(pkdata.AsSpan(0x00), value); }
        public ushort Sanity { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x04)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x04), value); }
        public ushort Checksum { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x06)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x06), value); }
        public override ushort Species { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x08)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x08), value); }
        public override int HeldItem { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x0A)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x0A), (ushort)value); }
        public override int TID { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x0C)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x0C), (ushort)value); }
        public override int SID { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x0E)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x0E), (ushort)value); }
        public override uint EXP { get => ReadUInt32LittleEndian(pkdata.AsSpan(0x10)); set => WriteUInt32LittleEndian(pkdata.AsSpan(0x10), value); }
        public override int Ability { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x14)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x14), (ushort)value); }
        public override int AbilityNumber { get => pkdata[0x16] & 7; set => pkdata[0x16] = (byte)((pkdata[0x16] & ~7) | (value & 7)); }
        public bool Favorite { get => (pkdata[0x16] & 8) != 0; set => pkdata[0x16] = (byte)((pkdata[0x16] & ~8) | ((value ? 1 : 0) << 3)); } // unused, was in LGPE but not in SWSH
        public bool CanGigantamax { get => (pkdata[0x16] & 16) != 0; set => pkdata[0x16] = (byte)((pkdata[0x16] & ~16) | (value ? 16 : 0)); }
        // 0x17 alignment unused
        public override int MarkValue { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x18)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x18), (ushort)value); }
        // 0x1A alignment unused
        // 0x1B alignment unused
        public override uint PID { get => ReadUInt32LittleEndian(pkdata.AsSpan(0x1C)); set => WriteUInt32LittleEndian(pkdata.AsSpan(0x1C), value); }
        public override int Nature { get => pkdata[0x20]; set => pkdata[0x20] = (byte)value; }
        public override int StatNature { get => pkdata[0x21]; set => pkdata[0x21] = (byte)value; }
        public override bool FatefulEncounter { get => (pkdata[0x22] & 1) == 1; set => pkdata[0x22] = (byte)((pkdata[0x22] & ~0x01) | (value ? 1 : 0)); }
        public bool Flag2 { get => (pkdata[0x22] & 2) == 2; set => pkdata[0x22] = (byte)((pkdata[0x22] & ~0x02) | (value ? 2 : 0)); }
        public override int Gender { get => (pkdata[0x22] >> 2) & 0x3; set => pkdata[0x22] = (byte)((pkdata[0x22] & 0xF3) | (value << 2)); }
        // 0x23 alignment unused

        public override byte Form { get => pkdata[0x24]; set => WriteUInt16LittleEndian(pkdata.AsSpan(0x24), value); }
        public override int EV_HP { get => pkdata[0x26]; set => pkdata[0x26] = (byte)value; }
        public override int EV_ATK { get => pkdata[0x27]; set => pkdata[0x27] = (byte)value; }
        public override int EV_DEF { get => pkdata[0x28]; set => pkdata[0x28] = (byte)value; }
        public override int EV_SPE { get => pkdata[0x29]; set => pkdata[0x29] = (byte)value; }
        public override int EV_SPA { get => pkdata[0x2A]; set => pkdata[0x2A] = (byte)value; }
        public override int EV_SPD { get => pkdata[0x2B]; set => pkdata[0x2B] = (byte)value; }
        public uint Sociability { get => ReadUInt32LittleEndian(pkdata.AsSpan(0x48)); set => WriteUInt32LittleEndian(pkdata.AsSpan(0x48), value); }

        // 0x4C-0x4F unused

        public byte HeightScalar { get => pkdata[0x50]; set => pkdata[0x50] = value; }
        public byte WeightScalar { get => pkdata[0x51]; set => pkdata[0x51] = value; }
        public override string Nickname
        {
            get => StringConverter8.GetString(Nickname_Trash);
            set => StringConverter8.SetString(Nickname_Trash, value.AsSpan(), 12, StringConverterOption.None);
        }
        public override ushort Move1 { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x72)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x72), value); }
        public override ushort Move2 { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x74)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x74), value); }
        public override ushort Move3 { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x76)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x76), value); }
        public override ushort Move4 { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x78)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x78), value); }

        public override int Move1_PP { get => pkdata[0x7A]; set => pkdata[0x7A] = (byte)value; }
        public override int Move2_PP { get => pkdata[0x7B]; set => pkdata[0x7B] = (byte)value; }
        public override int Move3_PP { get => pkdata[0x7C]; set => pkdata[0x7C] = (byte)value; }
        public override int Move4_PP { get => pkdata[0x7D]; set => pkdata[0x7D] = (byte)value; }
        public override int Move1_PPUps { get => pkdata[0x7E]; set => pkdata[0x7E] = (byte)value; }
        public override int Move2_PPUps { get => pkdata[0x7F]; set => pkdata[0x7F] = (byte)value; }
        public override int Move3_PPUps { get => pkdata[0x80]; set => pkdata[0x80] = (byte)value; }
        public override int Move4_PPUps { get => pkdata[0x81]; set => pkdata[0x81] = (byte)value; }

        public override ushort RelearnMove1 { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x82)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x82), value); }
        public override ushort RelearnMove2 { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x84)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x84), value); }
        public override ushort RelearnMove3 { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x86)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x86), value); }
        public override ushort RelearnMove4 { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x88)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x88), value); }

        public override int Stat_HPCurrent { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x8A)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x8A), (ushort)value); }

        private uint IV32 { get => ReadUInt32LittleEndian(pkdata.AsSpan(0x8C)); set => WriteUInt32LittleEndian(pkdata.AsSpan(0x8C), value); }
        public override int IV_HP { get => (int)(IV32 >> 00) & 0x1F; set => IV32 = (IV32 & ~(0x1Fu << 00)) | ((value > 31 ? 31u : (uint)value) << 00); }
        public override int IV_ATK { get => (int)(IV32 >> 05) & 0x1F; set => IV32 = (IV32 & ~(0x1Fu << 05)) | ((value > 31 ? 31u : (uint)value) << 05); }
        public override int IV_DEF { get => (int)(IV32 >> 10) & 0x1F; set => IV32 = (IV32 & ~(0x1Fu << 10)) | ((value > 31 ? 31u : (uint)value) << 10); }
        public override int IV_SPE { get => (int)(IV32 >> 15) & 0x1F; set => IV32 = (IV32 & ~(0x1Fu << 15)) | ((value > 31 ? 31u : (uint)value) << 15); }
        public override int IV_SPA { get => (int)(IV32 >> 20) & 0x1F; set => IV32 = (IV32 & ~(0x1Fu << 20)) | ((value > 31 ? 31u : (uint)value) << 20); }
        public override int IV_SPD { get => (int)(IV32 >> 25) & 0x1F; set => IV32 = (IV32 & ~(0x1Fu << 25)) | ((value > 31 ? 31u : (uint)value) << 25); }
        public override bool IsEgg { get => ((IV32 >> 30) & 1) == 1; set => IV32 = (IV32 & ~0x40000000u) | (value ? 0x40000000u : 0u); }
        public override bool IsNicknamed { get => ((IV32 >> 31) & 1) == 1; set => IV32 = (IV32 & 0x7FFFFFFFu) | (value ? 0x80000000u : 0u); }

        public byte DynamaxLevel { get => pkdata[0x90]; set => pkdata[0x90] = value; }

        // 0x91-0x93 unused
        public override int Status_Condition { get; set; }
        public  byte MainType { get => pkdata[0x94]; set => pkdata[0x94]= value; }
        public byte TeraType { get => pkdata[0x95]; set=> pkdata[0x95]=value; }
        public override string HT_Name
        {
            get => StringConverter8.GetString(HT_Trash);
            set => StringConverter8.SetString(HT_Trash, value.AsSpan(), 12, StringConverterOption.None);
        }

        public override int HT_Gender { get => pkdata[0xC2]; set => pkdata[0xC2] = (byte)value; }
        public byte HT_Language { get => pkdata[0xC3]; set => pkdata[0xC3] = value; }
        public override int CurrentHandler { get => pkdata[0xC4]; set => pkdata[0xC4] = (byte)value; }
        // 0xC5 unused (alignment)
        public int HT_TrainerID { get => ReadUInt16LittleEndian(pkdata.AsSpan(0xC6)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0xC6), (ushort)value); } // unused?
        public override int HT_Friendship { get => pkdata[0xC8]; set => pkdata[0xC8] = (byte)value; }
        public  byte HT_Intensity { get => pkdata[0xC9]; set => pkdata[0xC9] = value; }
        public byte HT_Memory { get => pkdata[0xCA]; set => pkdata[0xCA] = value; }
        public  byte HT_Feeling { get => pkdata[0xCB]; set => pkdata[0xCB] = value; }
        public ushort HT_TextVar { get => ReadUInt16LittleEndian(pkdata.AsSpan(0xCC)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0xCC), value); }
        public override int Version { get => pkdata[0xCE]; set => pkdata[0xCE] = (byte)value; }
        public override int Language { get => pkdata[0xD5]; set => pkdata[0xD5] = (byte)value; }
        public override string OT_Name
        {
            get => StringConverter8.GetString(OT_Trash);
            set => StringConverter8.SetString(OT_Trash, value.AsSpan(), 12, StringConverterOption.None);
        }

        public override int OT_Friendship { get => pkdata[0x112]; set => pkdata[0x112] = (byte)value; }
        public byte OT_Intensity { get => pkdata[0x113]; set => pkdata[0x113] = value; }
        public byte OT_Memory { get => pkdata[0x114]; set => pkdata[0x114] = value; }
        // 0x115 unused align
        public ushort OT_TextVar { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x116)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x116), value); }
        public byte OT_Feeling { get => pkdata[0x118]; set => pkdata[0x118] = value; }
        public override int Egg_Year { get => pkdata[0x119]; set => pkdata[0x119] = (byte)value; }
        public override int Egg_Month { get => pkdata[0x11A]; set => pkdata[0x11A] = (byte)value; }
        public override int Egg_Day { get => pkdata[0x11B]; set => pkdata[0x11B] = (byte)value; }
        public override int Met_Year { get => pkdata[0x11C]; set => pkdata[0x11C] = (byte)value; }
        public override int Met_Month { get => pkdata[0x11D]; set => pkdata[0x11D] = (byte)value; }
        public override int Met_Day { get => pkdata[0x11E]; set => pkdata[0x11E] = (byte)value; }
        // 0x11F unused align
        public override int Egg_Location { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x120)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x120), (ushort)value); }
        public override int Met_Location { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x122)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x122), (ushort)value); }
        public override int Ball { get => pkdata[0x124]; set => pkdata[0x124] = (byte)value; }
        public override int Met_Level { get => pkdata[0x125] & ~0x80; set => pkdata[0x125] = (byte)((pkdata[0x125] & 0x80) | value); }
        public override int OT_Gender { get => pkdata[0x125] >> 7; set => pkdata[0x125] = (byte)((pkdata[0x125] & ~0x80) | (value << 7)); }
        public byte HyperTrainFlags { get => pkdata[0x126]; set => pkdata[0x126] = value; }
        public bool HT_HP { get => ((HyperTrainFlags >> 0) & 1) == 1; set => HyperTrainFlags = (byte)((HyperTrainFlags & ~(1 << 0)) | ((value ? 1 : 0) << 0)); }
        public bool HT_ATK { get => ((HyperTrainFlags >> 1) & 1) == 1; set => HyperTrainFlags = (byte)((HyperTrainFlags & ~(1 << 1)) | ((value ? 1 : 0) << 1)); }
        public bool HT_DEF { get => ((HyperTrainFlags >> 2) & 1) == 1; set => HyperTrainFlags = (byte)((HyperTrainFlags & ~(1 << 2)) | ((value ? 1 : 0) << 2)); }
        public bool HT_SPA { get => ((HyperTrainFlags >> 3) & 1) == 1; set => HyperTrainFlags = (byte)((HyperTrainFlags & ~(1 << 3)) | ((value ? 1 : 0) << 3)); }
        public bool HT_SPD { get => ((HyperTrainFlags >> 4) & 1) == 1; set => HyperTrainFlags = (byte)((HyperTrainFlags & ~(1 << 4)) | ((value ? 1 : 0) << 4)); }
        public bool HT_SPE { get => ((HyperTrainFlags >> 5) & 1) == 1; set => HyperTrainFlags = (byte)((HyperTrainFlags & ~(1 << 5)) | ((value ? 1 : 0) << 5)); }
        public ulong Tracker
        {
            get => ReadUInt64LittleEndian(pkdata.AsSpan(0x135));
            set => WriteUInt64LittleEndian(pkdata.AsSpan(0x135), value);
        }


        #region Battle Stats
        public override int Stat_Level { get => pkdata[0x148]; set => pkdata[0x148] = (byte)value; }
        // 0x149 unused alignment
        public override int Stat_HPMax { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x14A)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x14A), (ushort)value); }
        public override int Stat_ATK { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x14C)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x14C), (ushort)value); }
        public override int Stat_DEF { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x14E)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x14E), (ushort)value); }
        public override int Stat_SPE { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x150)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x150), (ushort)value); }
        public override int Stat_SPA { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x152)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x152), (ushort)value); }
        public override int Stat_SPD { get => ReadUInt16LittleEndian(pkdata.AsSpan(0x154)); set => WriteUInt16LittleEndian(pkdata.AsSpan(0x154), (ushort)value); }
        public override void SetMarking(int index, int value)
        {
            if ((uint)index >= MarkingCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            var shift = index * 2;
            MarkValue = (MarkValue & ~(0b11 << shift)) | ((value & 3) << shift);
        }
        private byte PKRS { get => Data[0x32]; set => Data[0x32] = value; }
        public override int PKRS_Days { get => PKRS & 0xF; set => PKRS = (byte)((PKRS & ~0xF) | value); }
        public override int PKRS_Strain { get => PKRS >> 4; set => PKRS = (byte)((PKRS & 0xF) | (value << 4)); }
        public override IReadOnlyList<ushort> ExtraBytes => Unused;
        public override PersonalInfo PersonalInfo => PersonalTable.SWSH.GetFormEntry(Species, Form);
        public override bool IsNative => SWSH;
        public override EntityContext Context => EntityContext.Gen8;
        public override ushort MaxMoveID => 826;
        public override ushort MaxSpeciesID => 1014;
        public override int MaxAbilityID => 267;
        public override int MaxItemID => 1607;
        public override int MaxBallID => 26;
        public override int MaxGameID => 51;
        public override PKM Clone() => new PK8((byte[])Data.Clone());
        public override int GetMarking(int index)
        {
            if ((uint)index >= MarkingCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return (MarkValue >> (index * 2)) & 3;
        }
        public override int MarkingCount => 6;

        #endregion



    }
}
