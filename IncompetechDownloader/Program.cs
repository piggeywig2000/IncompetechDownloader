using System;
using System.IO;
using System.Net;

using AngleSharp;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;

namespace IncompetechDownloader
{
    class Program
    {
        static readonly bool OverwritePages = true; //For debug purposes. Default value: true. Might become a feature, but probably wont
        static readonly bool OverwriteSongs = true; //For debug purposes. Default value: true. Might become a feature, but probably wont

        static IHtmlParser Parser;

        static int DownloadAndFindNext(string url, int page)
        {
            Console.WriteLine("Downloading page " + page);
            const string NextPageSelector = "div.row > div > div.card > div.card-body > div.paginator-top > ul > li.page-item > a[rel=\"next\"]";

            //Download source
            string path = Path.GetFullPath(@"work\page" + page + ".html");
            if (!File.Exists(path) || OverwritePages)
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(url, path);
                }
            }

            string source = File.ReadAllText(path);

            AngleSharp.Html.Dom.IHtmlDocument document = Parser.ParseDocument(source);

            IHtmlCollection<IElement> pages = document.QuerySelectorAll(NextPageSelector);
            if (pages.Length != 0)
            {
                AngleSharp.Html.Dom.IHtmlAnchorElement thisPage = (AngleSharp.Html.Dom.IHtmlAnchorElement)pages[0];
                return DownloadAndFindNext(thisPage.Href, page + 1);
            }
            else
            {
                return page;
            }
        }

        static void Main(string[] args)
        {
            const string AudioPlayerSelector = "div.row > div > div.card > div.card-body > div.audioplayer-wrapper > div.audioplayer";
            const string DownloadDirectory = @"songs";

            //Get download url
            string baseUrl = "";
            if (args.Length > 0) { baseUrl = args[0]; }
            else { baseUrl = "https://incompetech.filmmusic.io/de/suche/"; }

            //If we're overwriting, delete work folder if it exists
            if (OverwritePages)
            {
                if (Directory.Exists(Path.GetFullPath("work")))
                {
                    Directory.Delete(Path.GetFullPath("work"), true);
                }
            }
            //Create work directory if it doesn't exist
            if (!Directory.Exists("work"))
            {
                Directory.CreateDirectory("work");
            }

            //Use the default configuration for AngleSharp
            IConfiguration config = Configuration.Default;

            //Create a new context for evaluating webpages with the given config
            IBrowsingContext context = BrowsingContext.New(config);

            IHtmlParser thisParser = context.GetService<IHtmlParser>();
            Parser = thisParser;

            Console.WriteLine("Downloading pages...\n");

            int totalPages = DownloadAndFindNext(baseUrl, 1);

            Console.WriteLine("Download complete! Found " + totalPages + " pages\n");
            Console.WriteLine("Downloading songs...");

            //If we're overwriting, delete song folder if it exists
            if (OverwriteSongs)
            {
                if (Directory.Exists(Path.GetFullPath(DownloadDirectory)))
                {
                    Directory.Delete(Path.GetFullPath(DownloadDirectory), true);
                }
            }
            //Create song folder if it doesn't exist
            if (!Directory.Exists(Path.GetFullPath(DownloadDirectory)))
            {
                Directory.CreateDirectory(Path.GetFullPath(DownloadDirectory));
            }

            //Ok, now parse the downloaded pages
            for (int thisPage = 1; thisPage <= totalPages; thisPage++)
            {
                Console.WriteLine("\nPage " + thisPage + " of " + totalPages + "\n");

                string path = Path.GetFullPath(@"work\page" + thisPage + ".html");
                string source = File.ReadAllText(path);

                AngleSharp.Html.Dom.IHtmlDocument document = Parser.ParseDocument(source);

                IHtmlCollection<IElement> pages = document.QuerySelectorAll(AudioPlayerSelector);

                foreach (IElement pageElement in pages)
                {
                    AngleSharp.Html.Dom.IHtmlDivElement song = (AngleSharp.Html.Dom.IHtmlDivElement)pageElement;
                    //Get path to save to
                    string savePath = Path.GetFullPath(DownloadDirectory + "\\" + song.GetAttribute("data-title") + ".mp3");

                    //If we're overwriting songs, the song shouldn't already exist. If it does, there's a problem.
                    if (OverwriteSongs)
                    {
                        while (File.Exists(savePath))
                        {
                            savePath = savePath + ".CONFLICT";
                        }
                    }

                    //If we're not overwriting songs, it will only download if it doesn't exist
                    if (!File.Exists(savePath) || OverwriteSongs)
                    {
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFile(song.GetAttribute("data-mp3").Replace("mp3low", "mp3"), savePath);
                        }
                    }
                    
                    Console.WriteLine(song.GetAttribute("data-title"));
                }
            }


            Console.WriteLine("\n\nEnd of program, bruh");
            Console.ReadKey();
        }
    }
}
