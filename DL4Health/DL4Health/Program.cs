using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DL4Health
{
    class Program
    {
        public enum BIO
        {
            B,
            I,
            O
        }

        /// <summary>
        /// Returns a string with the BIO tag depending on the specified position
        /// </summary>
        /// <param name="tag">The tag, class, not really sure how we name this distinct "clinical ner" values</param>
        /// <param name="bio">The BIO type enum</param>
        /// <returns></returns>
        public static string ToBioTag(string tag, BIO bio)
        {
            if (bio == BIO.O)
                return bio.ToString();

            return $"{tag}-{bio}";
        }

        static void Main(string[] args)
        {
            //var format = $"[{text}, [({chunck}, {class})]]"
            //TokenizeText("");
            //GetDistinctTags();
            //TagBIOTexts();
            //CombineTaggedTexts(new[] { "dev" });
            CombineTaggedTexts(new[] { "dev" }, false);
            //CombineTaggedTexts(new[] { "test" });

            //foreach (var file in Directory.GetFiles(@"D:\Projects\598 DL4Health\Project\pharmaconer\train-set_1.1\train\subtrack1", "*.txt")) {
            //    File.Copy(file, @$"D:\Projects\598 DL4Health\Project\pharmaconer\bio\corpus\train\{Path.GetFileName(file)}.txt");
            //}
        }

        /// <summary>
        /// Loads a pharmaconer .ann file into an Annotation object list
        /// </summary>
        /// <param name="filePath">Path on disk of the file</param>
        /// <returns></returns>
        private static List<Annotation> GetAnnotations(string filePath)
        {
            var ann = new List<Annotation>();
            var annotations = File.ReadAllLines(filePath);
            if (annotations.Any())
            {
                annotations = annotations.Where(x => !x.StartsWith("#")).Select(x => x).ToArray();

                return annotations
                    .Select(x => x.Split("\t"))
                    .Select(x => new { a = x[1].Split(), b = x[x.Length - 1] })
                    .Select(x =>
                    new Annotation()
                    {
                        Type = x.a[0],
                        Start = int.TryParse(x.a[1], out int _start) ? _start : 0,
                        End = int.TryParse(x.a[2], out int _end) ? _end : 0,
                        Text = x.b
                    }).ToList();
            }

            return ann;
        }

        /// <summary>
        /// Given a collection of words and a list of annotations corresponding to the word collection 
        /// it tries to match them to them
        /// </summary>
        /// <param name="words">A document text in BIO format ("tokenized"), which is a collection of words at this point</param>
        /// <param name="annotation">A list of annotation objects that correspond to the text</param>
        /// <returns>A collection of words with their BIO tag next to them separated by a tab</returns>
        private static IEnumerable<string> GetTag(string[] words, List<Annotation> annotation)
        {
            var prevWordType = BIO.O;
            int prevCount = 0;
            foreach (var word in words)
            {
                var matching = annotation.Where(x => x.Text.Split().Any(x => x == word)).FirstOrDefault();
                if (matching != null)
                {
                    if (prevWordType == BIO.B || prevWordType == BIO.I)
                    {
                        if (prevCount > 0)
                        {
                            prevWordType = BIO.I;
                            prevCount--;
                        }
                        else
                        {
                            prevWordType = BIO.B;
                            prevCount = matching.Text.Split().Length - 1;
                        }

                        yield return $"{word}\t{ToBioTag(matching.Type, prevWordType)}";
                    }
                    else if (prevWordType == BIO.O)
                    {
                        prevWordType = BIO.B;
                        prevCount = matching.Text.Split().Length - 1;
                        yield return $"{word}\t{ToBioTag(matching.Type, prevWordType)}";
                    }
                }
                else
                {
                    prevWordType = BIO.O;
                    prevCount = 0;
                    yield return $"{word}\t{prevWordType}";
                }
            }
        }

        /// <summary>
        /// Combines the separate BIO files into a single one
        /// </summary>
        /// <param name="types">Type dev, train or test that matches the pharmaconer folder structure</param>
        private static void CombineTaggedTexts(string[] types, bool lineByLine = true)
        {
            foreach(var t in types)
            {
                if (lineByLine)
                {
                    var bigList = new List<string>();
                    foreach (var f in Directory.GetFiles($@"D:\Projects\598 DL4Health\Project\pharmaconer\bio\{t}-t"))
                    {
                        var allLines = File.ReadAllLines(f);
                        bigList.AddRange(allLines);
                    }

                    File.WriteAllLines($@"D:\Projects\598 DL4Health\Project\pharmaconer\bio\{types.First()}.txt", bigList);
                }
                else
                {
                    var sw = new StringBuilder();
                    foreach (var f in Directory.GetFiles($@"D:\Projects\598 DL4Health\Project\pharmaconer\{t}-set_1.1\{t}\subtrack1", "*.txt"))
                    //foreach(var f in Directory.GetFiles($@"D:\Projects\598 DL4Health\Project\pharmaconer\bio\{t}-t"))
                    {
                        var allLines = File.ReadAllText(f);
                        sw.Append(allLines + "\r\n\r\n");
                    }


                    File.WriteAllText($@"D:\Projects\598 DL4Health\Project\pharmaconer\bio\text-corpus\{types.First()}.txt", sw.ToString());
                }

            }
        }

        /// <summary>
        /// This is used to collect the text files that need to get their BIO tags added
        /// </summary>
        private static void TagBIOTexts()
        {
            string type = "train";

            Directory.CreateDirectory($@"D:\Projects\598 DL4Health\Project\pharmaconer\bio\{type}-t");

            foreach(var filePath in Directory.GetFiles($@"D:\Projects\598 DL4Health\Project\pharmaconer\bio\{type}", "*.txt"))
            {
                var name = Path.GetFileNameWithoutExtension(filePath);
                var annotations = GetAnnotations($@"D:\Projects\598 DL4Health\Project\pharmaconer\{type}-set_1.1\{type}\subtrack1\{name}.ann");
                File.WriteAllLines($@"D:\Projects\598 DL4Health\Project\pharmaconer\bio\{type}-t\{name}.txt", GetTag(File.ReadAllLines(filePath), annotations));
            }
        }

        
    }

    /// <summary>
    /// Class used to hold Annotation objects in memory
    /// </summary>
    class Annotation
    {
        public string Type { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public string Text { get; set; }
    }
}
