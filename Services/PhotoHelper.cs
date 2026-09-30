namespace DailyExpenseTracker;

// Shrink profile photo
public static class PhotoHelper
{
    const int MaxSide = 600;

    // Save resized copy
    public static async Task<string?> PrepareAsync(FileResult file)
    {
        var id = Guid.NewGuid().ToString("N");
        var raw = System.IO.Path.Combine(FileSystem.CacheDirectory, "pp_" + id + ".src");
        var dst = System.IO.Path.Combine(FileSystem.CacheDirectory, "pp_" + id + ".jpg");

        try
        {
            using (var src = await file.OpenReadAsync())
            using (var o = File.Create(raw))
                await src.CopyToAsync(o);

            return await Task.Run(() => Resize(raw, dst) ? dst : null);
        }
        catch
        {
            return null;
        }
        finally
        {
            try { File.Delete(raw); } catch { }
        }
    }

    static bool Resize(string srcPath, string dstPath)
    {
        // Read size first
        var bounds = new global::Android.Graphics.BitmapFactory.Options { InJustDecodeBounds = true };
        global::Android.Graphics.BitmapFactory.DecodeFile(srcPath, bounds);
        if (bounds.OutWidth <= 0 || bounds.OutHeight <= 0) return false;

        int sample = 1;
        while (Math.Max(bounds.OutWidth, bounds.OutHeight) / (sample * 2) >= MaxSide) sample *= 2;

        var opts = new global::Android.Graphics.BitmapFactory.Options { InSampleSize = sample };
        using var bmp = global::Android.Graphics.BitmapFactory.DecodeFile(srcPath, opts);
        if (bmp == null) return false;

        int degrees = 0;
        try
        {
            using var exif = new global::Android.Media.ExifInterface(srcPath);
            var o = exif.GetAttributeInt(global::Android.Media.ExifInterface.TagOrientation, 1);
            degrees = o == 6 ? 90 : o == 3 ? 180 : o == 8 ? 270 : 0;
        }
        catch { }

        float scale = Math.Min(1f, (float)MaxSide / Math.Max(bmp.Width, bmp.Height));
        using var m = new global::Android.Graphics.Matrix();
        m.PostScale(scale, scale);
        if (degrees != 0) m.PostRotate(degrees);

        using var res = global::Android.Graphics.Bitmap.CreateBitmap(bmp, 0, 0, bmp.Width, bmp.Height, m, true);
        if (res == null) return false;

        using var fs = File.Create(dstPath);
        return res.Compress(global::Android.Graphics.Bitmap.CompressFormat.Jpeg!, 85, fs);
    }
}
