<?php
/**
 * Finds the longest reference that conforms to the character limit, given a verse and a standard reference
 * (dumbspeak: function that gives preference to "John" instead of "Jn" whenever we have space to display it)
 *
 */
function abbrev_finder($reference,  $verse, $fmt_string  = "Happy Sabbath!\r\n\"%s\" -- %s", $out_fmt = '{ "%s", "%s" },',$limit = 160) {
    $books = [
        "Amos"=> ["Amos", "Am"],
        "1 Chronicles"=> ["1 Chron.", "1 Chr"],
        "2 Chronicles"=> ["2 Chron.", "2 Chr"],
        "Daniel"=> ["Dan.", "Dn"],
        "Deuteronomy"=> ["Deut.", "Dt"],
        "Ecclesiastes"=> ["Eccles.", "Eccl"],
        "Esther"=> ["Esther", "Est"],
        "Exodus"=> ["Exod.", "Ex"],
        "Ezekiel"=> ["Ezek.", "Ez"],
        "Ezra"=> ["Ezra", "Ezr"],
        "Genesis"=> ["Gen.", "Gn"],
        "Habakkuk"=> ["Hab.", "Hb"],
        "Haggai"=> ["Hag.", "Hg"],
        "Hosea"=> ["Hosea", "Hos"],
        "Isaiah"=> ["Isa.", "Is"],
        "Jeremiah"=> ["Jer.", "Jer"],
        "Job"=> ["Job", "Jb"],
        "Joel"=> ["Joel", "Jl"],
        "Jonah"=> ["Jon.", "Jon"],
        "Joshua"=> ["Josh.", "Jo"],
        "Judges"=> ["Judg.", "Jgs"],
        "1 Kings"=> ["1 Kings", "1 Kgs"],
        "2 Kings"=> ["2 Kings", "2 Kgs"],
        "Lamentations"=> ["Lam.", "Lam"],
        "Leviticus"=> ["Lev.", "Lv"],
        "Malachi"=> ["Mal.", "Mal"],
        "Micah"=> ["Mic.", "Mi"],
        "Nahum"=> ["Nah.", "Na"],
        "Nehemiah"=> ["Neh.", "Neh"],
        "Numbers"=> ["Num.", "Nm"],
        "Obadiah"=> ["Obad.", "Ob"],
        "Proverbs"=> ["Prov.", "Prv"],
        "Psalms" => ["Ps." , "Ps"],
        "Ruth"=> ["Ruth", "Ru"],
        "1 Samuel"=> ["1 Sam.", "1 Sm"],
        "2 Samuel"=> ["2 Sam.", "2 Sm"],
        "Song of Solomon"=> ["Song of Songs", "Sol.", "Sg"],
        "Zechariah"=> ["Zech.", "Zec"],
        "Zephaniah"=> ["Zeph.", "Zep"],
        // NT
        "Acts" => ["Acts"],
        "Revelation" => ["Rev.", "Rev", "Rv"],
        "Colossians"=> ["Col.", "Col"],
        "1 Corinthians"=> ["1 Cor.", "1 Cor"],
        "2 Corinthians"=> ["2 Cor.", "2 Cor"],
        "Ephesians"=> ["Eph.", "Eph"],
        "Galatians"=> ["Gal.", "Gal"],
        "Hebrews"=> ["Heb.", "Heb"],
        "James"=> ["James", "Jas"],
        "John"=> ["John", "Jn"],
        "1 John"=> ["1 John", "1 Jn"],
        "2 John"=> ["2 John", "2 Jn"],
        "3 John"=> ["3 John", "3 Jn"],
        "Jude"=>["Jude"],
        "Luke"=> ["Luke", "Lk"],
        "Mark"=> ["Mark", "Mk"],
        "Matthew"=> ["Matt.", "Mt"],
        "1 Peter"=> ["1 Pet.", "1 Pt"],
        "2 Peter"=> ["2 Pet.", "2 Pt"],
        "Philemon"=> ["Philem.", "Phlm"],
        "Philippians"=> ["Phil.", "Phil"],
        "Romans"=> ["Rom.", "Rom"],
        "1 Thessalonians"=> ["1 Thess.", "1 Thes"],
        "2 Thessalonians"=> ["2 Thess.", "2 Thes"],
        "1 Timothy"=> ["1 Tim.", "1 Tm"],
        "2 Timothy"=> ["2 Tim.", "2 Tm"],
        "Titus"=> ["Titus", "Ti"],
        ];
    // To sort references by their length
    $sort_fuction = function($a, $b) { return strlen($a) <= strlen($b); };
    $reverse_array = [];
    // Populate index that gets us from Jn to "John"
    foreach($books as $b => $abbrevs) {
        foreach($abbrevs as $abbrev) {
            // Ignore dots for index
            $reverse_array[join(explode(".", $abbrev))] = $b;
            $reverse_array[$abbrev] = $b;
        }
    }

    $reference_parts = explode(" ", $reference);
    $reference_parts_reversed = array_reverse($reference_parts);
    // Drop off the chapter:verse off the array
    $chapter_versenum = array_shift($reference_parts_reversed);
    $book = rtrim(join(" ",array_reverse($reference_parts_reversed)));
    $long_bookname = $reverse_array[$book];
    // Append the fullname of the book
    array_unshift($books[$long_bookname], $long_bookname);
    usort($books[$long_bookname], $sort_fuction);
    foreach($books[$long_bookname] as $fullname => $abbrev) {
        $ref_with_number = $abbrev . " " . $chapter_versenum;
        $text_msg = sprintf($fmt_string, $verse, $ref_with_number);
        if(strlen($text_msg) <= $limit) {
            return sprintf($out_fmt, $verse, $ref_with_number) . PHP_EOL;
        }
    }
    return false;
}
