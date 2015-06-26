# Saluse ComicReader
This is a WPF application for Windows for viewing digital comics.

Currently supports these file formats:
  .cbr
  .cbz
  .zip
  .rar
  .pdf (if Ghostscript is installed)
  
In addition, Saluse ComicReader will also open any folder with the following image types:
  .jpg | .jpeg
  .png
  .bmp
  .tif | .tiff

## Note

This is basically a prototyped application and the user interactions are designed for myself. It was written as a replacement
to CDisplayEx because I needed an application that could remember the last position of each comic. It grew from there but now that
I'm sharing the application, there will be many refactoring and redesign to be usable to a wider audience

## Usage

Drag and drop a supported file or any folder (with images) onto the application to start viewing.

## Key Commands

W: Toggle between Full and Windowed screen

R: Rotate between Portrait and Landscape mode *

Z: Toggle zoom between fitting the screen and filling screen *

I: Show/Hide information about the comic and current image

Right | Down | PageDown | Space: Next image

Left | Up | PageUp | Shift-Space: Previous image

Home: First image

End: Last image

N: Next comic/document/folder *

B: Previous comic/document/folder *


E: Next effect *

Shift-E: Previous effect *

Ctrl-E: Switch current effect off


S: Save current image *


Esc: Exit application
