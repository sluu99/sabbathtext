<?php

class Abbrev {
    /**
     * @var array Supported abbreviations. Order is irrelevant
     */
    public static $book_to_abbrevs = [
        "Amos"            => ["Amos", "Am"],
        "1 Chronicles"    => ["1 Chron.", "1 Chr"],
        "2 Chronicles"    => ["2 Chron.", "2 Chr"],
        "Daniel"          => ["Dan.", "Dn"],
        "Deuteronomy"     => ["Deut.", "Dt"],
        "Ecclesiastes"    => ["Eccles.", "Eccl"],
        "Esther"          => ["Esther", "Est"],
        "Exodus"          => ["Exod.", "Ex"],
        "Ezekiel"         => ["Ezek.", "Ez"],
        "Ezra"            => ["Ezra", "Ezr"],
        "Genesis"         => ["Gen.", "Gn"],
        "Habakkuk"        => ["Hab.", "Hb"],
        "Haggai"          => ["Hag.", "Hg"],
        "Hosea"           => ["Hosea", "Hos"],
        "Isaiah"          => ["Isa.", "Is"],
        "Jeremiah"        => ["Jer.", "Jer"],
        "Job"             => ["Job", "Jb"],
        "Joel"            => ["Joel", "Jl"],
        "Jonah"           => ["Jon.", "Jon"],
        "Joshua"          => ["Josh.", "Jo"],
        "Judges"          => ["Judg.", "Jgs"],
        "1 Kings"         => ["1 Kings", "1 Kgs"],
        "2 Kings"         => ["2 Kings", "2 Kgs"],
        "Lamentations"    => ["Lam.", "Lam"],
        "Leviticus"       => ["Lev.", "Lv"],
        "Malachi"         => ["Mal.", "Mal"],
        "Micah"           => ["Mic.", "Mi"],
        "Nahum"           => ["Nah.", "Na"],
        "Nehemiah"        => ["Neh.", "Neh"],
        "Numbers"         => ["Num.", "Nm"],
        "Obadiah"         => ["Obad.", "Ob"],
        "Proverbs"        => ["Prov.", "Prv"],
        "Psalms"          => ["Ps.", "Ps"],
        "Ruth"            => ["Ruth", "Ru"],
        "1 Samuel"        => ["1 Sam.", "1 Sm"],
        "2 Samuel"        => ["2 Sam.", "2 Sm"],
        "Song of Solomon" => ["Song of Songs", "Sol.", "Sg"],
        "Zechariah"       => ["Zech.", "Zec"],
        "Zephaniah"       => ["Zeph.", "Zep"],
        "Acts"            => ["Acts"],
        "Revelation"      => ["Rev.", "Rev", "Rv"],
        "Colossians"      => ["Col.", "Col"],
        "1 Corinthians"   => ["1 Cor.", "1 Cor"],
        "2 Corinthians"   => ["2 Cor.", "2 Cor"],
        "Ephesians"       => ["Eph.", "Eph"],
        "Galatians"       => ["Gal.", "Gal"],
        "Hebrews"         => ["Heb.", "Heb"],
        "James"           => ["James", "Jas"],
        "John"            => ["John", "Jn"],
        "1 John"          => ["1 John", "1 Jn"],
        "2 John"          => ["2 John", "2 Jn"],
        "3 John"          => ["3 John", "3 Jn"],
        "Jude"            => ["Jude"],
        "Luke"            => ["Luke", "Lk"],
        "Mark"            => ["Mark", "Mk"],
        "Matthew"         => ["Matt.", "Mt"],
        "1 Peter"         => ["1 Pet.", "1 Pt"],
        "2 Peter"         => ["2 Pet.", "2 Pt"],
        "Philemon"        => ["Philem.", "Phlm"],
        "Philippians"     => ["Phil.", "Phil"],
        "Romans"          => ["Rom.", "Rom"],
        "1 Thessalonians" => ["1 Thess.", "1 Thes"],
        "2 Thessalonians" => ["2 Thess.", "2 Thes"],
        "1 Timothy"       => ["1 Tim.", "1 Tm"],
        "2 Timothy"       => ["2 Tim.", "2 Tm"],
        "Titus"           => ["Titus", "Ti"],
    ];

    /**
     * Finds the longest reference that conforms to the character limit, given a verse and a standard reference
     * (method that gives preference to "John" instead of "Jn" whenever we have enough space to display it)
     * @param string $reference A bible reference eg "Jn 3:16". Valid book abbreviations are listed in the books
     * @param string $verse A bible verse eg "For God so loved the world ..."
     * @param string $fmt_string A sprintf() format string that represents the text message to be sent
     * @param string $fmt_out A sprintf() format string that is used in the return value of the function
     * @param int $limit The character limit for the output
     * @return string The verse and the longest reference formatted with $fmt_out
     */
    public static function findLongestReference($reference, $verse, $fmt_string = "Happy Sabbath!\r\n\"%s\" -- %s", $fmt_out = '{ "%s", "%s" },', $limit = 160) {
        $reverse_array = [];
        foreach (self::$book_to_abbrevs as $b => $abbrevs) {
            foreach ($abbrevs as $abbrev) {
                // Ignore dots for index
                $reverse_array[implode(explode(".", $abbrev))] = $b;
                $reverse_array[$abbrev] = $b;
            }
        }
        $reference_parts = explode(" ", $reference);
        $reference_parts_reversed = array_reverse($reference_parts);

        // Drop off the chapter:verse off the array
        $chapter_versenum = array_shift($reference_parts_reversed);
        $book = rtrim(join(" ", array_reverse($reference_parts_reversed)));
        $long_bookname = $reverse_array[$book];

        // Find the longest reference first
        usort(self::$book_to_abbrevs[$long_bookname], function ($a, $b) {
            return strlen($a) <= strlen($b);
        });

        // Append the full name of the book
        if (reset(self::$book_to_abbrevs[$long_bookname]) != $long_bookname) {
            array_unshift(self::$book_to_abbrevs[$long_bookname], $long_bookname);
        }

        foreach (self::$book_to_abbrevs[$long_bookname] as $abbrev) {
            $ref_with_number = $abbrev . " " . $chapter_versenum;
            $text_msg = sprintf($fmt_string, $verse, $ref_with_number);
            if (strlen($text_msg) <= $limit) {
                return sprintf($fmt_out, $verse, $ref_with_number) . PHP_EOL;
            }
        }

        return false;
    }
}
