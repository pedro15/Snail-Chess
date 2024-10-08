using System.Runtime.CompilerServices;

namespace SnailChess.Core.MoveGen.Magics
{
    public static class MagicsUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMagicIndex(in ulong _occ, byte _shifts , in MagicEntry _entry)
        {
            return (int)(_entry.offset + (((_occ & _entry.mask) * _entry.magic) >> _shifts));
        }
        
        public static string PrintEntries(in MagicEntry[] _entries)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("{\n");
            for (byte sq = 0; sq < _entries.Length; sq++)
            {
                builder.AppendFormat("new MagicEntry({0}, {1}, {2}),",  $"0x{_entries[sq].mask:X16}UL", $"0x{_entries[sq].magic:X16}UL", $"0x{_entries[sq].offset:X6}");
                builder.AppendLine();
            }
            builder.Append("};\n");
            return builder.ToString();   
        }
        
        public static readonly MagicEntry[] MAGICS_ROOK = 
        {
            new MagicEntry(0x000101010101017EUL, 0x2080004000201880UL, 0x008000),
            new MagicEntry(0x000202020202027CUL, 0x0240044020001001UL, 0x009000),
            new MagicEntry(0x000404040404047AUL, 0x0820000800100004UL, 0x00A000),
            new MagicEntry(0x0008080808080876UL, 0x2071080020100040UL, 0x00B000),
            new MagicEntry(0x001010101010106EUL, 0x0140814004000100UL, 0x00C000),
            new MagicEntry(0x002020202020205EUL, 0x0800410102900AA0UL, 0x00D000),
            new MagicEntry(0x004040404040403EUL, 0x0600110400485200UL, 0x00E000),
            new MagicEntry(0x008080808080807EUL, 0x0200042508804402UL, 0x00F000),
            new MagicEntry(0x0001010101017E00UL, 0x8820020820041000UL, 0x010000),
            new MagicEntry(0x0002020202027C00UL, 0x0820181010010088UL, 0x011000),
            new MagicEntry(0x0004040404047A00UL, 0x0808901420201140UL, 0x012000),
            new MagicEntry(0x0008080808087600UL, 0x0000280002809200UL, 0x013000),
            new MagicEntry(0x0010101010106E00UL, 0x0120050004A20800UL, 0x014000),
            new MagicEntry(0x0020202020205E00UL, 0x8000460814000100UL, 0x015000),
            new MagicEntry(0x0040404040403E00UL, 0x0602400800804100UL, 0x016000),
            new MagicEntry(0x0080808080807E00UL, 0x029020108C004120UL, 0x017000),
            new MagicEntry(0x00010101017E0100UL, 0x0004040200400010UL, 0x018000),
            new MagicEntry(0x00020202027C0200UL, 0x1000A02204001012UL, 0x019000),
            new MagicEntry(0x00040404047A0400UL, 0x2038200600200101UL, 0x01A000),
            new MagicEntry(0x0008080808760800UL, 0x480040042180C800UL, 0x01B000),
            new MagicEntry(0x00101010106E1000UL, 0xA388150002040100UL, 0x01C000),
            new MagicEntry(0x00202020205E2000UL, 0x0300008002200414UL, 0x01D000),
            new MagicEntry(0x00404040403E4000UL, 0x6102004100008040UL, 0x01E000),
            new MagicEntry(0x00808080807E8000UL, 0x0200009002032040UL, 0x01F000),
            new MagicEntry(0x000101017E010100UL, 0x0080201080004000UL, 0x020000),
            new MagicEntry(0x000202027C020200UL, 0x0000400180200084UL, 0x021000),
            new MagicEntry(0x000404047A040400UL, 0x08C0064100132002UL, 0x022000),
            new MagicEntry(0x0008080876080800UL, 0x0200A94200800800UL, 0x023000),
            new MagicEntry(0x001010106E101000UL, 0x9000011100810008UL, 0x024000),
            new MagicEntry(0x002020205E202000UL, 0x0200080210410880UL, 0x025000),
            new MagicEntry(0x004040403E404000UL, 0x0452119450046240UL, 0x026000),
            new MagicEntry(0x008080807E808000UL, 0x00284C8010022100UL, 0x027000),
            new MagicEntry(0x0001017E01010100UL, 0x0808041020022000UL, 0x028000),
            new MagicEntry(0x0002027C02020200UL, 0x3910024803B00011UL, 0x029000),
            new MagicEntry(0x0004047A04040400UL, 0x0810200010050100UL, 0x02A000),
            new MagicEntry(0x0008087608080800UL, 0x4040020180128880UL, 0x02B000),
            new MagicEntry(0x0010106E10101000UL, 0x110E000132008800UL, 0x02C000),
            new MagicEntry(0x0020205E20202000UL, 0x000003000C100180UL, 0x02D000),
            new MagicEntry(0x0040403E40404000UL, 0x0018084200800890UL, 0x02E000),
            new MagicEntry(0x0080807E80808000UL, 0x1100C50020081100UL, 0x02F000),
            new MagicEntry(0x00017E0101010100UL, 0x8000104000208000UL, 0x030000),
            new MagicEntry(0x00027C0202020200UL, 0x0140001900110040UL, 0x031000),
            new MagicEntry(0x00047A0404040400UL, 0x9212902101100200UL, 0x032000),
            new MagicEntry(0x0008760808080800UL, 0x4008420844801000UL, 0x033000),
            new MagicEntry(0x00106E1010101000UL, 0x1010020900050001UL, 0x034000),
            new MagicEntry(0x00205E2020202000UL, 0x0004000200404080UL, 0x035000),
            new MagicEntry(0x00403E4040404000UL, 0x0E000100C2140008UL, 0x036000),
            new MagicEntry(0x00807E8080808000UL, 0x4800048002412018UL, 0x037000),
            new MagicEntry(0x007E010101010100UL, 0x02050910A0408200UL, 0x038000),
            new MagicEntry(0x007C020202020200UL, 0x1810002200180458UL, 0x039000),
            new MagicEntry(0x007A040404040400UL, 0x202201E420002882UL, 0x03A000),
            new MagicEntry(0x0076080808080800UL, 0x22002004001200A8UL, 0x03B000),
            new MagicEntry(0x006E101010101000UL, 0x0481228800021008UL, 0x03C000),
            new MagicEntry(0x005E202020202000UL, 0x14330004002040D8UL, 0x03D000),
            new MagicEntry(0x003E404040404000UL, 0x0100008621000040UL, 0x03E000),
            new MagicEntry(0x007E808080808000UL, 0x0041800143041020UL, 0x03F000),
            new MagicEntry(0x7E01010101010100UL, 0x0102102283004202UL, 0x040000),
            new MagicEntry(0x7C02020202020200UL, 0xA600810088400197UL, 0x041000),
            new MagicEntry(0x7A04040404040400UL, 0x0001021020000815UL, 0x042000),
            new MagicEntry(0x7608080808080800UL, 0x01200A0040102002UL, 0x043000),
            new MagicEntry(0x6E10101010101000UL, 0x000C010005080011UL, 0x044000),
            new MagicEntry(0x5E20202020202000UL, 0x1601200A04008831UL, 0x045000),
            new MagicEntry(0x3E40404040404000UL, 0x8040040042002091UL, 0x046000),
            new MagicEntry(0x7E80808080808000UL, 0x1012010084005022UL, 0x047000),
        };

        public static readonly MagicEntry[] MAGICS_BISHOP = 
        {
            new MagicEntry(0x0040201008040200UL, 0x0802100408801021UL, 0x000000),
            new MagicEntry(0x0000402010080400UL, 0x0081080010420902UL, 0x000200),
            new MagicEntry(0x0000004020100A00UL, 0x8009040020060300UL, 0x000400),
            new MagicEntry(0x0000000040221400UL, 0x00438281100010A0UL, 0x000600),
            new MagicEntry(0x0000000002442800UL, 0x0088806800022000UL, 0x000800),
            new MagicEntry(0x0000000204085000UL, 0x000018009802E061UL, 0x000A00),
            new MagicEntry(0x0000020408102000UL, 0x0802281118008120UL, 0x000C00),
            new MagicEntry(0x0002040810204000UL, 0x0084022104014000UL, 0x000E00),
            new MagicEntry(0x0020100804020000UL, 0x218100E202040118UL, 0x001000),
            new MagicEntry(0x0040201008040000UL, 0xC50011009401A102UL, 0x001200),
            new MagicEntry(0x00004020100A0000UL, 0x0204582054002001UL, 0x001400),
            new MagicEntry(0x0000004022140000UL, 0x0001005005400140UL, 0x001600),
            new MagicEntry(0x0000000244280000UL, 0x00000C25810A0680UL, 0x001800),
            new MagicEntry(0x0000020408500000UL, 0x15010018A41000C8UL, 0x001A00),
            new MagicEntry(0x0002040810200000UL, 0x0088041402420882UL, 0x001C00),
            new MagicEntry(0x0004081020400000UL, 0x0000400808001600UL, 0x001E00),
            new MagicEntry(0x0010080402000200UL, 0x3206008050610A08UL, 0x002000),
            new MagicEntry(0x0020100804000400UL, 0x00340003614020A0UL, 0x002200),
            new MagicEntry(0x004020100A000A00UL, 0x8000840810290A12UL, 0x002400),
            new MagicEntry(0x0000402214001400UL, 0x0020480200800200UL, 0x002600),
            new MagicEntry(0x0000024428002800UL, 0x0000200400200008UL, 0x002800),
            new MagicEntry(0x0002040850005000UL, 0x0020200088004082UL, 0x002A00),
            new MagicEntry(0x0004081020002000UL, 0x0400050140202110UL, 0x002C00),
            new MagicEntry(0x0008102040004000UL, 0x0840040901809022UL, 0x002E00),
            new MagicEntry(0x0008040200020400UL, 0x0024406AA0C10404UL, 0x003000),
            new MagicEntry(0x0010080400040800UL, 0x01104B0801048050UL, 0x003200),
            new MagicEntry(0x0020100A000A1000UL, 0x0400220010001040UL, 0x003400),
            new MagicEntry(0x0040221400142200UL, 0x2001004034004200UL, 0x003600),
            new MagicEntry(0x0002442800284400UL, 0x0010040000802104UL, 0x003800),
            new MagicEntry(0x0004085000500800UL, 0x0102222810220180UL, 0x003A00),
            new MagicEntry(0x0008102000201000UL, 0x00458440004C8182UL, 0x003C00),
            new MagicEntry(0x0010204000402000UL, 0x002004C0400481A0UL, 0x003E00),
            new MagicEntry(0x0004020002040800UL, 0x0008460990004404UL, 0x004000),
            new MagicEntry(0x0008040004081000UL, 0x04002029FC011800UL, 0x004200),
            new MagicEntry(0x00100A000A102000UL, 0x2042002440118040UL, 0x004400),
            new MagicEntry(0x0022140014224000UL, 0x8041010900080040UL, 0x004600),
            new MagicEntry(0x0044280028440200UL, 0x4004080200002008UL, 0x004800),
            new MagicEntry(0x0008500050080400UL, 0x4040830402081002UL, 0x004A00),
            new MagicEntry(0x0010200020100800UL, 0x2000480260440704UL, 0x004C00),
            new MagicEntry(0x0020400040201000UL, 0xB004484040000210UL, 0x004E00),
            new MagicEntry(0x0002000204081000UL, 0x0802205042000100UL, 0x005000),
            new MagicEntry(0x0004000408102000UL, 0x0008220200706042UL, 0x005200),
            new MagicEntry(0x000A000A10204000UL, 0x004C0A0040200404UL, 0x005400),
            new MagicEntry(0x0014001422400000UL, 0x0800006401002020UL, 0x005600),
            new MagicEntry(0x0028002844020000UL, 0x0408041082001820UL, 0x005800),
            new MagicEntry(0x0050005008040200UL, 0x6000848100081E00UL, 0x005A00),
            new MagicEntry(0x0020002010080400UL, 0x028B042081000401UL, 0x005C00),
            new MagicEntry(0x0040004020100800UL, 0x0008000880720600UL, 0x005E00),
            new MagicEntry(0x0000020408102000UL, 0xC001410880200001UL, 0x006000),
            new MagicEntry(0x0000040810204000UL, 0x826414080D200000UL, 0x006200),
            new MagicEntry(0x00000A1020400000UL, 0x2000800240248004UL, 0x006400),
            new MagicEntry(0x0000142240000000UL, 0x1024C14828081000UL, 0x006600),
            new MagicEntry(0x0000284402000000UL, 0x0880209030401090UL, 0x006800),
            new MagicEntry(0x0000500804020000UL, 0x0204025009A09002UL, 0x006A00),
            new MagicEntry(0x0000201008040200UL, 0x08020201AA001008UL, 0x006C00),
            new MagicEntry(0x0000402010080400UL, 0x0015480580040642UL, 0x006E00),
            new MagicEntry(0x0002040810204000UL, 0x0212010401448048UL, 0x007000),
            new MagicEntry(0x0004081020400000UL, 0x0200004030882008UL, 0x007200),
            new MagicEntry(0x000A102040000000UL, 0x0000002152008040UL, 0x007400),
            new MagicEntry(0x0014224000000000UL, 0x3400220000140080UL, 0x007600),
            new MagicEntry(0x0028440200000000UL, 0x0000000084A10010UL, 0x007800),
            new MagicEntry(0x0050080402000000UL, 0x04010C2088022028UL, 0x007A00),
            new MagicEntry(0x0020100804020000UL, 0x00040145020A0544UL, 0x007C00),
            new MagicEntry(0x0040201008040200UL, 0x3000448200820014UL, 0x007E00),
        };
    }
}