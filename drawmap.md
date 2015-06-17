# Introduction #

Drawmap is a command line tool that takes a [map xml file](mapXmlFormat.md) as an argument and draws the map to an image file.

# Usage #

```
Usage: [mono]  drawmap.exe [OPTIONS]+ "path to map file" 
Draws a map

example: mono Cumberland.DrawMap.exe -o=my.png /path/to/map 

Options:
  -e, --extents=VALUE        comma-delimited extents (e.g. -180,-90,180,90) 
  -h, --help                 show this message and exit
  -o, --output=VALUE         the path of the PNG image to create
  -w, --width=VALUE          the width of the image in pixels
  -t, --height=VALUE         the height of the image in pixels
```

# Examples #

![http://cumberland.googlecode.com/files/theming.png](http://cumberland.googlecode.com/files/theming.png)