using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicRec
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"D:\Data\Datasets\Last.fm\babak_user sessions.csv";

            var reader = new PlayingSessionReader(path);
            var container = new MusicDataContainer();

            reader.LoadData(container);

            Console.WriteLine("Finished!");
            Console.Read();
        }
    }
}
