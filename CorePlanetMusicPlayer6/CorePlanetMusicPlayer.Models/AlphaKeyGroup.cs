using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Globalization.Collation;

namespace CorePlanetMusicPlayer.Models
{
    public class AlphaKeyGroup<T> : List<T>
    {
        /// <summary>
        /// 用来获取Key的委托
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public delegate string GetKeyDelegate(T item);

        /// <summary>
        /// 
        /// </summary>
        public string Key { get; private set; }

        const string GlobeGroupKey = "??";

        public List<T> InternalList { get; private set; }

        public AlphaKeyGroup(string key)
        {
            this.Key = key;
            InternalList = new List<T>();
        }

        private static List<AlphaKeyGroup<T>> CreateDefaultGroups(CharacterGroupings slg)
        {
            List<AlphaKeyGroup<T>> list = new List<AlphaKeyGroup<T>>();

            foreach (CharacterGrouping cg in slg)
            {
                if (cg.Label == "") continue;
                if (cg.Label == "...")
                {
                    list.Add(new AlphaKeyGroup<T>(GlobeGroupKey));
                }
                else
                {
                    list.Add(new AlphaKeyGroup<T>(cg.Label));
                }
            }

            return list;
        }

        public static List<AlphaKeyGroup<T>> CreateGroups(IEnumerable<T> items, Func<T, string> keySelector)
        {
            CharacterGroupings slg = new CharacterGroupings();
            List<AlphaKeyGroup<T>> list = CreateDefaultGroups(slg);

            foreach (T item in items)
            {
                int index = 0;
                string label = slg.Lookup(keySelector(item));
                index = list.FindIndex(alphaKeyGroup => (alphaKeyGroup.Key.Equals(label, StringComparison.CurrentCulture)));

                if (index >= 0 && index < list.Count)
                {
                    //Debug.WriteLine("group!!!");
                    list[index].InternalList.Add(item);
                }
                else
                {
                    //临时解决方案，中文加入？？中
                    list[list.Count - 1].InternalList.Add(item);
                }
            }

            //if (sort)
            //{
            //    foreach (AlphaKeyGroup<T> group in list)
            //    {
            //        group.InternalList.Sort((c0, c1) => { return keySelector(c0).CompareTo(keySelector(c1)); });
            //    }
            //}

            //foreach (AlphaKeyGroup<T> item in list)
            //{
            //    Debug.WriteLine(item.Key+"|"+item.InternalList.Count);
            //}

            return list;
        }

        
        
    }
}
