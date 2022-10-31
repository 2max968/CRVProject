using CRVProject.Ortsschild;
using CRVProject.Helper;
using OpenCvSharp;

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
    
    locator.RunLocator();

    ImageGridWindow wnd = new ImageGridWindow(3, 1 + (locator.Ortsschilder.Count + 2 / 3));
    wnd.SetImage(0, 0, image);
    wnd.SetImage(1,0,locator.BinarizedImage);
    for (int i = 0; i < locator.Ortsschilder.Count; i++)
        wnd.SetImage(i % 3, i / 3 + 1, locator.Ortsschilder[i]);
    wnd.Run();
}