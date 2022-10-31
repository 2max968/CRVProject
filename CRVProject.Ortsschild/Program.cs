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
    .Select(fi => fi.Name)
    .ToArray();

while (true)
{
    int selectedImage = Util.SelectGUI(true, images);
    if (selectedImage < 0)
        return 0;

    using var image = Cv2.ImRead(images[selectedImage]);
    using var locator = new Locator(image);
    
    locator.RunLocator();

    ImageGridWindow wnd = new ImageGridWindow(3, 2);
    wnd.SetImage(0, 0, image);
    wnd.SetImage(1,0,locator.BinarizedImage);
    wnd.SetImage(0,1,locator.Ortsschilder.Count > 0 ? locator.Ortsschilder[0] : null);
    wnd.Run();
}