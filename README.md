YATT (Yet Another Torrent Thingy) is a lightweight torrent downloader built with Avalonia and MonoTorrent.

## Features

    Download torrents (duh).
    Set and enable different speed profiles.
    Stores current session ( files being downloaded and their states)
    if the file is associated to the app: open file to load it / start up application on file open
    Magnetic link support

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/atretador/YATT.git

2. Navigate to the project directory:
   ```bash
    cd YATT

3. Build the project:
   ```bash
    dotnet build

4. Run the application:
   ```bash
    dotnet run

Missing:

    save metadata along with session so we don't have to download it again for megnet links

Known bugs:

    Combo box for the profiles doesn't load the unit type properly.
