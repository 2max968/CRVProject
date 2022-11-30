﻿using CRVProject.Ortsschild;
using CRVProject.Helper;
using OpenCvSharp;
using System.Diagnostics;

Configuration.LoadConfiguration();

DirectoryInfo imageDirectory = new DirectoryInfo("Images");
if (!imageDirectory.Exists)
{
    Console.WriteLine("Cant find input directory 'Images'");
    return 1;
}

var images = imageDirectory.GetFiles()
    .Where(fi => Util.SupportedImageTypes
        .Select(type => fi.Name.ToLower().EndsWith($".{type}"))
        .Contains(true))
    .ToArray();

while (true)
{
    int selectedImage = Util.SelectGUI(true, images.Select(fi=>fi.Name).ToArray());
    if (selectedImage < 0)
        return 0;

    using var image = Cv2.ImRead(images[selectedImage].FullName);
    using var locator = new Locator(image);
    Util.PixelInfoWindow(image);

    locator.RunLocator();

    if(locator.CutoutImage != null)
        Cv2.ImWrite("cutout.png", locator.CutoutImage);

    ImageGridWindow wnd = new ImageGridWindow(3, 1 + (locator.Ortsschilder.Count + 2 / 3));
    wnd.SetImage(0, 0, image);
    wnd.SetImage(1,0,locator.BinarizedImage);
    wnd.SetImage(2, 0, locator.Corners);
    for (int i = 0; i < locator.Ortsschilder.Count; i++)
        wnd.SetImage(i % 3, i / 3 + 1, locator.Ortsschilder[i]);
    wnd.Run();
}
