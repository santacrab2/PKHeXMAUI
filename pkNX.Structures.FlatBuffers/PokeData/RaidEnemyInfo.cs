using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace pkNX.Structures.FlatBuffers
{
    public partial class RaidEnemyInfo
    {
        public void SerializePKHeX(BinaryWriter bw, byte star, sbyte rate, RaidSerializationFormat format)
        {
            BossPokePara.SerializePKHeX(bw, CaptureLv, format);
         
            bw.Write(DeliveryGroupID);

            // Append RNG details.
            bw.Write(star);
            bw.Write(rate);
        }

        public void SerializeType3(BinaryWriter bw)
        {
            // Fixed Nature, fixed IVs, fixed Scale
            var b = BossPokePara;
            if (b.Talenttype > TalentType.VALUE)
                throw new InvalidDataException("Invalid talent type for Type 3 serialization.");

            bw.Write(b.Seikaku == SeikakuType.DEFAULT ? (byte)25 : (byte)(b.Seikaku - 1));
            bw.Write((byte)b.TalentValue.HP);
            bw.Write((byte)b.TalentValue.ATK);
            bw.Write((byte)b.TalentValue.DEF);
            bw.Write((byte)b.TalentValue.SPE);
            bw.Write((byte)b.TalentValue.SPA);
            bw.Write((byte)b.TalentValue.SPD);
            bw.Write((byte)(b.Talenttype == 0 ? 0 : 1));
            bw.Write((byte)b.ScaleValue);
            bw.Write((byte)0);
        }

        public void SerializeTeraFinder(BinaryWriter bw)
        {
            bw.Write((uint)No);
            bw.Write(DropTableFix);
            bw.Write(DropTableRandom);
        }
    }
}
