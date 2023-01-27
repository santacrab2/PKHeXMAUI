using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pkNX.Structures.FlatBuffers
{
    public partial class PokeDataBattle
    {
        public void SerializePKHeX(BinaryWriter bw, sbyte captureLv, RaidSerializationFormat format)
        {
            if (format != RaidSerializationFormat.Type3)
                AssertRegularFormat();

            // If any PointUp for a move is nonzero, throw an exception.
            if (Waza1.PointUp != 0 || Waza2.PointUp != 0 || Waza3.PointUp != 0 || Waza4.PointUp != 0)
                throw new ArgumentOutOfRangeException(nameof(WazaSet.PointUp), $"No {nameof(WazaSet.PointUp)} allowed!");

            // flag BallId if not none
            if (BallId != BallType.NONE)
                throw new ArgumentOutOfRangeException(nameof(BallId), BallId, $"No {nameof(BallId)} allowed!");

            bw.Write(SpeciesConverterSV.GetNational9((ushort)DevId));
            bw.Write((byte)FormId);
            bw.Write((byte)Sex);

            bw.Write((byte)Tokusei);
            bw.Write((byte)TalentvNum);
            bw.Write((byte)Raretype);
            bw.Write((byte)captureLv);

            // Write moves
            bw.Write((ushort)Waza1.WazaId);
            bw.Write((ushort)Waza2.WazaId);
            bw.Write((ushort)Waza3.WazaId);
            bw.Write((ushort)Waza4.WazaId);

            // ROM raids with 5 stars have a few entries that are defined as DEFAULT
            // If the type is not {specified}, the game will assume it is RANDOM.
            // Thus, DEFAULT behaves like RANDOM.
            // Let's clean up this mistake and make it explicit so we don't have to program this workaround in other tools.
            var gem = Gemtype is GemType.DEFAULT ? GemType.RANDOM : Gemtype;
            bw.Write((byte)gem);
        }

        private void AssertRegularFormat()
        {
            if (Talenttype != TalentType.V_NUM)
                throw new ArgumentOutOfRangeException(nameof(TalentType), Talenttype, "No min flawless IVs?");
            if (TalentvNum == 0 && DevId != DevID.DEV_PATIRISU &&
                Level != 35) // nice mistake gamefreak -- 3star Pachirisu is 0 IVs.
                throw new ArgumentOutOfRangeException(nameof(TalentvNum), TalentvNum, "No min flawless IVs?");

            if (Seikaku != SeikakuType.DEFAULT)
                throw new ArgumentOutOfRangeException(nameof(Seikaku), Seikaku, $"No {nameof(Seikaku)} allowed!");
        }
    }
}

