<?php

function emitHeader()
{
?>
	<link rel="stylesheet" href="../jquery-treeview/jquery.treeview.css" />
	<link rel="stylesheet" href="../toc.css" />
	<script src="../jquery-treeview/lib/jquery.js" type="text/javascript"></script>
	<script src="../jquery-treeview/lib/jquery.cookie.js" type="text/javascript"></script>
	<script src="../jquery-treeview/jquery.treeview.js" type="text/javascript"></script>
	
	<script type="text/javascript">
		$(document).ready(function(){
			// first example
			$("#navigation").treeview({
				persist: "location",
				collapsed: true,
				unique: true
			});

		});
	</script>
<?php
}

function buildToc($dir = ".", $depth = 0, $target = "contentFrame", $start = "start.php")
{
	if ($depth == 0)
	{
		//echo "Cumberland Documentation<br />";
		echo "<ul id=\"navigation\">";
	}

	$ourDirList = @opendir($dir);
	while ($ourItem = readdir($ourDirList))
	{
		// filter out unwanted files
		$ext = substr($ourItem, strripos($ourItem, '.')+1, 3);
		if ($ourItem == "." || $ourItem == ".." || $ourItem == "index.html" || $ext == "php" || $ext == "css")
		{
			continue;
		}

		$ourPath = $dir . '/' . $ourItem;
		if (is_dir($ourPath))
		{
			echo "<li><a href=\"$ourPath\" target=\"$target\">$ourItem</a>";
			echo "<ul>";
			buildToc($ourPath, $depth+1);
			echo "</ul></li>";
		}
		else
		{
			echo "<li><a href=\"$ourPath\" target=\"$target\">";
			echo substr($ourItem, 0, strlen($ourItem)-5);
			echo "</a>";
		}
	}
	
	if ($depth == 0)
	{
		echo "</ul>";
	}
}

function getDirectory()
{
    	//get public directory structure eg "/top/second/third"
	$public_directory = dirname($_SERVER['PHP_SELF']);
    
	//place each directory into array
	$directory_array = explode('/', $public_directory);

	return $directory_array[sizeof($directory_array)-1];
} 

?>
