# Incompetech Downloader
Downloads mp3 files from https://incompetech.filmmusic.io/

## Overview
This command line program will download all the songs from every page on the url provided.

## Installation and Usage
Download [.NET Core 2.2](https://dotnet.microsoft.com/download/dotnet-core/2.2) from here if you don't already have it installed - if you don't plan on developing .NET Core apps yourself, choose the Runtime over the SDK.

Open the command line in the folder containing the program, and type `dotnet IncompetechDownloader.dll [url]`, where the [url] is the link to the first page in the list of songs. This field is optional - if it is not provided, the program will download all the songs from the website.

For example, to download all the songs in the Electronica genre, type `dotnet IncompetechDownloader.dll https://incompetech.filmmusic.io/de/musikrichtungen/musikrichtung/elektro/`
  
### Duplicate songs bug
The Incompetech wbesite currently has a bug. If you are logged out and the language is set to anything other than Deutsch, some songs will be replaced with duplicate songs. For this reason, *always set the language to Deutsch when providing the program a URL.*

### Handling filename conflicts
On some songs (such as EDM Detection Mode) there are two songs in the list with the same name, because one of them is a remake. This causes a filename conflict between the two songs. If the program encounters a filename conflict, the second one has the file extension `.mp3.CONFLICT`.

To resolve this, search for the song on the website. The version that appears further down the list is the one that has the `.mp3.CONFLICT` extension instead of the `.mp3` extension.

Rename one (or both) of the files, and then remove the `.CONFLICT` to change the file back into a normal playable mp3 file.
