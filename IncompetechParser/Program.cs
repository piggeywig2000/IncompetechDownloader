﻿using System;
using System.IO;
using System.Net;

using AngleSharp;
using AngleSharp.Html.Parser;
using AngleSharp.Dom;

namespace IncompetechParser
{
    class Program
    {
        static readonly bool OverwritePages = false;
        static readonly bool OverwriteSongs = false;

        static IHtmlParser Parser;

        static int DownloadAndFindNext(string url, int page)
        {
            Console.WriteLine("Downloading page " + page);
            const string NextPageSelector = "#searchresults > div.row > div.col-md-10 > div > div > div.paginator-top > ul > li.page-item > a[rel=\"next\"]";

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
            const string AudioPlayerSelector = "#searchresults > div.row > div.col-md-10 > div.card > div.card-body > div.audioplayer-wrapper > div.audioplayer";
            const string downloadDirectory = @"N:\Downloads";

            //Use the default configuration for AngleSharp
            IConfiguration config = Configuration.Default;

            //Create a new context for evaluating webpages with the given config
            IBrowsingContext context = BrowsingContext.New(config);

            IHtmlParser thisParser = context.GetService<IHtmlParser>();
            Parser = thisParser;

            Console.WriteLine("Downloading pages...\n");

            int totalPages = DownloadAndFindNext("https://incompetech.filmmusic.io/search/", 1);

            Console.WriteLine("Download complete! Found " + totalPages + " pages\n");
            Console.WriteLine("Downloading songs...");

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
                    string savePath = Path.GetFullPath(downloadDirectory + "\\" + song.GetAttribute("data-title") + ".mp3");

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
