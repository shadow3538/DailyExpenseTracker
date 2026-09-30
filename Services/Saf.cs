using Android.App;
using Android.Content;
using Android.Provider;
using AUri = Android.Net.Uri;

namespace DailyExpenseTracker;

// Storage Access Framework
public static class Saf
{
    public const int ReqTree = 7101;
    public const int ReqCreate = 7102;

    static TaskCompletionSource<Intent?>? _tcs;

    // Called from MainActivity.OnActivityResult
    public static void Complete(Result res, Intent? data)
    {
        var t = _tcs;
        _tcs = null;
        t?.TrySetResult(res == Result.Ok ? data : null);
    }

    static Context Ctx => Microsoft.Maui.ApplicationModel.Platform.AppContext;

    static async Task<Intent?> LaunchAsync(Intent intent, int req)
    {
        var act = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        if (act == null) return null;
        _tcs = new TaskCompletionSource<Intent?>();
        act.StartActivityForResult(intent, req);
        return await _tcs.Task;
    }

    const ActivityFlags RwFlags = ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission;

    static void Persist(AUri uri)
    {
        try { Ctx.ContentResolver?.TakePersistableUriPermission(uri, RwFlags); }
        catch (Exception ex) { AppLog.Error("Saf.Persist", ex); }
    }

    // pick

    // Pick a folder
    public static async Task<string?> PickFolderAsync()
    {
        var i = new Intent(Intent.ActionOpenDocumentTree);
        i.AddFlags(RwFlags | ActivityFlags.GrantPersistableUriPermission | ActivityFlags.GrantPrefixUriPermission);
        var data = await LaunchAsync(i, ReqTree);
        var uri = data?.Data;
        if (uri == null) return null;
        Persist(uri);
        return uri.ToString();
    }

    // Pick or create
    public static async Task<string?> PickNewFileAsync(string name)
    {
        var i = new Intent(Intent.ActionCreateDocument);
        i.AddCategory(Intent.CategoryOpenable);
        i.SetType("application/json");
        i.PutExtra(Intent.ExtraTitle, name);
        i.AddFlags(RwFlags | ActivityFlags.GrantPersistableUriPermission);
        var data = await LaunchAsync(i, ReqCreate);
        var uri = data?.Data;
        if (uri == null) return null;
        Persist(uri);
        return uri.ToString();
    }

    // names

    // Provider name from
    public static string ProviderOf(string uriText)
    {
        try
        {
            var a = AUri.Parse(uriText)?.Authority ?? "";
            if (a.Contains("google.android.apps.docs")) return "Google Drive";
            if (a.Contains("externalstorage")) return L.T("ফোনের স্টোরেজ / SD", "Phone / SD storage");
            if (a.Contains("downloads")) return L.T("ডাউনলোড ফোল্ডার", "Downloads");
            if (a.Contains("dropbox")) return "Dropbox";
            if (a.Contains("onedrive") || a.Contains("skydrive")) return "OneDrive";
            return a;
        }
        catch { return ""; }
    }

    // Readable name of
    public static string LabelOf(string uriText, bool folder)
    {
        try
        {
            var uri = AUri.Parse(uriText);
            if (uri == null) return "";
            var cr = Ctx.ContentResolver;
            if (cr == null) return "";
            AUri q = uri;
            if (folder)
            {
                var id = DocumentsContract.GetTreeDocumentId(uri);
                q = DocumentsContract.BuildDocumentUriUsingTree(uri, id)!;
            }
            using var c = cr.Query(q, new[] { OpenableColumns.DisplayName }, null, null, null);
            if (c != null && c.MoveToFirst())
            {
                var n = c.GetString(0);
                if (!string.IsNullOrWhiteSpace(n)) return n;
            }
        }
        catch { }
        return "";
    }

    // write

    // Write into a
    public static async Task<(bool Ok, bool Verified)> WriteFileAsync(string uriText, byte[] bytes)
    {
        var uri = AUri.Parse(uriText);
        if (uri == null) return (false, false);
        return await WriteUriAsync(uri, bytes);
    }

    // Write (or overwrite)
    public static async Task<(bool Ok, bool Verified)> WriteInFolderAsync(string treeText, string fileName, byte[] bytes)
    {
        var tree = AUri.Parse(treeText);
        var cr = Ctx.ContentResolver;
        if (tree == null || cr == null) return (false, false);

        var treeId = DocumentsContract.GetTreeDocumentId(tree);
        AUri? target = null;

        // reuse same-name file
        var kids = DocumentsContract.BuildChildDocumentsUriUsingTree(tree, treeId);
        using (var c = cr.Query(kids!, new[] { DocumentsContract.Document.ColumnDocumentId, DocumentsContract.Document.ColumnDisplayName }, null, null, null))
        {
            while (c != null && c.MoveToNext())
            {
                if (c.GetString(1) == fileName)
                {
                    target = DocumentsContract.BuildDocumentUriUsingTree(tree, c.GetString(0)!);
                    break;
                }
            }
        }

        if (target == null)
        {
            var parent = DocumentsContract.BuildDocumentUriUsingTree(tree, treeId);
            target = DocumentsContract.CreateDocument(cr, parent!, "application/json", fileName);
        }
        if (target == null) return (false, false);
        return await WriteUriAsync(target, bytes);
    }

    static async Task<(bool Ok, bool Verified)> WriteUriAsync(AUri uri, byte[] bytes)
    {
        var cr = Ctx.ContentResolver;
        if (cr == null) return (false, false);

        using (var os = cr.OpenOutputStream(uri, "wt"))
        {
            if (os == null) return (false, false);
            await os.WriteAsync(bytes, 0, bytes.Length);
            await os.FlushAsync();
        }

        // check size on
        bool verified = false;
        try
        {
            using var c = cr.Query(uri, new[] { OpenableColumns.Size }, null, null, null);
            if (c != null && c.MoveToFirst() && !c.IsNull(0))
                verified = c.GetLong(0) == bytes.Length;
        }
        catch { }
        return (true, verified);
    }
}
