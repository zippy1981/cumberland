

# Introduction #

Symbology in this context means the study of the symbols that represent cartographic features.  Cumberland currently provides a standard set of style options for creating effective maps.

All styling is done in the Style element which is a property of the Layer element.

# Common Properties #

These apply to all feature types:

```
<MaxScale>.013</MaxScale>
<MinScale>.00001</MinScale>
<Id>SmallTowns</Id>
```

MinScale/MaxScale can be set to prevent a style from being drawn at certain scales (map units per pixels).  Id is used by the kml exporter.

# Point styles #

Currently, cumberland supports a limited set of symbology for points.  There are shapes, currently squares and rectangles, or external images.  To use shapes, set the following properties:

```
...
<PointSize>5</PointSize>
<PointSymbol>Shape</PointSymbol>
<PointSymbolShape>Square</PointSymbolShape>
```

The point size is the width/height of the square or the diameter of the circle.  The PointSymbolShape can be Square,Circle, or None.  The fill color, line styles for shape point symbols is inherited from polygons.  The PointSymbol element define the type of point symbol.  For external images, it is set to 'Image':

```
<PointSymbol>Image</PointSymbol>
<PointSymbolImagePath>images/city_icon.png</PointSymbolImagePath>
```

Note the relative path, this is interpreted as relative to the map xml file.  This allows the images to be bundled with the map.

A example of point symbols is found below.

# Line styles #

Lines in cumberland are styled with the following properties:

```
<Style>
	<LineWidth>1</LineWidth>
	<LineStyle>Solid</LineStyle>
	<LineColor>255,165,165,165</LineColor>
</Style>
```

LineWidth is the width of the line in pixels, LineColor is the color of the line in format 'Alpha,Red,Green,Blue'.  LineStyle is the style of line.  It can be Solid, Dashed, Dotted, or None:

![http://cumberland.googlecode.com/files/Cumberland.LineStyle.png](http://cumberland.googlecode.com/files/Cumberland.LineStyle.png)

(LineStyle.None is not shown, as it would not draw the line.)

# Polygon styles #

Polygon styles inherit the line styles for drawing the polygon outline.  Additionally, there is a FillStyle property which can be set to:

  * None - The polygon will not be filled
  * Solid - The polygon will be filled with a solid color as defined by FillColor (format is 'A,R,G,B' just as LineColor).
  * Texture - The polygon will be filled with a texture as defined by FillTexturePath.  This must point to an image file.

# Labelling #

_Labelling is new to Cumberland 0.2_

Cumberland provides a set of properties for configuring how you want labels to look.  To turn labelling on for a style, you set ShowLabels:

```
<ShowLabels>true</ShowLabels>
```

minimum and maximum scales can be set the same as layers and styles:

```
<LabelMaxScale>.008</LabelMaxScale>
<LabelMinScale>.0000001</LabelMinScale>
```

The font style can be configured:

```
<LabelFont>SansSerif</LabelFont>
<LabelColor>155,105,105,255</LabelColor>
<LabelFontEmSize>8</LabelFontEmSize>
```

If LabelFont is set to 'Custom', you can set the LabelCustomFont property to point a font on your system.

```
<LabelCustomFont>Arial</LabelCustomFont>
```

You can also add an outline to your label:

```
<LabelDecoration>Outline</LabelDecoration>
<LabelOutlineWidth>1</LabelOutlineWidth>
<LabelOutlineColor>255,255,0,0</LabelOutlineColor>
```

Finally, you have some control over the position of labels.  You have the ability to control where the location of the label in relation to the point of origin ([options](http://salmonsalvo.net/cumberland/docs/0.2/Cumberland/Cumberland/LabelPosition.html)) and how far away from the point it is:

```
<LabelPosition>Top</LabelPosition>
<LabelPixelOffset>1</LabelPixelOffset>
```

Also, for point features, you can set the angle of the label:

```
<LabelAngle>45</LabelAngle>
```

## Example ##

The following provides some examples of what is possible:

![http://cumberland.googlecode.com/files/labelling-samples.png](http://cumberland.googlecode.com/files/labelling-samples.png)

# Thematic mapping #

Thematic mapping allows using particular styles based on a feature's properties.  For example, drawing states based on their population.  This must be configured in the [Layer element](mapXmlFormat#Layer_Elements.md).  The field that we test in the feature is specified by Layer->ThemeField.  The type of thematic mapping is specified in Layer->Theme.  Currently, there are two supported types of themes:

  1. Unique - the field value must equal the value specified in the style:
```
    <UniqueThemeValue>Rio Grande de Santiago</UniqueThemeValue>
```
  1. NumericRange - the (numeric) field value must fall within the numeric range specified in the style:
```
    <MinRangeThemeValue>2000001</MinRangeThemeValue>
    <MaxRangeThemeValue>100000000</MaxRangeThemeValue>			
```

If a feature's field value does not match any of the provided styles, it will not be drawn.  Also, Layer->Theme.None disables theming and is the same as not providing the element.

# Example #

Below is a rendered map which demonstrates the symbology described above.  The map for this is available in the source [here](http://code.google.com/p/cumberland/source/browse/trunk/cumberland/Cumberland.Tests/maps/mexico.xml).

![http://cumberland.googlecode.com/files/theming.png](http://cumberland.googlecode.com/files/theming.png)