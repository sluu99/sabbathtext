<?php
require_once "abbrev.php";

$fd = fopen("verses.csv", "r");
$count = 0;
$abbrev = new Abbrev;

while ($row = fgetcsv($fd)) {
    list($ref, $verse) = $row;
    if ($found = $abbrev->findLongestReference($ref, $verse)) {
        echo $found;
        $count++;
    }
}

echo "Found $count verses " . PHP_EOL;
