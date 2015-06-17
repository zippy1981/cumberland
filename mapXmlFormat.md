

# Introduction #

Cumberland provides an xml format that you can use to store and load maps.  It details such information as projection, layer data sources, symbology, and so forth.  Users may find it simpler to create their maps through xml, instead of through the API.  Additionally, the [drawmap](drawmap.md) and [tilepyramider](tilepyramider.md) command line tools requires this as input.

The map xml format is not a strict schema, in that any missing or out of place element will not cause it to fail.  It just looks for certain elements and ignores the rest.

# Xml Document structure #

## The Map Element ##

The xml document should be prefaced as usual and have a 'Map' element at the root level:

```
<?xml version="1.0" encoding="utf-8"?>
<Map>
...
```

This element supports several child elements:

```
<Width>400</Width>
<Height>400</Height>
<Extents>-120,12,-82,34</Extents>
<Projection>+init=epsg:4326</Projection>
<Layers>
...
</Layers>
```

The Width/Height set the size of the image to draw in pixels.  The Projection defines the output projection of the map.  The extents define the portion of the map to draw.  Layers can hold zero to many 'Layer' elements.

### New to 0.2 ###

The BackgroundColor property has been added, with which you can set the background color of the map:

```
<BackgroundColor>255,255,255,255</BackgroundColor>
```

## The Layers Element ##

The Layers element can have zero to many Layer elements.  A layer encompasses a single data source along with symbology information which tells the map drawer how to render it.  It supports the following child elements:

```
<Layer>
    <Id>states</Id>
    <Projection>+init=epsg:4326</Projection>
    <Theme>NumericRange</Theme>
    <ThemeField>AREA</ThemeField>
    <Data ...>
        ...
    </Data>
    <Styles>
        ...
    </Styles>
</Layer>
```

The Id element is simply a holder you can put a name or unique id in for your own reference.  The Projection element holds the coordinate system of the underlying data.  The Theme and ThemeField are used for theming.  The 'Theme' specifies the type of theming to do, and the 'ThemeField' specifies the field in the underlying data source to do the theming on.

### New to 0.2 ###

Several new elements have been added to Layer:

```
<LabelField>ROUTE</LabelField>
<MaxScale>.013</MaxScale>
<MinScale>.01</MinScale>
<Visible>false</Visible>
```

LabelField is the field in the feature source to use for labels, MaxScale/MinScale can be used to control the maximum/minimum scale (in map unit per pixel) that this layer is visible at, and Visible can be used to turn off a layer.

## The 'Data' Element ##

This element stores information about the feature source.  It is extendable in that new feature source types can be created and references to their data stored.  To let cumberland know the type, there are two attributes that must be set:

  * sourceType - specifies the type of feature source this is (database, file)
  * sourceInstance - specifies the actual feature source type (PostGIS, shapefile)

The inner content within this element is dependent on it's sourceType attribute.  For example a shapefile data element looks as so:

```
<Data sourceType="Cumberland.Data.IFileFeatureSource" sourceInstance="Cumberland.Data.Shapefile.Shapefile">
    <FilePath>../shape_eg_data/mexico/states.shp</FilePath>
</Data>
```

Note the relative path to the shapefile.  If a relative path is used, it is expected to be relative to the map xml file path.  This can be used for bundling up resources without having to worry about mangled file paths.

A database data element looks like this:

```
<Data sourceType="Cumberland.Data.IDatabaseFeatureSource" sourceInstance="Cumberland.Data.PostGIS.PostGISFeatureSource">
    <ConnectionString>Server=192.168.1.107;Port=5432;User Id=pguser;Password=pgpublic;Database=florida;</ConnectionString>
    <TableName>springs_fdep_2000</TableName>
</Data>
```

### Subqueries on database feature sources ###

There are other optional parameters for database feature sources that can be used for doing subqueries.  An Example:

```
<Data sourceType="Cumberland.Data.IDatabaseFeatureSource" sourceInstance="Cumberland.Data.PostGIS.PostGISFeatureSource">
         <ConnectionString>Server=localhost;Port=5432;User Id=pguser;Password=pgpublic;Database=florida;</ConnectionString>
         <TableName>(select * from fdot_majroads where usroute = 'I    10') as interstates </TableName>
         <ForcedSrid>3086</ForcedSrid>
         <ForcedFeatureType>Polyline</ForcedFeatureType>
         <ForcedGeometryColumn>the_geom</ForcedGeometryColumn>
         <ForcedSpatialType>geography</ForcedSpatialType>
</Data>
```

The Forced properties all tell cumberland what kind of data it is working with.

### Inline features (a.k.a. acetate layers) ###

Additionally, you can use the SimpleFeatureSource class to create an inline feature source ([example here](http://code.google.com/p/cumberland/source/browse/trunk/cumberland/Cumberland.Tests/maps/simplefeatures.xml)).

## The Styles element ##

The 'Styles' element, within the 'Layer' element can have zero to many 'Style' elements.  These affect how the features in the feature source are drawn.  If there is no Layer.Theme element or it is set to 'None', then the first Style element is used.  If theming is enabled, then the style element that is used depends on if the feature matches parameters specified within the style.  If there is no matching style for the feature, the feature is not drawn.

The style element is covered thoroughly in the [basic symbology](BasicSymbology.md) page.

# Examples #

That's the basics of the map xml format.  Several example maps are stored in the source:

http://code.google.com/p/cumberland/source/browse/#svn/trunk/cumberland/Cumberland.Tests/maps