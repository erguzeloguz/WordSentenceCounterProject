using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace WordSentenceCounterProject
{
    class Program
    {
        static void Main(string[] args)
        {
            WordCounterProject counter = new WordCounterProject();
            counter.Start();
            Console.Read();

        }
    }

    class WordCounterProject
    {
        #region VariablesDef

        private char[] delimiterChars = { '.', '?', '!' };// cümlerleri ayırmak adına tanımladığım noktalama işaretleri
        private int sentenceIndex = 0; //cümle Index
        private int threadCount = 5;// Default tanımlanan thread sayısı 
        private int completedThread = 0;// Tüm threadlerin tamamlandıklarını kontrol etmek için bu parametre tanımlandı
        private List<string> wordCounts = new List<string>();// Cümle içinde yer alan tüm kelimeler bu listeye eklenir.
        private string fileName = "C:\\Calismalar\\test.txt"; // Dosya yolu tanım
        private string[] sentences; // Cümleler
        private static readonly object Lock = new object();//Aynı anda farklı threadlerin aynı cümleye atanmaması için o fonksiyon içinde lock objesi kullanıldı
        private List<ThreadItem> threadItemCount = new List<ThreadItem>();
        int avgScore = 0;

        #endregion VariablesDef 

        public void Start()
        {
            try
            {
                FilePathAndThreadCountDef();

                string text = FileRead(fileName)
                    .Replace("\r", "")
                    .Replace("\n", " ")
                    .Replace("\t", " ")
                    .Replace(",", "")
                    .Replace(";", "")
                    .Replace("“", "")
                    .Replace("”", "")
                    .Replace("...", ".")
                    .Replace("-", "");

                sentences = text.Split(delimiterChars);

                for (int i = 0; i < threadCount; i++)
                {
                    Thread thread = new Thread(ThreadProcessRun);
                    thread.Start(i);
                }

                while (threadCount != completedThread) { } //Tüm threadlerin tamamlanması beklenir

                if (sentences.Length > 0)
                {
                    avgScore = wordCounts.Count / (sentences.Length - 1); // Cümle içindeki ortalama kelime sayısı
                }

                int sentenceCount = sentences.Length; //Cümle Sayısı

                WriteResult(sentenceCount, avgScore, threadCount);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Read(); ;
            }
        }

        public void FilePathAndThreadCountDef()
        {
            Console.WriteLine("Dosya yolu giriniz : (Örnek : C:\\Calismalar\\test.txt)");
            fileName = Console.ReadLine();
            Console.WriteLine("Yardımcı thread sayısı girmek ister misiniz? (Y/N) (Default Thread Sayısı : 5)");
            if (Console.ReadLine() == "Y")
            {
                Console.WriteLine("Yardımcı thread sayısını giriniz : ");
                threadCount = Convert.ToInt32(Console.ReadLine());
            }
            Console.WriteLine("İşlemler Gerçekleştiriliyor.");

        }

        public static string FileRead(string fileName)
        {
            //Dosya okuma fonksiyonu
            string text = "";
            using (StreamReader sr = new StreamReader(fileName))
            {
                String line = sr.ReadToEnd();
                text += line;
            }
            return text;
        }

        /// <summary>
        /// Thread Process süreci çalıştırılır
        /// </summary>
        public void ThreadProcessRun(object data)
        {
            int sentenceCount = 0;
            while (sentenceIndex < sentences.Length - 1)
            {
                lock (Lock)
                {
                    WordCount(data, sentenceIndex, ref sentenceCount);
                    sentenceIndex++;
                }
            }
            threadItemCount.Add(new ThreadItem() { ThreadId = data, SentenceCount = sentenceCount });
            completedThread++;
        }

        /// <summary>
        /// Cümle boşluklara göre parse edilip ve wordCount listesine eklenir,
        /// ilgili thread için cümle sayısı 1 artırılır
        /// </summary>
        public void WordCount(object data, int sentenceIndex, ref int sentenceCount)
        {
            if (sentenceIndex < sentences.Length)
            {
                string[] words = sentences[sentenceIndex].Trim().Split(' ');
                foreach (var item in words)
                {
                    wordCounts.Add(item);
                }
                sentenceCount++;
            }
        }

        public void WriteResult(int sentenceCount, int avgScore, int threadCount)
        {
            Console.WriteLine("Sentence Count  : " + sentenceCount);

            Console.WriteLine("Avg. Word Count : " + avgScore);

            Console.WriteLine("Thread Counts : " + threadCount);

            foreach (var item in threadItemCount)
            {
                Console.WriteLine("     ThreadId= {0}, Count={1}", item.ThreadId, item.SentenceCount);
            }

            wordCounts
             .GroupBy(k => k)
             .Select(g => new
             {
                 Word = g.Key,
                 Count = g.Count()
             })
             .OrderByDescending(a => a.Count)
             .ToList()
             .ForEach(a =>
             {
                 Console.WriteLine(a.Word + " : " + a.Count);
             });


        }

        class ThreadItem
        {
            public object ThreadId { get; set; }
            public int SentenceCount { get; set; }
        }

    }
}