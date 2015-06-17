

# Introduction #

This tutorial will go over a simple application built with cumberland.  This example will be a Windows Forms application which displays a map.

# Set up #

  1. Create a 'Windows Application' project in Visual Studio
  1. Download the cumberland library ([here](http://code.google.com/p/cumberland/downloads/list))
  1. Unzip
  1. Add a reference to 'Cumberland.dll' (and optionally 'Cumberland.Data.PostGIS.dll' or 'Cumberland.Data.SqlServer' if needed)
  1. copy proj.dll into your project and set 'Copy To Output Directory' to 'Copy if newer'

# Create the interface #

Let's keep it simple for now and just drag a PictureBox and Button onto our window.  I name the button "Draw Map", and double-click it to create the event.

# The code-behind #

First, we add a class variable for the map:

```
    public partial class Form1 : Form
    {
        Map m = new Map();
        ...
```

Next, we handle the button click to draw the map and set the picture box image to it:

```
        private void button1_Click(object sender, EventArgs e)
        {
            m.Width = pictureBox1.Width;
            m.Height = pictureBox1.Height;

            MapDrawer drawer = new MapDrawer();
            pictureBox1.Image = drawer.Draw(m);
        }
```

At this point, we can run our app and click the button, but nothing happens as there is nothing in the map to draw.  We need some data.

# Adding Data #

For this example, I have grabbed some data from the state of Massachusettes.  Let's start with the [counties](http://www.mass.gov/mgis/counties.htm).  Download this, execute, and it contains multiple files.  We want the 'COUNTIES\_POLY.shp'.  Now, we can add it to our map as so:

```
        public Form1()
        {
            InitializeComponent();

            // load the shapefile
            Shapefile shp = new Shapefile(@"C:\Documents and Settings\Scott\My Documents\mass\counties\COUNTIES_POLY.shp");

            // create our layer
            Layer l = new Layer();

            // set its data source to our shapefile
            l.Data = shp;

            // provide a default style
            l.Styles.Add(new Style());

            // add our layer to the map so it will get rendered            
            m.Layers.Add(l);

            // set the map extents to the layer extents so it is in view
            m.Extents = shp.Extents;
        }
```

Which, when run, produces:

http://cumberland.googlecode.com/files/CreateASimpleApp-fig1.PNG