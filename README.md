# TheElvenScrolls

TheElvenScrolls is a multiplatform ASCII letter creation tool. From a given text and a given template application generates justified text wrapped in that template.

# Create scroll

To create a scroll there must be a valid template and text files specified in application settings. Simply executing the program will generate ready scroll under output path. It is possible to replace input text with output by specifying the same file for input and output in application settings.

# Create template

Templates are expected to have exactly three parts: Begin, Middle and End. Each part has some fillable space defined, where the justified text will be written. Begin part is filled first, then End, then zero or more Middle parts.

Currently only templates with a __constant__ (or empty - in case of Begin and End parts) fillable space width across all parts are supported.

## Template format

First two lines define fill and blank character. The former defines space in the template to be replaced by text and the latter is a character that will be used to replace fill characters in the output file. Template parts come after config. Each template part is separated by a chosen separator.

To create template:
1. Open new text file.
2. Specify Fill and Blank characters in the first two lines, according to _fillConfigPattern_ and _blankConfigPattern_ conventions.
3. Mark end of config with _partSeparatorPattern_ string.
4. Add Begin part. It may, but does not have to contain fillable space.
5. Mark end of Begin part with _partSeparatorPattern_ string.
6. Add Middle part. It __must__ contain fill characters, defined right after _fillConfigPattern_.
7. Mark end of Middle part with _partSeparatorPattern_ string.
8. Add End part. I may, but does not have to contain fillable space.
9. Save template file as _templatePath_.
10. Provide input and run the program.

Sample template:

```
Fill=+
Blank= 
<NEXT>
 ______________________________________________________________
| ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ |
<NEXT>
| ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ |
<NEXT>
| ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ |
 ---------------------------------------------------------------
```

# Settings

Settings can be modified directly in appsettings.json file, but it is preferable to override settings in appsettings.user.json file, created on program first exit.

## Description

### Default Files Settings

These settings are used if no other settings are specified.

* __template__: Input template file path. File must exist and contain a valid template.
* __input__: Input raw text path file that will be justified and inserted into template.
* __output__: Scroll output file. It may be te same as _inputPath_ to overwrite raw input with scroll.

### Directory Settings

Items in Menu are searched in directories defined inside this section.

* __template__: Templates folder path.
* __input__: Input text files folder path.
* __output__: Output folder path.

### Template File Settings
* __fillConfigPattern__: Pattern specified in first or second line of the template file. Character right after this pattern will mark template fillable space.
* __blankConfigPattern__: Pattern specified in first or second line of the template file. Character right after this pattern is what fill character will be converted to in the output file.
* __partSeparatorPattern__: Pattern that separates template config from Begin part, Begin part from Middle part and Middle from End part.

### Justifier Settings
* __endingThresholdPercent__: Defines percent of last line of a paragraph filled with text, above which the line will be justified.
* __pauseAfterLongWords__: Whether _pause_ should be inserted at long word (longer than a single line width) break point.
* __indentParagraphs__: Whether paragraphs should be indented.
* __breakOnlyOnEmptyLines__: Whether every line break will be new line in the output or only empty new lines.
* __paragraph__: String to indent paragraphs with, if _indentParagraphs_ is true.
* __pause__: String to write at the end of the line for long words, if _pauseAfterLongWords_ is true.
* __excludedPunctuations__: Characters that are punctuations but should not be followed by double space in justification process.

## Format

### Default Files Settings
* __template__: string, default &quot;Scrolls\\Default\\sample_template.txt&quot;
* __input__: string, default &quot;Scrolls\\Default\\sample_input.txt&quot;
* __output__: string, default &quot;Scrolls\\Default\\output_scroll.txt&quot;

### DirectorySettings
* __template__: string, default &quot;Scrolls\\Templates&quot;
* __input__: string, default &quot;Scrolls\\Input&quot;
* __output__: string, default &quot;Scrolls\\Output&quot;

### Template File Settings
* __fillConfigPattern__: string, default &quot;Fill=&quot;
* __blankConfigPattern__: string, default &quot;Blank=&quot;
* __partSeparatorPattern__: string, default "&lt;NEXT&gt;"

### Justifier Settings
* __endingThresholdPercent__: double [0; 1], default 0.80
* __pauseAfterLongWords__: bool, default true
* __indentParagraphs__: bool, default true
* __breakOnlyOnEmptyLines__: bool, default true
* __paragraph__: string, default &quot;&nbsp;&nbsp;&nbsp;&quot;
* __pause__: string, default &quot;-&quot;
* __excludedPunctuations__: List<char>, default [ &quot;-&quot; ]