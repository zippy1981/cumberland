

# New Features #

  * Shapefile - support 'M' shapetypes (value is ignored)
  * Cascading styles - The Style class has had MinScale/MaxScale properties, but now they work as expected in that if a style is out of scale, it keeps looking.  Previously the scale properties were applied after the style was found.
  * added Style.UniqueElseFlag which can be used to style remaining features
  * label outlines have been improved on Microsoft .NET
  * added Layer.AllowDuplicateLabels.  Setting this to false will not draw a label string that has already been drawn for this Layer.

## Line and Ring Simplification ##

Style now has Simplify and SimplifyTolerance properties which allows the ability to simplify features which can have the effect of making them appear nicer when zoomed far out ([details](http://www.salmonsalvo.net/blog/?p=205)).

![http://cumberland.googlecode.com/files/poly_tolerance.png](http://cumberland.googlecode.com/files/poly_tolerance.png)

## switch to Well-Known Binary ##

Querying SQL Server and PostGIS previously used Well-Known Text.  This has been switched to Well-Known Binary.  The speed-up is substantial.  For example, take this PostGIS WKT Query:

```
select astext(the_geom) as the_geom , perimeter , name2 
from lakes 
where the_geom && 
SetSRID('BOX3D(1.74622982740402E-10 -6499.99999999873, 795000 788500.000000004)'::box3d, 3087)
```

It takes approx. 3 seconds.  Switch the 'astext' to 'asbinary', and the query takes ~500 milliseconds.  There is also a substantial improvement in SQL Server.  Parsing is also faster.  For the above query, parsing the WKT took about 5 seconds.  The WKB took 4.


# Fixes #

  * #7 - tilepyramider throws exception when placed in PATH on Windows and called elsewhere
  * #8 - OverflowException is thrown