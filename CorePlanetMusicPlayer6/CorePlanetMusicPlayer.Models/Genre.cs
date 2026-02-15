using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorePlanetMusicPlayer.Models
{
    public class Genre
    {
        public Genre(uint id)
        {
            this.Id = id;
            if(GenreManager.GenreMap.TryGetValue(id ,out string name))
            {
                this.Name = name;
            }
            
        }

        public string Name { get; set; } = "未知流派";

        public uint Id { get; private set; }

        public List<IMusic> Music { get; set; } = new List<IMusic>();

        public string CoverPath
        {
            get
            {
                //TODO: 设置专辑封面图
                // 思路：查询所有的音乐，如果有此专辑中有本地音频文件，就从本地读取封面；如果全是StreamMusic默认为空，可以单独设置冯敏URL地址
                return GetCoverPath();
            }
        }

        private string GetCoverPath()
        {
            foreach (IMusic music in Music)
            {
                if (music is LocalMusic)
                {
                    return ((LocalMusic)music).Path;
                }
            }
            return "";
        }
    }

    public class GenreManager
    {
        public static ObservableCollection<Genre> Genres { get; set; } = new ObservableCollection<Genre>();

        public static readonly Dictionary<uint, string> GenreMap = new Dictionary<uint, string>
        {
            { 0, "Blues" },
            { 1, "Classic Rock" },
            { 2, "Country" },
            { 3, "Dance" },
            { 4, "Disco" },
            { 5, "Funk" },
            { 6, "Grunge" },
            { 7, "Hip-Hop" },
            { 8, "Jazz" },
            { 9, "Metal" },
            { 10, "New Age" },
            { 11, "Oldies" },
            { 12, "Other" },
            { 13, "Pop" },
            { 14, "R&B" },
            { 15, "Rap" },
            { 16, "Reggae" },
            { 17, "Rock" },
            { 18, "Techno" },
            { 19, "Industrial" },
            { 20, "Alternative" },
            { 21, "Ska" },
            { 22, "Death Metal" },
            { 23, "Pranks" },
            { 24, "Soundtrack" },
            { 25, "Euro-Techno" },
            { 26, "Ambient" },
            { 27, "Trip-Hop" },
            { 28, "Vocal" },
            { 29, "Jazz+Funk" },
            { 30, "Fusion" },
            { 31, "Trance" },
            { 32, "Classical" },
            { 33, "Instrumental" },
            { 34, "Acid" },
            { 35, "House" },
            { 36, "Game" },
            { 37, "Sound Clip" },
            { 38, "Gospel" },
            { 39, "Noise" },
            { 40, "AlternRock" },
            { 41, "Bass" },
            { 42, "Soul" },
            { 43, "Punk" },
            { 44, "Space" },
            { 45, "Meditative" },
            { 46, "Instrumental Pop" },
            { 47, "Instrumental Rock" },
            { 48, "Ethnic" },
            { 49, "Gothic" },
            { 50, "Darkwave" },
            { 51, "Techno-Industrial" },
            { 52, "Electronic" },
            { 53, "Pop-Folk" },
            { 54, "Eurodance" },
            { 55, "Dream" },
            { 56, "Southern Rock" },
            { 57, "Comedy" },
            { 58, "Cult" },
            { 59, "Gangsta Rap" },
            { 60, "Top 40" },
            { 61, "Christian Rap" },
            { 62, "Pop / Funk" },
            { 63, "Jungle" },
            { 64, "Native American" },
            { 65, "Cabaret" },
            { 66, "New Wave" },
            { 67, "Psychedelic" },
            { 68, "Rave" },
            { 69, "Showtunes" },
            { 70, "Trailer" },
            { 71, "Lo-Fi" },
            { 72, "Tribal" },
            { 73, "Acid Punk" },
            { 74, "Acid Jazz" },
            { 75, "Polka" },
            { 76, "Retro" },
            { 77, "Musical" },
            { 78, "Rock & Roll" },
            { 79, "Hard Rock" },
            { 80, "Folk" },
            { 81, "Folk-Rock" },
            { 82, "National Folk" },
            { 83, "Swing" },
            { 84, "Fast Fusion" },
            { 85, "Bebob" },
            { 86, "Latin" },
            { 87, "Revival" },
            { 88, "Celtic" },
            { 89, "Bluegrass" },
            { 90, "Avantgarde" },
            { 91, "Gothic Rock" },
            { 92, "Progressive Rock" },
            { 93, "Psychedelic Rock" },
            { 94, "Symphonic Rock" },
            { 95, "Slow Rock" },
            { 96, "Big Band" },
            { 97, "Chorus" },
            { 98, "Easy Listening" },
            { 99, "Acoustic" },
            { 100, "Humour" },
            { 101, "Speech" },
            { 102, "Chanson" },
            { 103, "Opera" },
            { 104, "Chamber Music" },
            { 105, "Sonata" },
            { 106, "Symphony" },
            { 107, "Booty Bass" },
            { 108, "Primus" },
            { 109, "Porn Groove" },
            { 110, "Satire" },
            { 111, "Slow Jam" },
            { 112, "Club" },
            { 113, "Tango" },
            { 114, "Samba" },
            { 115, "Folklore" },
            { 116, "Ballad" },
            { 117, "Power Ballad" },
            { 118, "Rhythmic Soul" },
            { 119, "Freestyle" },
            { 120, "Duet" },
            { 121, "Punk Rock" },
            { 122, "Drum Solo" },
            { 123, "A Cappella" },
            { 124, "Euro-House" },
            { 125, "Dance Hall" },
            { 126, "Goa" },
            { 127, "Drum & Bass" },
            { 128, "Club-House" },
            { 129, "Hardcore" },
            { 130, "Terror" },
            { 131, "Indie" },
            { 132, "BritPop" },
            { 133, "Negerpunk" },
            { 134, "Polsk Punk" },
            { 135, "Beat" },
            { 136, "Christian Gangsta Rap" },
            { 137, "Heavy Metal" },
            { 138, "Black Metal" },
            { 139, "Crossover" },
            { 140, "Contemporary Christian" },
            { 141, "Christian Rock" },
            { 142, "Merengue" },
            { 143, "Salsa" },
            { 144, "Thrash Metal" },
            { 145, "Anime" },
            { 146, "JPop" },
            { 147, "Synthpop" },
            { 148, "Abstract" },
            { 149, "Art Rock" },
            { 150, "Baroque" },
            { 151, "Bhangra" },
            { 152, "Big Beat" },
            { 153, "Breakbeat" },
            { 154, "Chillout" },
            { 155, "Downtempo" },
            { 156, "Dub" },
            { 157, "EBM" },
            { 158, "Eclectic" },
            { 159, "Electro" },
            { 160, "Electroclash" },
            { 161, "Emo" },
            { 162, "Experimental" },
            { 163, "Garage" },
            { 164, "Global" },
            { 165, "IDM" },
            { 166, "Illbient" },
            { 167, "Industro-Goth" },
            { 168, "Jam Band" },
            { 169, "Krautrock" },
            { 170, "Leftfield" },
            { 171, "Lounge" },
            { 172, "Math Rock" },
            { 173, "New Romantic" },
            { 174, "Nu-Breakz" },
            { 175, "Post-Punk" },
            { 176, "Post-Rock" },
            { 177, "Psytrance" },
            { 178, "Shoegaze" },
            { 179, "Space Rock" },
            { 180, "Trop Rock" },
            { 181, "World Music" },
            { 182, "Neoclassical" },
            { 183, "Audiobook" },
            { 184, "Audio Theatre" },
            { 185, "Neue Deutsche Welle" },
            { 186, "Podcast" },
            { 187, "Indie Rock" },
            { 188, "G-Funk" },
            { 189, "Dubstep" },
            { 190, "Garage Rock" },
            { 191, "Psybient" }
        };

        public static readonly Dictionary<uint, string> GenreMap_Chinese = new Dictionary<uint, string>
        {
            { 0, "布鲁斯" },
            { 1, "经典摇滚" },
            { 2, "乡村音乐" },
            { 3, "舞曲" },
            { 4, "迪斯科" },
            { 5, "放克" },
            { 6, "垃圾摇滚" },
            { 7, "嘻哈" },
            { 8, "爵士" },
            { 9, "金属乐" },
            { 10, "新世纪音乐" },
            { 11, "老歌" },
            { 12, "其他" },
            { 13, "流行乐" },
            { 14, "节奏布鲁斯" },
            { 15, "说唱" },
            { 16, "雷鬼" },
            { 17, "摇滚" },
            { 18, "科技舞曲" },
            { 19, "工业音乐" },
            { 20, "另类音乐" },
            { 21, "斯卡音乐" },
            { 22, "死亡金属" },
            { 23, "恶作剧音乐" },
            { 24, "原声配乐" },
            { 25, "欧洲科技舞曲" },
            { 26, "氛围音乐" },
            { 27, "Trip-Hop" },
            { 28, "人声" },
            { 29, "爵士放克" },
            { 30, "融合爵士" },
            { 31, "迷幻舞曲" },
            { 32, "古典音乐" },
            { 33, "器乐" },
            { 34, "酸性音乐" },
            { 35, "浩室音乐" },
            { 36, "游戏音乐" },
            { 37, "声音片段" },
            { 38, "福音音乐" },
            { 39, "噪音音乐" },
            { 40, "另类摇滚" },
            { 41, "贝斯音乐" },
            { 42, "灵魂乐" },
            { 43, "朋克" },
            { 44, "太空音乐" },
            { 45, "冥想音乐" },
            { 46, "流行器乐" },
            { 47, "摇滚器乐" },
            { 48, "民族音乐" },
            { 49, "哥特音乐" },
            { 50, "暗潮" },
            { 51, "工业科技舞曲" },
            { 52, "电子音乐" },
            { 53, "流行民谣" },
            { 54, "欧洲舞曲" },
            { 55, "梦幻音乐" },
            { 56, "南方摇滚" },
            { 57, "喜剧音乐" },
            { 58, "小众音乐" },
            { 59, "匪帮说唱" },
            { 60, "前40热门金曲" },
            { 61, "基督教说唱" },
            { 62, "流行放克" },
            { 63, "丛林音乐" },
            { 64, "美国原住民音乐" },
            { 65, "卡巴莱" },
            { 66, "新浪潮" },
            { 67, "迷幻音乐" },
            { 68, "锐舞音乐" },
            { 69, "音乐剧选段" },
            { 70, "预告片配乐" },
            { 71, "低保真" },
            { 72, "部落音乐" },
            { 73, "酸性朋克" },
            { 74, "酸性爵士" },
            { 75, "波尔卡" },
            { 76, "复古音乐" },
            { 77, "音乐剧" },
            { 78, "摇滚乐" },
            { 79, "硬摇滚" },
            { 80, "民谣" },
            { 81, "民谣摇滚" },
            { 82, "民族民谣" },
            { 83, "摇摆乐" },
            { 84, "快速融合乐" },
            { 85, "比波普爵士" },
            { 86, "拉丁音乐" },
            { 87, "复兴音乐" },
            { 88, "凯尔特音乐" },
            { 89, "蓝草音乐" },
            { 90, "先锋音乐" },
            { 91, "哥特摇滚" },
            { 92, "前卫摇滚" },
            { 93, "迷幻摇滚" },
            { 94, "交响摇滚" },
            { 95, "慢摇滚" },
            { 96, "大乐队" },
            { 97, "合唱音乐" },
            { 98, "轻音乐" },
            { 99, "原声音乐" },
            { 100, "幽默音乐" },
            { 101, "演讲录音" },
            { 102, "香颂" },
            { 103, "歌剧" },
            { 104, "室内乐" },
            { 105, "奏鸣曲" },
            { 106, "交响乐" },
            { 107, "电臀贝斯" },
            { 108, "普里默斯（乐队风格）" },
            { 109, "色情律动" },
            { 110, "讽刺音乐" },
            { 111, "慢节奏音乐" },
            { 112, "俱乐部音乐" },
            { 113, "探戈" },
            { 114, "桑巴" },
            { 115, "民间传说音乐" },
            { 116, "民谣" },
            { 117, "强力民谣" },
            { 118, "节奏灵魂乐" },
            { 119, "自由风格" },
            { 120, "二重唱" },
            { 121, "朋克摇滚" },
            { 122, "鼓独奏" },
            { 123, "无伴奏合唱" },
            { 124, "欧洲浩室" },
            { 125, "舞厅音乐" },
            { 126, "果阿 trance" },
            { 127, "鼓打贝斯" },
            { 128, "俱乐部浩室" },
            { 129, "硬核音乐" },
            { 130, "恐怖音乐" },
            { 131, "独立音乐" },
            { 132, "英伦流行" },
            { 133, "黑人朋克" },
            { 134, "波兰朋克" },
            { 135, "节拍音乐" },
            { 136, "基督教匪帮说唱" },
            { 137, "重金属" },
            { 138, "黑金属" },
            { 139, "跨界音乐" },
            { 140, "当代基督教音乐" },
            { 141, "基督教摇滚" },
            { 142, "梅伦格舞" },
            { 143, "萨尔萨舞" },
            { 144, "鞭挞金属" },
            { 145, "动漫音乐" },
            { 146, "日本流行" },
            { 147, "合成器流行" },
            { 148, "抽象音乐" },
            { 149, "艺术摇滚" },
            { 150, "巴洛克音乐" },
            { 151, "旁遮普音乐" },
            { 152, "大节拍" },
            { 153, "碎拍" },
            { 154, "弛放音乐" },
            { 155, "慢节奏电子" },
            { 156, "配音音乐" },
            { 157, "电子身体音乐" },
            { 158, "折衷主义音乐" },
            { 159, "电子乐" },
            { 160, "电子碰撞" },
            { 161, "情绪硬核" },
            { 162, "实验音乐" },
            { 163, "车库音乐" },
            { 164, "全球音乐" },
            { 165, "智能舞曲" },
            { 166, "氛围实验" },
            { 167, "工业哥特" },
            { 168, "即兴乐队" },
            { 169, "德国摇滚" },
            { 170, "非主流音乐" },
            { 171, " Lounge音乐" },
            { 172, "数学摇滚" },
            { 173, "新浪漫主义" },
            { 174, "新碎拍" },
            { 175, "后朋克" },
            { 176, "后摇滚" },
            { 177, "迷幻 trance" },
            { 178, "自赏派" },
            { 179, "太空摇滚" },
            { 180, "热带摇滚" },
            { 181, "世界音乐" },
            { 182, "新古典主义" },
            { 183, "有声书" },
            { 184, "广播剧" },
            { 185, "德国新浪潮" },
            { 186, "播客" },
            { 187, "独立摇滚" },
            { 188, "G放克" },
            { 189, "回响贝斯" },
            { 190, "车库摇滚" },
            { 191, "迷幻氛围" }
        };

        public static void RefreshGenresList(List<IMusic> musicList)
        {
            Genres.Clear();
            AddMusicToGenre(musicList);
        }

        public static void AddMusicToGenre(List<IMusic> musicList)
        {
            // 1. 构建现有年份的字典
            var genreDict = Genres.ToDictionary(y => y.Id);

            // 2. 批量处理
            foreach (IMusic music in musicList)
            {
                Genre genre;
                if (!genreDict.TryGetValue(music.Genre, out genre))
                {
                    genre = new Genre(music.Genre);
                    Genres.Add(genre);
                    genreDict[music.Genre] = genre;
                }
                genre.Music.Add(music);
            }
        }

        public static void AddMusicToGenre(IMusic music)
        {
            var genreList = Genres.Where(x => x.Id == music.Genre).ToList();
            Genre genre = null;
            if (genreList != null && genreList.Count != 0)
            {
                genre.Music.Add(music);
            }
            else
            {
                genre = new Genre(music.Genre);
                genre.Music.Add(music);
                Genres.Add(genre);
            }
        }

        public static void RemoveMusicFromGenre(IMusic music)
        {
            List<Genre> genres = Genres.ToList();
            Genre genre = genres.Find(x => x.Id == music.Genre);
            if (genre == null)
                return;
            bool isSucceed = genre.Music.Remove(music);
            Debug.WriteLine($"从流派中移除音乐：{music.Title} 是否成功？ {isSucceed}");
            if (genre.Music.Count == 0)
                Genres.Remove(genre);
        }

        public static void RemoveMusicFromGenre(List<IMusic>musicList)
        {
            List<Genre> genres = Genres.ToList();
            foreach (IMusic music in musicList)
            {
                Genre genre = genres.Find(x => x.Id == music.Genre);
                if (genre == null)
                    continue;
                bool isSucceed = genre.Music.Remove(music);
                Debug.WriteLine($"从流派中移除音乐：{music.Title} 是否成功？ {isSucceed}");
                if (genre.Music.Count == 0)
                    Genres.Remove(genre);
            }
        }
    }
}
