# Handbrake Encoder Service

### Background

If you rip DVDs or Blu-rays, you probably know that the files are pretty big. Sometimes over 30GB for a single file. So you should probably be using some kind of encoder that encodes the video to another format while also making the file smaller (also decreasing quality but that is a different story). A great tool for encoding is called [Handbrake](https://handbrake.fr/).

But ripping DVDs and Blu-rays can be can be tidious if you are doing it by hand. First you have to rip it. Wait for that to finish. Then you have to encode it, which can take hours. Wouldn't it be nice if there was some way to automate it?

Handbrake Encoder Service to the rescue! It is a Windows service that lives on your machine that is looking at a couple of directories for Movies and TV Shows to encode. It finds the files and sends them to Handbrake where they get encoded. After encoding, they will be sent off to the destination directory.

I am hoping to make it so that the encoder can tell when there is a TV show so that it can name the TV show in a [Plex](https://www.plex.tv/) friendly manner.

### To use

You will need the following:
* Admin privileges
* Visual Studio 2019
* A Windows computer. I have only tested on Windows 10.
* Handbrake installed

You will need to change the movie/tv shows input/output directores in the App.config to be the places you want them to be. By default they are
* Movie input -> C:\video\movie
* Movie output -> C:\video\movie_output
* TV Show input -> C:\video\tv_shows
* TV Show output -> C:\video\tv_shows_output

Simply compile the solution and then run the `install.bat` file as admin. This is simply using the Windows `installUtil` to install the service. 

Once completed, start putting movies and tv shows into the directories for encoding and the service will take care of the rest.
